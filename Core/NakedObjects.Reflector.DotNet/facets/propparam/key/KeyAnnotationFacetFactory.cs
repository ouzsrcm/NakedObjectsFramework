// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using System.ComponentModel.DataAnnotations;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Facets.Objects.Key;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Util;
using PropertyInfo = System.Reflection.PropertyInfo;

namespace NakedObjects.Reflector.DotNet.Facets.Objects.Key {
    public class KeyAnnotationFacetFactory : AnnotationBasedFacetFactoryAbstract {
        public KeyAnnotationFacetFactory()
            : base(NakedObjectFeatureType.PropertiesOnly) {}

        public override bool Process(PropertyInfo property, IMethodRemover methodRemover, IFacetHolder holder) {
            Attribute attribute = property.GetCustomAttribute<KeyAttribute>();
            return FacetUtils.AddFacet(Create(attribute, holder));
        }


        private static IKeyFacet Create(Attribute attribute, IFacetHolder holder) {
            return attribute == null ? null : new KeyFacetAnnotation(holder);
        }
    }
}