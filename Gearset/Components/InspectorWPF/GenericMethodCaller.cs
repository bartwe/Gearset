using System;
using System.Linq;
using System.Reflection;

namespace Gearset.Components.InspectorWPF {
    public class GenericMethodCaller : MethodCaller {
        /// <summary>
        /// TargetObject for instance methods.
        /// </summary>
        readonly Object _invocationTarget;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="methodInfo"></param>
        public GenericMethodCaller(MethodInfo methodInfo, Object target)
            : base(methodInfo) {
            _invocationTarget = target;
        }

        /// <summary>
        /// True if the method is an instance method.
        /// </summary>
        bool IsStatic { get { return MethodInfo.IsStatic; } }

        /// <summary>
        /// Calls the method with the established parameters.
        /// </summary>
        public override void CallMethod() {
            MethodInfo.Invoke(_invocationTarget, (from i in Parameters select i.Parameter.Property).ToArray());
        }
    }
}
