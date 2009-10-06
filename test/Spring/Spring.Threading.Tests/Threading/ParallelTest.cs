﻿using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Spring.Threading.AtomicTypes;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Execution;

namespace Spring.Threading
{
    /// <summary>
    /// Test cases for <see cref="Parallel{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [TestFixture(typeof(int))] 
    [TestFixture(typeof(string))] 
    public class ParallelTest<T> : ThreadingTestFixture<T>
    {
        private class ManagedThreadLimiter
        {
            protected readonly TestThreadManager _manager;
            public int Threshold = int.MaxValue;
            public readonly AtomicInteger ThreadCount = new AtomicInteger();

            protected ManagedThreadLimiter(TestThreadManager manager) 
            {
                _manager = manager;
            }

            protected int NextThreadId(bool noException)
            {
                if (noException || ThreadCount.Value < Threshold)
                {
                    var next = ThreadCount.IncrementValueAndReturn();
                    if (next <= Threshold) return next;
                    if(noException) return -next;
                }
                throw new RejectedExecutionException();
            }
        }

        private class ManagedThreadFactory : ManagedThreadLimiter, IThreadFactory
        {
            public ManagedThreadFactory(TestThreadManager manager) : base(manager)
            {
            }

            public Thread NewThread(IRunnable runnable)
            {
                return _manager.NewVerifiableThread(runnable.Run, "T" + NextThreadId(false));
            }
        }

        private class ThreadManagerExecutor : ManagedThreadLimiter, IExecutor
        {
            public TimeSpan Delay { get; set; }

            public ThreadManagerExecutor(TestThreadManager manager) : base(manager)
            {
            }

            public void Execute(IRunnable command)
            {
                Execute(command.Run);
            }

            public void Execute(Action action)
            {
                var useDelay = Delay > TimeSpan.Zero;
                var tid = NextThreadId(useDelay);
                ThreadStart ts;
                if (useDelay && tid < 0)
                    ts = delegate
                             {
                                 Thread.Sleep(Delay);
                                 action();
                             };
                else ts = () => action();
                _manager.StartAndAssertRegistered("T" + tid, ts);
            }
        }

        private int _sampleSize;
        private const int _parallelism = 5;
        private ThreadManagerExecutor _executor;
        private Parallel<T> _parallel;

        [SetUp] public void SetUp()
        {
            _sampleSize = 20;
            _executor = new ThreadManagerExecutor(ThreadManager);
            _parallel = new Parallel<T>(_executor);
        }

        [Test] public void ConstructorChokesOnNullExecutor()
        {
            var e = Assert.Throws<ArgumentNullException>(
                ()=>new Parallel<T>(null));
            Assert.That(e.ParamName, Is.EqualTo("executor"));
        }

