// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects;

namespace AdventureWorksModel {
    [IconName("house.png")]
    public class VendorAddress : AWDomainObject, IAddressRole {
        [Hidden(WhenTo.Always)]
        public virtual int VendorID { get; set; }

        [Hidden(WhenTo.Always)]
        public virtual int AddressID { get; set; }

        [Disabled]
        public virtual Address Address { get; set; }

        public virtual AddressType AddressType { get; set; }

        [Hidden(WhenTo.Always)]
        public virtual Vendor Vendor { get; set; }

        #region ModifiedDate

        [MemberOrder(99)]
        [Disabled]
        public override DateTime ModifiedDate { get; set; }

        #endregion

        #region Title

        public override string ToString() {
            var t = Container.NewTitleBuilder();
            t.Append(AddressType).Append(":", Address);
            return t.ToString();
        }

        #endregion
    }
}