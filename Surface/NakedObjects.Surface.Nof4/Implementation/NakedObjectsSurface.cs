﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using NakedObjects.Architecture;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facets.Actcoll.Typeof;
using NakedObjects.Architecture.Facets.Collections.Modify;
using NakedObjects.Architecture.Facets.Objects.Immutable;
using NakedObjects.Architecture.Facets.Objects.Parseable;
using NakedObjects.Architecture.Facets.Objects.Validation;
using NakedObjects.Architecture.Persist;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Resolve;
using NakedObjects.Architecture.Services;
using NakedObjects.Architecture.Spec;
using NakedObjects.Surface.Context;
using NakedObjects.Surface.Nof4.Context;
using NakedObjects.Surface.Nof4.Utility;
using NakedObjects.Surface.Nof4.Wrapper;
using NakedObjects.Util;

namespace NakedObjects.Surface.Nof4.Implementation {
    public class NakedObjectsSurface : INakedObjectsSurface {
        private readonly INakedObjectsFramework framework;
        private readonly IOidStrategy oidStrategy;

        public NakedObjectsSurface(IOidStrategy oidStrategy, INakedObjectsFramework framework) {
            oidStrategy.Surface = this;
            this.oidStrategy = oidStrategy;
            this.framework = framework;
            OidStrategyHolder.OidStrategy = oidStrategy;
        }

        #region INakedObjectsSurface Members

        public ObjectContextSurface GetImage(string imageId) {
            return null;
        }


        public void Start() {
            //framework.EnsureReady();
            //SetSession();
            framework.LifecycleManager.StartTransaction();
        }

        public void End(bool success) {
            if (success) {
                framework.LifecycleManager.EndTransaction();
            }
            else {
                framework.LifecycleManager.AbortTransaction();
            }
        }

        public IPrincipal GetUser() {
            return MapErrors(() => framework.Session.Principal);
        }

        public INakedObjectSpecificationSurface[] GetDomainTypes() {
            return MapErrors(() => framework.Metadata.AllSpecifications.
                Where(s => !IsGenericType(s)).
                Select(GetSpecificationWrapper).ToArray());
        }

        public ObjectContextSurface GetService(LinkObjectId serviceName) {
            return MapErrors(() => GetServiceInternal(serviceName).ToObjectContextSurface(this, framework));
        }

        public ListContextSurface GetServices() {
            return MapErrors(() => GetServicesInternal().ToListContextSurface(this, framework));
        }

        public ObjectContextSurface GetObject(INakedObjectSurface nakedObject) {
            return MapErrors(() => GetObjectContext(((NakedObjectWrapper) nakedObject).WrappedNakedObject).ToObjectContextSurface(this, framework));
        }

        public INakedObjectSpecificationSurface GetDomainType(string typeName) {
            return MapErrors(() => GetSpecificationWrapper(GetDomainTypeInternal(typeName)));
        }

        public PropertyTypeContextSurface GetPropertyType(string typeName, string propertyName) {
            return MapErrors(() => {
                Tuple<INakedObjectAssociation, INakedObjectSpecification> pc = GetPropertyTypeInternal(typeName, propertyName);

                return new PropertyTypeContextSurface {
                    Property = new NakedObjectAssociationWrapper(pc.Item1, this, framework),
                    OwningSpecification = GetSpecificationWrapper(pc.Item2)
                };
            });
        }

        public ActionTypeContextSurface GetActionType(string typeName, string actionName) {
            return MapErrors(() => {
                Tuple<ActionContext, INakedObjectSpecification> pc = GetActionTypeInternal(typeName, actionName);
                return new ActionTypeContextSurface {
                    ActionContext = pc.Item1.ToActionContextSurface(this, framework),
                    OwningSpecification = GetSpecificationWrapper(pc.Item2)
                };
            });
        }

        public ParameterTypeContextSurface GetActionParameterType(string typeName, string actionName, string parmName) {
            return MapErrors(() => {
                var pc = GetActionParameterTypeInternal(typeName, actionName, parmName);

                return new ParameterTypeContextSurface {
                    Action = new NakedObjectActionWrapper(pc.Item1, this, framework, pc.Item4),
                    OwningSpecification = GetSpecificationWrapper(pc.Item2),
                    Parameter = new NakedObjectActionParameterWrapper(pc.Item3, this, framework, pc.Item4)
                };
            });
        }

        public ObjectContextSurface Persist(string typeName, ArgumentsContext arguments) {
            return MapErrors(() => CreateObject(typeName, arguments));
        }

        public UserCredentials Validate(string user, string password) {
            return new UserCredentials(user, password, new List<string>());
        }

        public ObjectContextSurface GetObject(LinkObjectId oid) {
            return MapErrors(() => GetObjectInternal(oid).ToObjectContextSurface(this, framework));
        }

        public ObjectContextSurface PutObject(LinkObjectId oid, ArgumentsContext arguments) {
            return MapErrors(() => ChangeObject(GetObjectAsNakedObject(oid), arguments));
        }

        public PropertyContextSurface GetProperty(LinkObjectId oid, string propertyName) {
            return MapErrors(() => GetProperty(GetObjectAsNakedObject(oid), propertyName).ToPropertyContextSurface(this, framework));
        }

