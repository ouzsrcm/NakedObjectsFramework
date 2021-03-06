// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using org.nakedobjects.@object;

namespace NakedObjects.Facade.Nof2 {
    public class OidFacade : IOidFacade {
        private readonly Oid oid;

        public OidFacade(Oid oid) {
            this.oid = oid;
        }

        #region IOidFacade Members

        public object Value {
            get { return oid; }
        }

        #endregion

        public override bool Equals(object obj) {
            var oidWrapper = obj as OidFacade;
            if (oidWrapper != null) {
                return Equals(oidWrapper);
            }
            return false;
        }

        public bool Equals(OidFacade other) {
            if (ReferenceEquals(null, other)) { return false; }
            if (ReferenceEquals(this, other)) { return true; }
            return Equals(other.oid, oid);
        }

        public override int GetHashCode() {
            return (oid != null ? oid.GetHashCode() : 0);
        }
    }
}