        [Test] public void ForEachChokesOnNullSource()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => _parallel.ForEach(null, 2, t=>{}));
            Assert.That(e.ParamName, Is.EqualTo("source"));
            e = Assert.Throws<ArgumentNullException>(
                () => _parallel.ForEach(null, new ParallelOptions(), t => { }));
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [Test] public void ForEachChokesOnNullBody()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => _parallel.ForEach(new T[0], 2, null));
            Assert.That(e.ParamName, Is.EqualTo("body"));
            e = Assert.Throws<ArgumentNullException>(
                () => _parallel.ForEach(new T[0], new ParallelOptions(), null));
            Assert.That(e.ParamName, Is.EqualTo("body"));
        }

        [Test] public void ForEachChokesOnNullParallelOptions()
        {
            var e = Assert.Throws<ArgumentNullException>(
                () => _parallel.ForEach(new T[0], null, t => { }));
            Assert.That(e.ParamName, Is.EqualTo("parallelOptions"));
        }

        [Test] public void ForEachReturnsImmediatelyOnEmptySource()
        {
            List<T> results = new List<T>();
            _parallel.ForEach(new T[0], new ParallelOptions(),
                t => { lock (results) results.Add(t); });
            Assert.That(results, Is.Empty);
            Assert.That(_executor.ThreadCount.Value, Is.EqualTo(0));
        }

        [Test] public void ForEachUseCurrentThreadWhenParallelistIsOne()
        {
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> results = new List<T>(_sampleSize);
            _parallel.ForEach(sources, 1,
                t => { Thread.Sleep(10); lock (results) results.Add(t); });
            Assert.That(results, Is.EquivalentTo(sources));
            Assert.That(_executor.ThreadCount.Value, Is.EqualTo(0));
        }

        [Test] public void ForEachCompletesAllFaskTasks()
        {
            _sampleSize = 200;
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> results = new List<T>(_sampleSize);
            _parallel.ForEach(sources, new ParallelOptions{MaxDegreeOfParallelism = _parallelism}, 
                t => { lock (results) results.Add(t); });
            Assert.That(results, Is.EquivalentTo(sources));
            Assert.That(_executor.ThreadCount.Value, Is.LessThanOrEqualTo(_parallelism));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachCompletesAllSlowTasks([Values(int.MaxValue, 3, 1, 0)] int maxThread)
        {
            _executor.Threshold = maxThread;
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> results = new List<T>(_sampleSize);
            _parallel.ForEach(sources, _parallelism,
                t => { Thread.Sleep(10); lock (results) results.Add(t); });
            Assert.That(results, Is.EquivalentTo(sources));
            Assert.That(_executor.ThreadCount.Value, Is.EqualTo(Math.Min(_parallelism, maxThread)));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachDoesNotSumitMoreThenMaxDegreeOfParallelism()
        {
            _executor.Threshold = _parallelism / 2;
            _executor.Delay = SHORT_DELAY;
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            List<T> results = new List<T>(_sampleSize);
            _parallel.ForEach(sources, _parallelism,
                t => { Thread.Sleep(10); lock (results) results.Add(t); });
            Assert.That(_executor.ThreadCount.Value, Is.EqualTo(_parallelism));
            Assert.That(results, Is.EquivalentTo(sources));
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachLimitsParallismToThreadPoolExecutorCoreSize(
            [Values(_parallelism - 2, _parallelism + 2)] int coreSize)
        {
            var tf = new ManagedThreadFactory(ThreadManager);
            var executor = new ThreadPoolExecutor(coreSize, _parallelism + 2, 
                TimeSpan.MaxValue, new LinkedBlockingQueue<IRunnable>(1), tf);
            try
            {
                var parallel = new Parallel<T>(executor);
                T[] sources = TestData<T>.MakeTestArray(_sampleSize);
                List<T> results = new List<T>(_sampleSize);
                parallel.ForEach(sources, _parallelism,
                    t => { Thread.Sleep(10); lock (results) results.Add(t); });
                Assert.That(results, Is.EquivalentTo(sources));
                Assert.That(parallel.ActualDegreeOfParallelism, Is.EqualTo(Math.Min(_parallelism, coreSize)));
            }
            finally
            {
                executor.ShutdownNow();
            }
            
            ThreadManager.JoinAndVerify();
        }

        [Test] public void ForEachChokesOnAnyExceptionFromBody()
        {
            var exception = new Exception("exception message");
            T[] sources = TestData<T>.MakeTestArray(_sampleSize);
            ThreadManager.StartAndAssertRegistered("Driver",
                () => {
                    var e = Assert.Throws<AggregateException>(() =>
                        _parallel.ForEach(sources, _parallelism,
                            t =>
                            {
                                if (Equals(t, sources[0]))
                                {
                                    Thread.Sleep(SHORT_DELAY);
                                    throw exception;
                                }
                                var e2 = Assert.Throws<ThreadInterruptedException>(
                                    () => Thread.Sleep(LONG_DELAY_MS));
                                if (!Equals(t, sources[_sampleSize-1])) throw e2;
                            }));
                    Assert.That(e.InnerException, Is.SameAs(exception));
                    Assert.That(_executor.ThreadCount.Value, Is.EqualTo(_parallelism));
                });
            Thread.Sleep(SHORT_DELAY);
            ThreadManager.JoinAndVerify();
        }
    }
}