        public ListContextSurface GetPropertyCompletions(LinkObjectId objectId, string propertyName, ArgumentsContext arguments) {
            return MapErrors(() => GetPropertyCompletions(GetObjectAsNakedObject(objectId), propertyName, arguments).ToListContextSurface(this, framework));
        }

        public ListContextSurface GetParameterCompletions(LinkObjectId objectId, string actionName, string parmName, ArgumentsContext arguments) {
            return MapErrors(() => GetParameterCompletions(GetObjectAsNakedObject(objectId), actionName, parmName, arguments).ToListContextSurface(this, framework));
        }

        public ListContextSurface GetServiceParameterCompletions(LinkObjectId objectId, string actionName, string parmName, ArgumentsContext arguments) {
            return MapErrors(() => GetParameterCompletions(GetServiceAsNakedObject(objectId), actionName, parmName, arguments).ToListContextSurface(this, framework));
        }

        public ActionContextSurface GetServiceAction(LinkObjectId serviceName, string actionName) {
            return MapErrors(() => GetAction(actionName, GetServiceAsNakedObject(serviceName)).ToActionContextSurface(this, framework));
        }

        public ActionContextSurface GetObjectAction(LinkObjectId objectId, string actionName) {
            return MapErrors(() => GetAction(actionName, GetObjectAsNakedObject(objectId)).ToActionContextSurface(this, framework));
        }

        public PropertyContextSurface PutProperty(LinkObjectId objectId, string propertyName, ArgumentContext argument) {
            return MapErrors(() => ChangeProperty(GetObjectAsNakedObject(objectId), propertyName, argument));
        }

        public PropertyContextSurface DeleteProperty(LinkObjectId objectId, string propertyName, ArgumentContext argument) {
            return MapErrors(() => ChangeProperty(GetObjectAsNakedObject(objectId), propertyName, argument));
        }


        public ActionResultContextSurface ExecuteObjectAction(LinkObjectId objectId, string actionName, ArgumentsContext arguments) {
            return MapErrors(() => {
                ActionContext actionContext = GetInvokeActionOnObject(objectId, actionName);
                return ExecuteAction(actionContext, arguments);
            });
        }

        public ActionResultContextSurface ExecuteServiceAction(LinkObjectId serviceName, string actionName, ArgumentsContext arguments) {
            return MapErrors(() => {
                ActionContext actionContext = GetInvokeActionOnService(serviceName, actionName);
                return ExecuteAction(actionContext, arguments);
            });
        }

        #endregion

        #region Helpers

        private void SetSession() {
            //framework.Instance.SetSession(new WindowsSession(Thread.CurrentPrincipal));
        }


        private INakedObjectAssociation GetPropertyInternal(INakedObject nakedObject, string propertyName, bool onlyVisible = true) {
            if (string.IsNullOrWhiteSpace(propertyName)) {
                throw new BadRequestNOSException();
            }

            IEnumerable<INakedObjectAssociation> propertyQuery = nakedObject.Specification.Properties;

            if (onlyVisible) {
                propertyQuery = propertyQuery.Where(p => p.IsVisible(framework.Session, nakedObject, framework.LifecycleManager));
            }

            INakedObjectAssociation property = propertyQuery.SingleOrDefault(p => p.Id == propertyName);

            if (property == null) {
                throw new PropertyResourceNotFoundNOSException(propertyName);
            }

            return property;
        }


        private PropertyContext GetProperty(INakedObject nakedObject, string propertyName, bool onlyVisible = true) {
            INakedObjectAssociation property = GetPropertyInternal(nakedObject, propertyName, onlyVisible);
            return new PropertyContext {Target = nakedObject, Property = property};
        }

        private ListContext GetServicesInternal() {
            INakedObject[] services = framework.LifecycleManager.GetServicesWithVisibleActions(ServiceTypes.Menu | ServiceTypes.Contributor, framework.LifecycleManager);
            INakedObjectSpecification elementType = framework.Metadata.GetSpecification(typeof (object));

            return new ListContext {
                ElementType = elementType,
                List = services,
                IsListOfServices = true
            };
        }

        private ListContext GetCompletions(PropParmAdapter propParm, INakedObject nakedObject, ArgumentsContext arguments) {
            INakedObject[] list = propParm.GetList(nakedObject, arguments);

            return new ListContext {
                ElementType = propParm.Specification,
                List = list,
                IsListOfServices = false
            };
        }

        private ListContext GetPropertyCompletions(INakedObject nakedObject, string propertyName, ArgumentsContext arguments) {
            var property = GetPropertyInternal(nakedObject, propertyName) as IOneToOneFeature;
            return GetCompletions(new PropParmAdapter(property, this, framework), nakedObject, arguments);
        }

        private ListContext GetParameterCompletions(INakedObject nakedObject, string actionName, string parmName, ArgumentsContext arguments) {
            INakedObjectActionParameter parm = GetParameterInternal(actionName, parmName, nakedObject);
            return GetCompletions(new PropParmAdapter(parm, this, framework), nakedObject, arguments);
        }

        private Tuple<INakedObjectAssociation, INakedObjectSpecification> GetPropertyTypeInternal(string typeName, string propertyName) {
            if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(propertyName)) {
                throw new BadRequestNOSException();
            }

            INakedObjectSpecification spec = GetDomainTypeInternal(typeName);

            INakedObjectAssociation property = spec.Properties.SingleOrDefault(p => p.Id == propertyName);

