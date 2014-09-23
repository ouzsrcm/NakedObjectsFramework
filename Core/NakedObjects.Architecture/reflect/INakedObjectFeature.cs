// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Persist;
using NakedObjects.Architecture.Security;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Architecture.Reflect {
    /// <summary>
    ///     Base interface for fields, collections, actions and action parameters.
    /// </summary>
    public interface INakedObjectFeature : IFacetHolder, INamedAndDescribed {
        /// <summary>
        ///     Returns the specifications for the feature.
        /// </summary>
        /// <para>
        ///     Will be non-<c>null</c> value for everything <i>except</i> an  action.
        /// </para>
        INakedObjectSpecification Specification { get; }

        bool IsNullable { get; }
        IConsent IsUsable(ISession session, INakedObject target, ILifecycleManager persistor);
    }
}