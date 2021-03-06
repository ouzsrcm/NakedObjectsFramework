// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Common.Logging;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Configuration;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Menu;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.Core;
using NakedObjects.Core.Util;
using NakedObjects.Meta.SpecImmutable;
using NakedObjects.Util;

namespace NakedObjects.ParallelReflect.Component {
    public sealed class ParallelReflector : IReflector {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ParallelReflector));
        private readonly IReflectorConfiguration config;
        private readonly FacetDecoratorSet facetDecoratorSet;
        private readonly IMetamodelBuilder initialMetamodel;
        private readonly IMenuFactory menuFactory;
        private readonly ISet<Type> serviceTypes = new HashSet<Type>();

        public ParallelReflector(IClassStrategy classStrategy,
                                 IMetamodelBuilder metamodel,
                                 IReflectorConfiguration config,
                                 IMenuFactory menuFactory,
                                 IFacetDecorator[] facetDecorators,
                                 IFacetFactory[] facetFactories) {
            Assert.AssertNotNull(classStrategy);
            Assert.AssertNotNull(metamodel);
            Assert.AssertNotNull(config);
            Assert.AssertNotNull(menuFactory);

            this.ClassStrategy = classStrategy;
            this.initialMetamodel = metamodel;
            this.config = config;
            this.menuFactory = menuFactory;
            facetDecoratorSet = new FacetDecoratorSet(facetDecorators);
            FacetFactorySet = new FacetFactorySet(facetFactories);
        }

        // exposed for testing
        public IFacetDecoratorSet FacetDecoratorSet => facetDecoratorSet;

        #region IReflector Members

        public bool ConcurrencyChecking => config.ConcurrencyChecking;

        public bool IgnoreCase => config.IgnoreCase;

        public IClassStrategy ClassStrategy { get; }

        public IFacetFactorySet FacetFactorySet { get; }

        public IMetamodel Metamodel => null;

        public ITypeSpecBuilder LoadSpecification(Type type) {
            throw new NotImplementedException();
        }

        public T LoadSpecification<T>(Type type) where T : ITypeSpecImmutable {
            throw new NotImplementedException();
        }

        public void LoadSpecificationForReturnTypes(IList<PropertyInfo> properties, Type classToIgnore) {
            throw new NotImplementedException();
        }

        public ITypeSpecBuilder[] AllObjectSpecImmutables => initialMetamodel.AllSpecifications.Cast<ITypeSpecBuilder>().ToArray();

        public Tuple<ITypeSpecBuilder, IImmutableDictionary<string, ITypeSpecBuilder>> LoadSpecification(Type type, IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            Assert.AssertNotNull(type);

            var actualType = ClassStrategy.GetType(type);
            var typeKey = ClassStrategy.GetKeyForType(actualType);
            if (!metamodel.ContainsKey(typeKey)) {
                return LoadPlaceholder(actualType, metamodel);
            }

            return new Tuple<ITypeSpecBuilder, IImmutableDictionary<string, ITypeSpecBuilder>>(metamodel[typeKey], metamodel);
        }

        public void Reflect() {
            Type[] s1 = config.Services;
            Type[] services = s1;
            Type[] nonServices = GetTypesToIntrospect();

            services.ForEach(t => serviceTypes.Add(t));

            var allTypes = services.Union(nonServices).ToArray();

            var mm = InstallSpecificationsParallel(allTypes, initialMetamodel, config);

            PopulateAssociatedActions(s1, mm);

            //Menus installed once rest of metamodel has been built:
            InstallMainMenus(mm);
            InstallObjectMenus(mm);
        }

        #endregion

        public Tuple<ITypeSpecBuilder, IImmutableDictionary<string, ITypeSpecBuilder>> IntrospectSpecification(Type actualType, IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            Assert.AssertNotNull(actualType);

            var typeKey = ClassStrategy.GetKeyForType(actualType);

            if (string.IsNullOrEmpty(metamodel[typeKey].FullName)) {
                return LoadSpecificationAndCache(actualType, metamodel);
            }

            return new Tuple<ITypeSpecBuilder, IImmutableDictionary<string, ITypeSpecBuilder>>(metamodel[typeKey], metamodel);
        }

        private Type EnsureGenericTypeIsComplete(Type type) {
            if (type.IsGenericType && !type.IsConstructedGenericType) {
                var genericType = type.GetGenericTypeDefinition();
                var genericParms = genericType.GetGenericArguments().Select(a => typeof(object)).ToArray();

                return type.GetGenericTypeDefinition().MakeGenericType(genericParms);
            }

            return type;
        }

        private Type[] GetTypesToIntrospect() {
            var types = config.TypesToIntrospect.Select(EnsureGenericTypeIsComplete);
            var systemTypes = config.SupportedSystemTypes.Select(EnsureGenericTypeIsComplete);
            return types.Union(systemTypes).ToArray();
        }

        private IImmutableDictionary<string, ITypeSpecBuilder> IntrospectPlaceholders(IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            var ph = metamodel.Where(i => string.IsNullOrEmpty(i.Value.FullName)).Select(i => i.Value.Type);
            var mm = ph.AsParallel().SelectMany(type => IntrospectSpecification(type, metamodel).Item2).Distinct(new DE()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value).ToImmutableDictionary();

            if (mm.Any(i => string.IsNullOrEmpty(i.Value.FullName))) {
                return IntrospectPlaceholders(mm);
            }

            return mm;
        }

        private IMetamodelBuilder InstallSpecificationsParallel(Type[] types, IMetamodelBuilder metamodel, IReflectorConfiguration config) {
            var mm = GetPlaceholders(types);
            mm = IntrospectPlaceholders(mm);
            mm.ForEach(i => metamodel.Add(i.Value.Type, i.Value));
            return metamodel;
        }

        private void PopulateAssociatedActions(Type[] services, IMetamodelBuilder metamodel) {
            var nonServiceSpecs = AllObjectSpecImmutables.OfType<IObjectSpecBuilder>();

            foreach (var spec in nonServiceSpecs) {
                PopulateAssociatedActions(spec, services, metamodel);
            }
        }

        private void PopulateAssociatedActions(IObjectSpecBuilder spec, Type[] services, IMetamodelBuilder metamodel) {
            if (string.IsNullOrWhiteSpace(spec.FullName)) {
                string id = (spec.Identifier != null ? spec.Identifier.ClassName : "unknown") ?? "unknown";
                Log.WarnFormat("Specification with id : {0} as has null or empty name", id);
            }

            if (FasterTypeUtils.IsSystem(spec.FullName) && !spec.IsCollection) {
                return;
            }

            if (FasterTypeUtils.IsNakedObjects(spec.FullName)) {
                return;
            }

            PopulateContributedActions(spec, services, metamodel);
        }

        private void InstallMainMenus(IMetamodelBuilder metamodel) {
            var menus = config.MainMenus?.Invoke(menuFactory);
            // Unlike other things specified in config, this one can't be checked when ReflectorConfiguration is constructed.
            // Allows developer to deliberately not specify any menus
            if (menus != null) {
                if (!menus.Any()) {
                    //Catches accidental non-specification of menus
                    throw new ReflectionException(Log.LogAndReturn("No MainMenus specified."));
                }

                foreach (IMenuImmutable menu in menus.OfType<IMenuImmutable>()) {
                    metamodel.AddMainMenu(menu);
                }
            }
        }

        private void InstallObjectMenus(IMetamodelBuilder metamodel) {
            IEnumerable<IMenuFacet> menuFacets = metamodel.AllSpecifications.Where(s => s.ContainsFacet<IMenuFacet>()).Select(s => s.GetFacet<IMenuFacet>());
            menuFacets.ForEach(mf => mf.CreateMenu(metamodel));
        }

        private void PopulateContributedActions(IObjectSpecBuilder spec, Type[] services, IMetamodel metamodel) {

            var result = services.
                AsParallel().
                Select(serviceType => {
                    var serviceSpecification = (IServiceSpecImmutable)metamodel.GetSpecification(serviceType);
                    IActionSpecImmutable[] serviceActions = serviceSpecification.ObjectActions.Where(sa => sa != null).ToArray();

                    var matchingActionsForObject = new List<IActionSpecImmutable>();
                    var matchingActionsForCollection = new List<IActionSpecImmutable>();
                    var finderActions = new List<IActionSpecImmutable>();

                    foreach (var sa in serviceActions) {
                        if (serviceType != spec.Type) {
                            if (sa.IsContributedTo(spec)) {
                                matchingActionsForObject.Add(sa);
                            }

                            if (sa.IsContributedToCollectionOf(spec)) {
                                matchingActionsForCollection.Add(sa);
                            }
                        }

                        if (sa.IsFinderMethodFor(spec)) {
                            finderActions.Add(sa);
                        }
                    }

                    return new Tuple<List<IActionSpecImmutable>, List<IActionSpecImmutable>, List<IActionSpecImmutable>>(matchingActionsForObject, matchingActionsForCollection, finderActions.OrderBy(a => a, new MemberOrderComparator<IActionSpecImmutable>()).ToList());
                }).
                Aggregate(new Tuple<List<IActionSpecImmutable>, List<IActionSpecImmutable>, List<IActionSpecImmutable>>(new List<IActionSpecImmutable>(), new List<IActionSpecImmutable>(), new List<IActionSpecImmutable>()),
                (a, t) => {
                    a.Item1.AddRange(t.Item1);
                    a.Item2.AddRange(t.Item2);
                    a.Item3.AddRange(t.Item3);
                    return a;
                });

            //
            var contribActions = new List<IActionSpecImmutable>();

            // group by service - probably do this better - TODO
            foreach (var service in services) {
                var matching = result.Item1.Where(i => i.OwnerSpec.Type == service);
                contribActions.AddRange(matching);
            }

            spec.AddContributedActions(contribActions);
            spec.AddCollectionContributedActions(result.Item2);
            spec.AddFinderActions(result.Item3);
        }



        private ITypeSpecBuilder GetPlaceholder(Type type, IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            ITypeSpecBuilder specification = CreateSpecification(type, metamodel);

            if (specification == null) {
                throw new ReflectionException(Log.LogAndReturn($"unrecognised type {type.FullName}"));
            }

            return specification;
        }

        private Tuple<ITypeSpecBuilder, IImmutableDictionary<string, ITypeSpecBuilder>> LoadPlaceholder(Type type, IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            ITypeSpecBuilder specification = CreateSpecification(type, metamodel);

            if (specification == null) {
                throw new ReflectionException(Log.LogAndReturn($"unrecognised type {type.FullName}"));
            }

            metamodel = metamodel.Add(ClassStrategy.GetKeyForType(type), specification);

            return new Tuple<ITypeSpecBuilder, IImmutableDictionary<string, ITypeSpecBuilder>>(specification, metamodel);
        }

        private IImmutableDictionary<string, ITypeSpecBuilder> GetPlaceholders(Type[] types) {
            return types.Select(t => ClassStrategy.GetType(t)).Where(t => t != null).Distinct(new DR(ClassStrategy)).ToDictionary(t => ClassStrategy.GetKeyForType(t), t => GetPlaceholder(t, null)).ToImmutableDictionary();
        }

        private Tuple<ITypeSpecBuilder, IImmutableDictionary<string, ITypeSpecBuilder>> LoadSpecificationAndCache(Type type, IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            ITypeSpecBuilder specification = metamodel[ClassStrategy.GetKeyForType(type)];

            if (specification == null) {
                throw new ReflectionException(Log.LogAndReturn($"unrecognised type {type.FullName}"));
            }

            metamodel = specification.Introspect(facetDecoratorSet, new Introspector(this), metamodel);

            return new Tuple<ITypeSpecBuilder, IImmutableDictionary<string, ITypeSpecBuilder>>(specification, metamodel);
        }

        private ITypeSpecBuilder CreateSpecification(Type type, IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            TypeUtils.GetType(type.FullName); // This should ensure type is cached

            return IsService(type) ? (ITypeSpecBuilder) ImmutableSpecFactory.CreateServiceSpecImmutable(type, metamodel) : ImmutableSpecFactory.CreateObjectSpecImmutable(type, metamodel);
        }

        private bool IsService(Type type) {
            return serviceTypes.Contains(type);
        }

        #region Nested type: DE

        private class DE : IEqualityComparer<KeyValuePair<string, ITypeSpecBuilder>> {
            #region IEqualityComparer<KeyValuePair<string,ITypeSpecBuilder>> Members

            public bool Equals(KeyValuePair<string, ITypeSpecBuilder> x, KeyValuePair<string, ITypeSpecBuilder> y) {
                return x.Key.Equals(y.Key, StringComparison.Ordinal);
            }

            public int GetHashCode(KeyValuePair<string, ITypeSpecBuilder> obj) {
                return obj.Key.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Nested type: DR

        private class DR : IEqualityComparer<Type> {
            private readonly IClassStrategy classStrategy;

            public DR(IClassStrategy classStrategy) {
                this.classStrategy = classStrategy;
            }

            #region IEqualityComparer<Type> Members

            public bool Equals(Type x, Type y) {
                return classStrategy.GetKeyForType(x).Equals(classStrategy.GetKeyForType(y), StringComparison.Ordinal);
            }

            public int GetHashCode(Type obj) {
                return classStrategy.GetKeyForType(obj).GetHashCode();
            }

            #endregion
        }

        #endregion
    }

    // Copyright (c) Naked Objects Group Ltd.
}