            if (property == null) {
                throw new TypePropertyResourceNotFoundNOSException(propertyName, typeName);
            }

            return new Tuple<INakedObjectAssociation, INakedObjectSpecification>(property, spec);
        }

        private PropertyContext CanChangeProperty(INakedObject nakedObject, string propertyName, object toPut = null) {
            PropertyContext context = GetProperty(nakedObject, propertyName);
            context.ProposedValue = toPut;
            var property = (IOneToOneAssociation) context.Property;

            if (ConsentHandler(IsCurrentlyMutable(context.Target), context, Cause.Immutable)) {
                if (ConsentHandler(property.IsUsable(framework.Session, context.Target, framework.LifecycleManager), context, Cause.Disabled)) {
                    if (toPut != null && ConsentHandler(CanSetPropertyValue(context), context, Cause.WrongType)) {
                        ConsentHandler(property.IsAssociationValid(context.Target, context.ProposedNakedObject, framework.Session), context, Cause.Other);
                    }
                }
            }

            return context;
        }

        private PropertyContext CanSetProperty(INakedObject nakedObject, string propertyName, object toPut = null) {
            PropertyContext context = GetProperty(nakedObject, propertyName, false);
            context.ProposedValue = toPut;
            var property = (IOneToOneAssociation) context.Property;

            //if (ConsentHandler(IsCurrentlyMutable(context.Target), context, Cause.Immutable)) {
            if (toPut != null && ConsentHandler(CanSetPropertyValue(context), context, Cause.WrongType)) {
                ConsentHandler(property.IsAssociationValid(context.Target, context.ProposedNakedObject, framework.Session), context, Cause.Other);
            }
            else if (toPut == null && (property.IsMandatory && property.IsUsable(framework.Session, context.Target, framework.LifecycleManager).IsAllowed)) {
                // only check user editable fields
                context.Reason = "Mandatory";
                context.ErrorCause = Cause.Other;
            }
            //}

            return context;
        }


        private IConsent CrossValidate(ObjectContext context) {
            INakedObjectValidation[] validators = context.Specification.ValidateMethods();

            foreach (INakedObjectValidation validator in validators) {
                string[] parmNames = validator.ParameterNames;

                List<PropertyContext> matchingparms = parmNames.Select(pn => context.VisibleProperties.Single(p => p.Id.ToLower() == pn)).ToList();

                if (matchingparms.Count() == parmNames.Count()) {
                    string result = validator.Execute(context.Target, matchingparms.Select(p => p.ProposedNakedObject).ToArray());

                    if (!string.IsNullOrEmpty(result)) {
                        return new Veto(result);
                    }
                }
            }

            if (context.Specification.ContainsFacet<IValidateProgrammaticUpdatesFacet>()) {
                string state = context.Target.ValidToPersist();
                if (state != null) {
                    return new Veto(state);
                }
            }
            return new Allow();
        }


        private PropertyContextSurface ChangeProperty(INakedObject nakedObject, string propertyName, ArgumentContext argument) {
            ValidateConcurrency(nakedObject, argument.Digest);
            PropertyContext context = CanChangeProperty(nakedObject, propertyName, argument.Value);
            if (string.IsNullOrEmpty(context.Reason)) {
                IEnumerable<PropertyContext> existingValues = context.Target.Specification.Properties.Where(p => p.Id != context.Id).
                    Select(p => new {p, no = p.GetNakedObject(context.Target, framework.LifecycleManager)}).
                    Select(ao => new PropertyContext {
                        Property = ao.p,
                        ProposedNakedObject = ao.no,
                        ProposedValue = ao.no == null ? null : ao.no.Object,
                        Target = context.Target
                    }
                    ).Union(new[] {context});

                var objectContext = new ObjectContext(context.Target) {VisibleProperties = existingValues.ToArray()};

                if (ConsentHandler(CrossValidate(objectContext), objectContext, Cause.Other)) {
                    if (!argument.ValidateOnly) {
                        SetProperty(context);
                    }
                }
                else {
                    context.Reason = objectContext.Reason;
                    context.ErrorCause = objectContext.ErrorCause;
                }
            }
            context.Mutated = true; // mark as changed even if property not actually changed to stop self rep
            return context.ToPropertyContextSurface(this, framework);
        }

        private void SetProperty(PropertyContext context) {
            ((IOneToOneAssociation) context.Property).SetAssociation(context.Target, context.ProposedValue == null ? null : context.ProposedNakedObject, framework.LifecycleManager);
        }

        private static void ValidateConcurrency(INakedObject nakedObject, string digest) {
            if (!string.IsNullOrEmpty(digest) && new VersionWrapper(nakedObject.Version).IsDifferent(digest)) {
                throw new PreconditionFailedNOSException();
            }
        }

