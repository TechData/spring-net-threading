#region License
/*
* Copyright (C) 2008-2009 the original author or authors.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*      http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion

namespace Spring.Threading
{
    /// <summary>
    /// TODO LinqBridge: This should be replaced by LinqBridge.
    /// Delegate to be submitted for execution.
    /// </summary>
    /// <seealso cref="IExecutor.Execute(Action)"/>
    public delegate void Action(); //NET_ONLY

    /// <summary>
    /// TODO LinqBridge: This should be replaced by LinqBridge.
    /// Analogue of System.Action{T1, T2}.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public delegate void Action<T1, T2>(T1 a1, T2 a2);

    /// <summary>
    /// TODO LinqBridge: This should be replaced by LinqBridge.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="a1"></param>
    /// <param name="a2"></param>
    /// <param name="a3"></param>
    public delegate void Action<T1, T2, T3>(T1 a1, T2 a2, T3 a3);
}
