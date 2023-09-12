using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// A container for holding a delegate with an argument value.
    /// </summary>
    /// <typeparam name="TArg">The argument type.</typeparam>
    /// <typeparam name="TResult">The return type.</typeparam>
    public class DelegateWithArgumentContainer<TArg, TResult>
    {
        public Func<TArg, TResult> function;
        public TArg arg;

        /// <summary>
        /// Get the value of the delegate with the argument inserted.
        /// </summary>
        /// <returns>The value returned by the delegate with the argument inserted.</returns>
        public TResult GetValue()
        {
            return function(arg);
        }
    }
}