        private ObjectContextSurface ChangeObject(INakedObject nakedObject, ArgumentsContext arguments) {
            ValidateConcurrency(nakedObject, arguments.Digest);

            Dictionary<string, PropertyContext> contexts;
            try {
                contexts = arguments.Values.ToDictionary(kvp => kvp.Key, kvp => CanChangeProperty(nakedObject, kvp.Key, kvp.Value));
            }
            catch (PropertyResourceNotFoundNOSException e) {
                // no matching property for argument - consider this a syntax error 
                throw new BadRequestNOSException(e.Message);
            }


            var objectContext = new ObjectContext(contexts.First().Value.Target) {VisibleProperties = contexts.Values.ToArray()};

            // if we fail we need to display passed in properties - if OK all visible
            PropertyContext[] propertiesToDisplay = objectContext.VisibleProperties;

            if (contexts.Values.All(c => string.IsNullOrEmpty(c.Reason))) {
                if (ConsentHandler(CrossValidate(objectContext), objectContext, Cause.Other)) {
                    if (!arguments.ValidateOnly) {
                        Array.ForEach(objectContext.VisibleProperties, SetProperty);
                    }

                    propertiesToDisplay = nakedObject.Specification.Properties.
                        Where(p => p.IsVisible(framework.Session, nakedObject, framework.LifecycleManager)).
                        Select(p => new PropertyContext {Target = nakedObject, Property = p}).ToArray();
                }
            }

            ObjectContext oc = GetObjectContext(objectContext.Target);
            oc.Mutated = true;
            oc.Reason = objectContext.Reason;
            oc.VisibleProperties = propertiesToDisplay;
            return oc.ToObjectContextSurface(this, framework);
        }

        private ObjectContextSurface SetObject(INakedObject nakedObject, ArgumentsContext arguments) {
            if (nakedObject.Specification.Properties.Where(p => !p.IsCollection).Any(p => !arguments.Values.Keys.Contains(p.Id))) {
                throw new BadRequestNOSException("Malformed arguments");
            }

            Dictionary<string, PropertyContext> contexts = arguments.Values.ToDictionary(kvp => kvp.Key, kvp => CanSetProperty(nakedObject, kvp.Key, kvp.Value));
            var objectContext = new ObjectContext(contexts.First().Value.Target) {VisibleProperties = contexts.Values.ToArray()};

            // if we fail we need to display all - if OK only those that are visible 
            PropertyContext[] propertiesToDisplay = objectContext.VisibleProperties;

            if (contexts.Values.All(c => string.IsNullOrEmpty(c.Reason))) {
                if (ConsentHandler(CrossValidate(objectContext), objectContext, Cause.Other)) {
                    if (!arguments.ValidateOnly) {
                        Array.ForEach(objectContext.VisibleProperties, SetProperty);

                        if (nakedObject.Specification.Persistable == PersistableType.UserPersistable) {
                            framework.LifecycleManager.MakePersistent(nakedObject);
                        }
                        else {
                            framework.LifecycleManager.ObjectChanged(nakedObject);
                        }
                        propertiesToDisplay = nakedObject.Specification.Properties.
                            Where(p => p.IsVisible(framework.Session, nakedObject, framework.LifecycleManager)).
                            Select(p => new PropertyContext {Target = nakedObject, Property = p}).ToArray();
                    }
                }
            }

            ObjectContext oc = GetObjectContext(objectContext.Target);
            oc.Reason = objectContext.Reason;
            oc.VisibleProperties = propertiesToDisplay;
            return oc.ToObjectContextSurface(this, framework);
        }

        private bool ValidateParameters(ActionContext actionContext, IDictionary<string, object> rawParms) {
            if (rawParms.Any(kvp => !actionContext.Action.Parameters.Select(p => p.Id).Contains(kvp.Key))) {
                throw new BadRequestNOSException("Malformed arguments");
            }

            bool isValid = true;
            var orderedParms = new Dictionary<string, ParameterContext>();

            // handle contributed actions 

            if (actionContext.Action.IsContributedMethod && !actionContext.Action.OnType.Equals(actionContext.Target.Specification)) {
                INakedObjectActionParameter parm = actionContext.Action.Parameters.FirstOrDefault(p => actionContext.Target.Specification.IsOfType(p.Specification));

                if (parm != null) {
                    rawParms.Add(parm.Id, actionContext.Target.Object);
                }
            }

            // check mandatory fields first as standard NO behaviour is that no validation takes place until 
            // all mandatory fields are set. 
            foreach (INakedObjectActionParameter parm in actionContext.Action.Parameters) {
                orderedParms[parm.Id] = new ParameterContext();

                object value = rawParms.ContainsKey(parm.Id) ? rawParms[parm.Id] : null;

                orderedParms[parm.Id].ProposedValue = value;
                orderedParms[parm.Id].Parameter = parm;
                orderedParms[parm.Id].Action = actionContext.Action;

                var stringValue = value as string;

                if (parm.IsMandatory && (value == null || (value is string && string.IsNullOrEmpty(stringValue)))) {
                    isValid = false;
                    orderedParms[parm.Id].Reason = "Mandatory"; // i18n
                }
            }

            //check for individual parameter validity, including parsing of text input
            if (isValid) {
                foreach (INakedObjectActionParameter parm in actionContext.Action.Parameters) {
                    try {
                        INakedObject valueNakedObject = GetValue(parm.Specification, rawParms.ContainsKey(parm.Id) ? rawParms[parm.Id] : null);

                        orderedParms[parm.Id].ProposedNakedObject = valueNakedObject;

                        IConsent consent = parm.IsValid(actionContext.Target, valueNakedObject, framework.LifecycleManager, framework.Session);
                        if (!consent.IsAllowed) {
                            orderedParms[parm.Id].Reason = consent.Reason;
                            isValid = false;
                        }
                    }
                    catch (InvalidEntryException) {
                        isValid = false;
                        orderedParms[parm.Id].ErrorCause = Cause.WrongType;
                        orderedParms[parm.Id].Reason = "Invalid Entry"; // i18n 
                    }
                }
            }

            // check for validity of whole set, including any 'co-validation' involving multiple parameters
            if (isValid) {
                IConsent consent = actionContext.Action.IsParameterSetValid(framework.Session, actionContext.Target, orderedParms.Select(kvp => kvp.Value.ProposedNakedObject).ToArray(), framework.LifecycleManager);
                if (!consent.IsAllowed) {
                    actionContext.Reason = consent.Reason;
                    isValid = false;
                }
            }

            actionContext.VisibleParameters = orderedParms.Select(p => p.Value).ToArray();

            return isValid;
        }


