// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;

namespace NakedObjects.Core.Persist {
    [Obsolete] // unused ?
    public interface IDependencyInjector {
        void InjectDependencies(object dependent);
    }
}