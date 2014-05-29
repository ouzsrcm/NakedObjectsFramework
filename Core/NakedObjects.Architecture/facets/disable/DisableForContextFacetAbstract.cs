// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Interactions;

namespace NakedObjects.Architecture.Facets.Disable {
    public abstract class DisableForContextFacetAbstract : FacetAbstract, IDisableForContextFacet {
        protected DisableForContextFacetAbstract(IFacetHolder holder)
            : base(Type, holder) {}

        public static Type Type {
            get { return typeof (IDisableForContextFacet); }
        }

        #region IDisableForContextFacet Members

        public virtual string Disables(InteractionContext ic) {
            INakedObject target = ic.Target;
            return DisabledReason(target);
        }

        public virtual DisabledException CreateExceptionFor(InteractionContext ic) {
            return new DisabledException(ic, Disables(ic));
        }

        public abstract string DisabledReason(INakedObject nakedObject);

        #endregion
    }


    // Copyright (c) Naked Objects Group Ltd.
}