        private IConsent IsOfCorrectType(IOneToManyAssociation property, PropertyContext context) {
            // todo this should probably be in the framework somewhere
            INakedObject collectionNakedObject = property.GetNakedObject(context.Target, framework.LifecycleManager);
            ITypeOfFacet facet = collectionNakedObject.GetTypeOfFacetFromSpec();

            var introspectableSpecification = facet.ValueSpec;
            var spec = framework.Metadata.GetSpecification(introspectableSpecification);
            if (context.ProposedNakedObject.Specification.IsOfType(spec)) {
                return new Allow();
            }
            return new Veto(string.Format("Not a suitable type; must be a {0}", introspectableSpecification.FullName));
        }

        private bool ConsentHandler(IConsent consent, Context.Context context, Cause cause) {
            if (consent.IsVetoed) {
                context.Reason = consent.Reason;
                context.ErrorCause = cause;
                return false;
            }
            return true;
        }

        private PropertyContext SetupPropertyContext(INakedObject nakedObject, string propertyName, object toAdd) {
            PropertyContext context = GetProperty(nakedObject, propertyName);
            context.ProposedValue = toAdd;
            context.ProposedNakedObject = framework.LifecycleManager.CreateAdapter(toAdd, null, null);
            return context;
        }

        private PropertyContextSurface ChangeCollection(PropertyContext context, Func<INakedObject, INakedObject, IConsent> validator, Action<INakedObject, INakedObject> mutator, ArgumentContext argument) {
            ValidateConcurrency(context.Target, argument.Digest);

            var property = (IOneToManyAssociation) context.Property;

            if (ConsentHandler(IsOfCorrectType(property, context), context, Cause.Other)) {
                if (ConsentHandler(IsCurrentlyMutable(context.Target), context, Cause.Immutable)) {
                    if (ConsentHandler(property.IsUsable(framework.Session, context.Target, framework.LifecycleManager), context, Cause.Disabled)) {
                        if (ConsentHandler(validator(context.Target, context.ProposedNakedObject), context, Cause.Other)) {
                            if (!argument.ValidateOnly) {
                                mutator(context.Target, context.ProposedNakedObject);
                            }
                        }
                    }
                }
            }
            context.Mutated = true;
            return context.ToPropertyContextSurface(this, framework);
        }

        private ActionResultContextSurface ExecuteAction(ActionContext actionContext, ArgumentsContext arguments) {
            ValidateConcurrency(actionContext.Target, arguments.Digest);

            var actionResultContext = new ActionResultContext {Target = actionContext.Target, ActionContext = actionContext};
            if (ConsentHandler(actionContext.Action.IsUsable(framework.Session, actionContext.Target, framework.LifecycleManager), actionResultContext, Cause.Disabled)) {
                if (ValidateParameters(actionContext, arguments.Values) && !arguments.ValidateOnly) {
                    INakedObject result = actionContext.Action.Execute(actionContext.Target, actionContext.VisibleParameters.Select(p => p.ProposedNakedObject).ToArray(), framework.LifecycleManager, framework.Session);
                    actionResultContext.Result = GetObjectContext(result);
                }
            }
            return actionResultContext.ToActionResultContextSurface(this, framework);
        }

        // TODO either move this into framework or (better?) add a VetoCause enum to Veto and use  
        private static IConsent IsCurrentlyMutable(INakedObject target) {
            bool isPersistent = target.ResolveState.IsPersistent();

            var immutableFacet = target.Specification.GetFacet<IImmutableFacet>();
            if (immutableFacet != null) {
                WhenTo when = immutableFacet.Value;
                if (when == WhenTo.UntilPersisted && !isPersistent) {
                    return new Veto(Resources.NakedObjects.FieldDisabledUntil);
                }
                if (when == WhenTo.OncePersisted && isPersistent) {
                    return new Veto(Resources.NakedObjects.FieldDisabledOnce);
                }
                INakedObjectSpecification tgtSpec = target.Specification;
                if (tgtSpec.IsAlwaysImmutable() || (tgtSpec.IsImmutableOncePersisted() && isPersistent)) {
                    return new Veto(Resources.NakedObjects.FieldDisabled);
                }
            }
            return new Allow();
        }


