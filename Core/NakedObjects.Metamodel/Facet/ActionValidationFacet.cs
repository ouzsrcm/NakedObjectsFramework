// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Interactions;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core.Util;
using Common.Logging;

namespace NakedObjects.Meta.Facet {
    [Serializable]
    public sealed class ActionValidationFacet : FacetAbstract, IActionValidationFacet, IImperativeFacet {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ActionValidationFacet));
        private readonly MethodInfo method;
        [field: NonSerialized]
        private Func<object, object[], object> methodDelegate;

        public ActionValidationFacet(MethodInfo method, ISpecification holder)
            : base(typeof (IActionValidationFacet), holder) {
            this.method = method;
            methodDelegate = DelegateUtils.CreateDelegate(method);
        }

        #region IActionValidationFacet Members

        public string Invalidates(IInteractionContext ic) {
            return InvalidReason(ic.Target, ic.ProposedArguments);
        }

        public Exception CreateExceptionFor(IInteractionContext ic) {
            return new ActionArgumentsInvalidException(ic, Invalidates(ic));
        }

        public string InvalidReason(INakedObjectAdapter target, INakedObjectAdapter[] proposedArguments) {
            if (methodDelegate != null) {
                return (string)methodDelegate(target.GetDomainObject(), proposedArguments.Select(no => no.GetDomainObject()).ToArray());
            }
            //Fall back (e.g. if method has > 6 params) on reflection...
            Log.WarnFormat("Invoking validate method via reflection as no delegate {0}.{1}", target, method);
            return (string)InvokeUtils.Invoke(method, target, proposedArguments);
        }

        #endregion

        #region IImperativeFacet Members

        public MethodInfo GetMethod() {
            return method;
        }

        public Func<object, object[], object> GetMethodDelegate() {
            return methodDelegate;
        }

        #endregion

        protected override string ToStringValues() {
            return "method=" + method;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) {
            methodDelegate = DelegateUtils.CreateDelegate(method);
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}