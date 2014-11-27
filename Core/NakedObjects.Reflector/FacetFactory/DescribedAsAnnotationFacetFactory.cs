// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.ComponentModel;
using System.Reflection;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Meta.Facet;
using NakedObjects.Meta.Utils;
using NakedObjects.Util;

namespace NakedObjects.Reflect.FacetFactory {
    public class DescribedAsAnnotationFacetFactory : AnnotationBasedFacetFactoryAbstract {
        public DescribedAsAnnotationFacetFactory(int numericOrder)
            : base(numericOrder, FeatureType.Everything) {}

        public override void Process(IReflector reflector, Type type, IMethodRemover methodRemover, ISpecificationBuilder specification) {
            Attribute attribute = type.GetCustomAttributeByReflection<DescriptionAttribute>() ?? (Attribute) type.GetCustomAttributeByReflection<DescribedAsAttribute>();
            FacetUtils.AddFacet(Create(attribute, specification));
        }

        private static void Process(MemberInfo member, ISpecification holder) {
            Attribute attribute = AttributeUtils.GetCustomAttribute<DescriptionAttribute>(member) ?? (Attribute) AttributeUtils.GetCustomAttribute<DescribedAsAttribute>(member);
            FacetUtils.AddFacet(Create(attribute, holder));
        }

        public override void Process(IReflector reflector, MethodInfo method, IMethodRemover methodRemover, ISpecificationBuilder specification) {
            Process(method, specification);
        }

        public override void Process(IReflector reflector, PropertyInfo property, IMethodRemover methodRemover, ISpecificationBuilder specification) {
            Process(property, specification);
        }

        public override void ProcessParams(IReflector reflector, MethodInfo method, int paramNum, ISpecificationBuilder holder) {
            ParameterInfo parameter = method.GetParameters()[paramNum];
            Attribute attribute = parameter.GetCustomAttributeByReflection<DescriptionAttribute>() ?? (Attribute) parameter.GetCustomAttributeByReflection<DescribedAsAttribute>();
            FacetUtils.AddFacet(Create(attribute, holder));
        }

        private static IDescribedAsFacet Create(Attribute attribute, ISpecification holder) {
            if (attribute == null) {
                return null;
            }
            if (attribute is DescribedAsAttribute) {
                return Create((DescribedAsAttribute) attribute, holder);
            }
            if (attribute is DescriptionAttribute) {
                return Create((DescriptionAttribute) attribute, holder);
            }
            throw new ArgumentException("Unexpected attribute type: " + attribute.GetType());
        }

        private static IDescribedAsFacet Create(DescribedAsAttribute attribute, ISpecification holder) {
            return new DescribedAsFacetAnnotation(attribute.Value, holder);
        }

        private static IDescribedAsFacet Create(DescriptionAttribute attribute, ISpecification holder) {
            return new DescribedAsFacetAnnotation(attribute.Description, holder);
        }
    }
}