        private INakedObject GetValue(INakedObjectSpecification specification, object rawValue) {
            if (rawValue == null) {
                return null;
            }

            if (specification.IsParseable) {
                return specification.GetFacet<IParseableFacet>().ParseTextEntry(rawValue.ToString(), framework.LifecycleManager);
            }

            if (specification.IsCollection) {
                var elementSpec = specification.GetFacet<ITypeOfFacet>().ValueSpec;

                if (elementSpec.IsParseable) {
                    var elements = ((IEnumerable) rawValue).Cast<object>().Select(e => elementSpec.GetFacet<IParseableFacet>().ParseTextEntry(e.ToString(), framework.LifecycleManager)).ToArray();
                    var elementType = TypeUtils.GetType(elementSpec.FullName);
                    Type collType = typeof (List<>).MakeGenericType(elementType);
                    var collection = framework.LifecycleManager.CreateAdapter(Activator.CreateInstance(collType), null, null);

                    collection.Specification.GetFacet<ICollectionFacet>().Init(collection, elements);
                    return collection;
                }
            }


            return framework.LifecycleManager.CreateAdapter(rawValue, null, null);
        }

        private IConsent CanSetPropertyValue(PropertyContext context) {
            try {
                context.ProposedNakedObject = GetValue(context.Specification, context.ProposedValue);
                return new Allow();
            }
            catch (InvalidEntryException e) {
                return new Veto(e.Message);
            }
        }

        private static T MapErrors<T>(Func<T> f) {
            try {
                return f();
            }
            catch (NakedObjectsSurfaceException) {
                throw;
            }
            catch (Exception e) {
                throw SurfaceUtils.Map(e);
            }
        }

        private INakedObject GetObjectAsNakedObject(LinkObjectId objectId) {
            object obj = oidStrategy.GetDomainObjectByOid(objectId);
            return framework.LifecycleManager.CreateAdapter(obj, null, null);
        }


        private INakedObject GetServiceAsNakedObject(LinkObjectId serviceName) {
            object obj = oidStrategy.GetServiceByServiceName(serviceName);
            return framework.LifecycleManager.CreateAdapter(obj, null, null);
        }

        private ParameterContext[] FilterParmsForContributedActions(INakedObjectAction action, INakedObjectSpecification targetSpec, string uid) {
            INakedObjectActionParameter[] parms;
            if (action.IsContributedMethod && !action.OnType.Equals(targetSpec)) {
                var tempParms = new List<INakedObjectActionParameter>();

                bool skipped = false;
                foreach (INakedObjectActionParameter parameter in action.Parameters) {
                    // skip the first parm that matches the target. 
                    if (targetSpec.IsOfType(parameter.Specification) && !skipped) {
                        skipped = true;
                    }
                    else {
                        tempParms.Add(parameter);
                    }
                }

                parms = tempParms.ToArray();
            }
            else {
                parms = action.Parameters;
            }
            return parms.Select(p => new ParameterContext {
                Action = action,
                Parameter = p,
                OverloadedUniqueId = uid
            }).ToArray();
        }


        private Tuple<INakedObjectAction, string> GetActionInternal(string actionName, INakedObject nakedObject) {
            if (string.IsNullOrWhiteSpace(actionName)) {
                throw new BadRequestNOSException();
            }

            INakedObjectAction[] actions = nakedObject.Specification.GetActionLeafNodes().Where(p => p.IsVisible(framework.Session, nakedObject, framework.LifecycleManager)).ToArray();
            INakedObjectAction action = actions.SingleOrDefault(p => p.Id == actionName) ?? SurfaceUtils.GetOverloadedAction(actionName, nakedObject.Specification);

            if (action == null) {
                throw new ActionResourceNotFoundNOSException(actionName);
            }

            return new Tuple<INakedObjectAction, string>(action, SurfaceUtils.GetOverloadedUId(action, nakedObject.Specification));
        }


        private INakedObjectActionParameter GetParameterInternal(string actionName, string parmName, INakedObject nakedObject) {
            var actionAndUid = GetActionInternal(actionName, nakedObject);

            if (string.IsNullOrWhiteSpace(parmName) || string.IsNullOrWhiteSpace(parmName)) {
                throw new BadRequestNOSException();
            }
            INakedObjectActionParameter parm = actionAndUid.Item1.Parameters.SingleOrDefault(p => p.Id == parmName);

            if (parm == null) {
                // throw something;
            }

            return parm;
        }


        private ActionContext GetAction(string actionName, INakedObject nakedObject) {
            var actionAndUid = GetActionInternal(actionName, nakedObject);
            return new ActionContext {
                Target = nakedObject,
                Action = actionAndUid.Item1,
                VisibleParameters = FilterParmsForContributedActions(actionAndUid.Item1, nakedObject.Specification, actionAndUid.Item2),
                OverloadedUniqueId = actionAndUid.Item2
            };
        }


        private Tuple<ActionContext, INakedObjectSpecification> GetActionTypeInternal(string typeName, string actionName) {
            if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(actionName)) {
                throw new BadRequestNOSException();
            }

            INakedObjectSpecification spec = GetDomainTypeInternal(typeName);
            var actionAndUid = SurfaceUtils.GetActionandUidFromSpec(spec, actionName, typeName);

            var actionContext = new ActionContext {
                Action = actionAndUid.Item1,
                VisibleParameters = FilterParmsForContributedActions(actionAndUid.Item1, spec, actionAndUid.Item2),
                OverloadedUniqueId = actionAndUid.Item2
            };

