// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using NakedObjects.Architecture;

namespace NakedObjects.Core.Persist {
    public class AdapterException : NakedObjectSystemException {
        public AdapterException(string message) : base(message) {}
    }
}