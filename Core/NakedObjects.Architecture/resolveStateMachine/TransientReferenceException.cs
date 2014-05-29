// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

namespace NakedObjects.Architecture.Resolve {
    public class TransientReferenceException : NakedObjectApplicationException {
        public TransientReferenceException(string msg) : base(msg) {}
    }
}