            return new Tuple<ActionContext, INakedObjectSpecification>(actionContext, spec);
        }

        private Tuple<INakedObjectAction, INakedObjectSpecification, INakedObjectActionParameter, string> GetActionParameterTypeInternal(string typeName, string actionName, string parmName) {
            if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(actionName) || string.IsNullOrWhiteSpace(parmName)) {
                throw new BadRequestNOSException();
            }

            INakedObjectSpecification spec = GetDomainTypeInternal(typeName);
            Tuple<INakedObjectAction, string> actionAndUid = SurfaceUtils.GetActionandUidFromSpec(spec, actionName, typeName);

            INakedObjectActionParameter parm = actionAndUid.Item1.Parameters.SingleOrDefault(p => p.Id == parmName);

            if (parm == null) {
                throw new TypeActionParameterResourceNotFoundNOSException(parmName, actionName, typeName);
            }

            return new Tuple<INakedObjectAction, INakedObjectSpecification, INakedObjectActionParameter, string>(actionAndUid.Item1, spec, parm, actionAndUid.Item2);
        }


        private ActionContext GetInvokeActionOnObject(LinkObjectId objectId, string actionName) {
            INakedObject nakedObject = GetObjectAsNakedObject(objectId);
            return GetAction(actionName, nakedObject);
        }

        private ActionContext GetInvokeActionOnService(LinkObjectId serviceName, string actionName) {
            INakedObject nakedObject = GetServiceAsNakedObject(serviceName);
            return GetAction(actionName, nakedObject);
        }


        private ObjectContext GetObjectContext(INakedObject nakedObject) {
            if (nakedObject == null) {
                return null;
            }

            INakedObjectAction[] actions = nakedObject.Specification.GetActionLeafNodes().Where(p => p.IsVisible(framework.Session, nakedObject, framework.LifecycleManager)).ToArray();
            INakedObjectAssociation[] properties = nakedObject.Specification.Properties.Where(p => p.IsVisible(framework.Session, nakedObject, framework.LifecycleManager)).ToArray();

            return new ObjectContext(nakedObject) {
                VisibleActions = actions.Select(a => new {action = a, uid = SurfaceUtils.GetOverloadedUId(a, nakedObject.Specification)}).Select(a => new ActionContext {
                    Action = a.action,
                    Target = nakedObject,
                    VisibleParameters = FilterParmsForContributedActions(a.action, nakedObject.Specification, a.uid),
                    OverloadedUniqueId = a.uid
                }).ToArray(),
                VisibleProperties = properties.Select(p => new PropertyContext {
                    Property = p,
                    Target = nakedObject
                }).ToArray()
            };
        }


        private ObjectContext GetObjectInternal(LinkObjectId oid) {
            INakedObject nakedObject = GetObjectAsNakedObject(oid);
            return GetObjectContext(nakedObject);
        }

        private ObjectContext GetServiceInternal(LinkObjectId serviceName) {
            INakedObject nakedObject = GetServiceAsNakedObject(serviceName);
            return GetObjectContext(nakedObject);
        }

        private INakedObjectSpecification GetDomainTypeInternal(string domainTypeId) {
            try {
                var spec = (NakedObjectSpecificationWrapper) oidStrategy.GetSpecificationByLinkDomainType(domainTypeId);
                return spec.WrappedValue;
            }
            catch (Exception) {
                throw new TypeResourceNotFoundNOSException(domainTypeId);
            }
        }


        private ObjectContextSurface CreateObject(string typeName, ArgumentsContext arguments) {
            if (string.IsNullOrWhiteSpace(typeName)) {
                throw new BadRequestNOSException();
            }

            INakedObjectSpecification spec = GetDomainTypeInternal(typeName);
            INakedObject nakedObject = framework.LifecycleManager.CreateInstance(spec);

            return SetObject(nakedObject, arguments);
        }

        private class PropParmAdapter {
            private readonly INakedObjectsFramework framework;
            private readonly INakedObjectActionParameter parm;
            private readonly IOneToOneFeature prop;
            private readonly INakedObjectsSurface surface;

            private PropParmAdapter(object p, INakedObjectsSurface surface, INakedObjectsFramework framework) {
                this.surface = surface;
                this.framework = framework;
                if (p == null) {
                    throw new BadRequestNOSException();
                }
            }

            public PropParmAdapter(IOneToOneFeature prop, INakedObjectsSurface surface, INakedObjectsFramework framework)
                : this((object) prop, surface, framework) {
                this.prop = prop;
                CheckAutocompleOrConditional();
            }

            public PropParmAdapter(INakedObjectActionParameter parm, INakedObjectsSurface surface, INakedObjectsFramework framework)
                : this((object) parm, surface, framework) {
                this.parm = parm;
                CheckAutocompleOrConditional();
            }

            private bool IsAutoCompleteEnabled {
                get { return prop == null ? parm.IsAutoCompleteEnabled : prop.IsAutoCompleteEnabled; }
            }

            public INakedObjectSpecification Specification {
                get { return prop == null ? parm.Specification : prop.Specification; }
            }

            private Func<Tuple<string, INakedObjectSpecification>[]> GetChoicesParameters {
                get { return prop == null ? (Func<Tuple<string, INakedObjectSpecification>[]>) parm.GetChoicesParameters : prop.GetChoicesParameters; }
            }

            private Func<INakedObject, IDictionary<string, INakedObject>, ILifecycleManager, INakedObject[]> GetChoices {
                get { return prop == null ? (Func<INakedObject, IDictionary<string, INakedObject>, ILifecycleManager, INakedObject[]>) parm.GetChoices : prop.GetChoices; }
            }

            private Func<INakedObject, string, ILifecycleManager, INakedObject[]> GetCompletions {
                get { return prop == null ? (Func<INakedObject, string, ILifecycleManager, INakedObject[]>) parm.GetCompletions : prop.GetCompletions; }
            }

            private void CheckAutocompleOrConditional() {
                if (!(IsAutoCompleteEnabled || GetChoicesParameters().Any())) {
                    throw new BadRequestNOSException();
                }
            }

            public INakedObject[] GetList(INakedObject nakedObject, ArgumentsContext arguments) {
                return IsAutoCompleteEnabled ? GetAutocompleteList(nakedObject, arguments) : GetConditionalList(nakedObject, arguments);
            }

            private string CheckForMissingArgument(string key, object value, INakedObjectSpecification expectedType) {
                if (expectedType.IsParseable) {
                    var valueAsString = value as string;
                    return valueAsString == null || string.IsNullOrEmpty(valueAsString) ? string.Format("Missing argument {0}", key) : null;
                }
                return value == null ? string.Format("Missing argument {0}", key) : null;
            }

            private INakedObjectSpecificationSurface GetSpecificationWrapper(INakedObjectSpecification spec) {
                return new NakedObjectSpecificationWrapper(spec, surface, framework);
            }

            private INakedObject[] GetConditionalList(INakedObject nakedObject, ArgumentsContext arguments) {
                Tuple<string, INakedObjectSpecification>[] expectedParms = GetChoicesParameters();
                IDictionary<string, object> actualParms = arguments.Values;

                string[] expectedParmNames = expectedParms.Select(t => t.Item1).ToArray();
                string[] actualParmNames = actualParms.Keys.ToArray();

                if (expectedParmNames.Count() < actualParmNames.Count()) {
                    throw new BadRequestNOSException("Wrong number of conditional arguments");
                }

                if (!actualParmNames.All(expectedParmNames.Contains)) {
                    throw new BadRequestNOSException("Unrecognised conditional argument(s)");
                }

                Func<Tuple<string, INakedObjectSpecification>, object> getValue = ep => {
                    if (actualParms.ContainsKey(ep.Item1)) {
                        return actualParms[ep.Item1];
                    }
                    return ep.Item2.IsParseable ? "" : null;
                };


                var matchedParms = expectedParms.ToDictionary(ep => ep.Item1, ep => new {
                    expectedType = ep.Item2,
                    value = getValue(ep),
                    actualType = getValue(ep) == null ? null : framework.Metadata.GetSpecification(getValue(ep).GetType())
                });

                var errors = new List<ContextSurface>();

                var mappedArguments = new Dictionary<string, INakedObject>();

                foreach (var ep in expectedParms) {
                    string key = ep.Item1;
                    var mp = matchedParms[key];
                    object value = mp.value;
                    INakedObjectSpecification expectedType = mp.expectedType;
                    INakedObjectSpecification actualType = mp.actualType;

                    if (expectedType.IsParseable && actualType.IsParseable) {
                        string rawValue = value.ToString();

                        try {
                            mappedArguments[key] = expectedType.GetFacet<IParseableFacet>().ParseTextEntry(rawValue, framework.LifecycleManager);

                            errors.Add(new ChoiceContextSurface(key, GetSpecificationWrapper(expectedType)) {
                                ProposedValue = rawValue
                            });
                        }
                        catch (Exception e) {
                            errors.Add(new ChoiceContextSurface(key, GetSpecificationWrapper(expectedType)) {
                                Reason = e.Message,
                                ProposedValue = rawValue
                            });
                        }
                    }
                    else if (actualType != null && !actualType.IsOfType(expectedType)) {
                        errors.Add(new ChoiceContextSurface(key, GetSpecificationWrapper(expectedType)) {
                            Reason = string.Format("Argument is of wrong type is {0} expect {1}", actualType.FullName, expectedType.FullName),
                            ProposedValue = actualParms[ep.Item1]
                        });
                    }
                    else {
                        mappedArguments[key] = framework.LifecycleManager.CreateAdapter(value, null, null);

                        errors.Add(new ChoiceContextSurface(key, GetSpecificationWrapper(expectedType)) {
                            ProposedValue = getValue(ep)
                        });
                    }
                }

                if (errors.Any(e => !string.IsNullOrEmpty(e.Reason))) {
                    throw new BadRequestNOSException("Wrong type of conditional argument(s)", errors);
                }

                return GetChoices(nakedObject, mappedArguments, framework.LifecycleManager);
            }

            private INakedObject[] GetAutocompleteList(INakedObject nakedObject, ArgumentsContext arguments) {
                if (arguments.SearchTerm == null) {
                    throw new BadRequestNOSException("Missing or malformed search term");
                }
                return GetCompletions(nakedObject, arguments.SearchTerm, framework.LifecycleManager);
            }
        }

        #endregion

        private INakedObjectSpecificationSurface GetSpecificationWrapper(INakedObjectSpecification spec) {
            return new NakedObjectSpecificationWrapper(spec, this, framework);
        }

        private static bool IsGenericType(INakedObjectSpecification spec) {
            Type type = TypeUtils.GetType(spec.FullName);

            if (type != null) {
                return type.IsGenericType;
            }

            return false;
        }
    }
}