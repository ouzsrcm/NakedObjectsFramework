// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Common.Logging;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core.Resolve;
using NakedObjects.Core.Util;
using NakedObjects.UtilInternal;

namespace NakedObjects.Core.Container {
    public sealed class DomainObjectContainer : IDomainObjectContainer, IInternalAccess {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DomainObjectContainer));
        private readonly INakedObjectsFramework framework;

        public DomainObjectContainer(INakedObjectsFramework framework) {
            this.framework = framework;
        }

        #region IDomainObjectContainer Members

        public IQueryable<T> Instances<T>() where T : class {
            return framework.Persistor.Instances<T>();
        }

        public IQueryable Instances(Type type) {
            return framework.Persistor.Instances(type);
        }

        public void DisposeInstance(object persistentObject) {
            if (persistentObject == null) {
                throw new ArgumentException(Log.LogAndReturn(Resources.NakedObjects.DisposeReferenceError));
            }
            INakedObjectAdapter adapter = framework.NakedObjectManager.GetAdapterFor(persistentObject);
            if (!IsPersistent(persistentObject)) {
                throw new DisposeFailedException(Log.LogAndReturn(string.Format(Resources.NakedObjects.NotPersistentMessage, adapter)));
            }
            framework.Persistor.DestroyObject(adapter);
        }

        public T GetService<T>() {
            return framework.ServicesManager.GetServices().Select(no => no.Object).OfType<T>().SingleOrDefault();
        }

        public IPrincipal Principal => framework.Session.Principal;

        public void InformUser(string message) {
            framework.MessageBroker.AddMessage(message);
        }

        public bool IsPersistent(object obj) {
            return !AdapterFor(obj).Oid.IsTransient;
        }

        public void Persist<T>(ref T transientObject) {
            INakedObjectAdapter adapter = framework.NakedObjectManager.GetAdapterFor(transientObject);
            if (IsPersistent(transientObject)) {
                throw new PersistFailedException(Log.LogAndReturn(string.Format(Resources.NakedObjects.AlreadyPersistentMessage, adapter)));
            }
            Validate(adapter);
            framework.LifecycleManager.MakePersistent(adapter);
            transientObject = adapter.GetDomainObject<T>();
        }

        public T NewTransientInstance<T>() where T : new() {
            return (T) NewTransientInstance(typeof (T));
        }

        public T NewViewModel<T>() where T : IViewModel, new() {
            return (T) NewViewModel(typeof (T));
        }

        public IViewModel NewViewModel(Type type) {
            var spec = (IObjectSpec) framework.MetamodelManager.GetSpecification(type);
            if (spec.IsViewModel) {
                return framework.LifecycleManager.CreateViewModel(spec).GetDomainObject<IViewModel>();
            }
            return null;
        }

        public object NewTransientInstance(Type type) {
            var spec = (IObjectSpec) framework.MetamodelManager.GetSpecification(type);
            return framework.LifecycleManager.CreateInstance(spec).Object;
        }

        public void ObjectChanged(object obj) {
            if (obj != null) {
                INakedObjectAdapter adapter = AdapterFor(obj);
                Validate(adapter);
                framework.Persistor.ObjectChanged(adapter, framework.LifecycleManager, framework.MetamodelManager);
            }
        }

        public void RaiseError(string message) {
            throw new DomainException(Log.LogAndReturn(message));
        }

        public void Refresh(object obj) {
            INakedObjectAdapter nakedObjectAdapter = AdapterFor(obj);
            framework.Persistor.Refresh(nakedObjectAdapter);
            ObjectChanged(obj);
        }

        public void Resolve(object parent) {
            INakedObjectAdapter adapter = AdapterFor(parent);
            if (adapter.ResolveState.IsResolvable()) {
                framework.Persistor.ResolveImmediately(adapter);
            }
        }

        public void Resolve(object parent, object field) {
            if (field == null) {
                Resolve(parent);
            }
        }

        public void WarnUser(string message) {
            framework.MessageBroker.AddWarning(message);
        }

        public void AbortCurrentTransaction() {
            framework.TransactionManager.UserAbortTransaction();
        }

        #endregion

        #region IInternalAccess Members

        public PropertyInfo[] GetKeys(Type type) {
            return framework.Persistor.GetKeys(type);
        }

        public object FindByKeys(Type type, object[] keys) {
            return framework.Persistor.FindByKeys(type, keys).GetDomainObject();
        }

        #endregion

        private void Validate(INakedObjectAdapter adapter) {
            if (adapter.Spec.ContainsFacet<IValidateProgrammaticUpdatesFacet>()) {
                string state = adapter.ValidToPersist();
                if (state != null) {
                    throw new PersistFailedException(Log.LogAndReturn(string.Format(Resources.NakedObjects.PersistStateError, adapter.Spec.ShortName, adapter.TitleString(), state)));
                }
            }
        }

        private INakedObjectAdapter AdapterFor(object obj) {
            return framework.NakedObjectManager.CreateAdapter(obj, null, null);
        }

        #region Titles

        public ITitleBuilder NewTitleBuilder() {
            return new TitleBuilderImpl(this);
        }

        public ITitleBuilder NewTitleBuilder(object obj, string defaultTitle = null) {
            return new TitleBuilderImpl(this, obj, defaultTitle);
        }

        public ITitleBuilder NewTitleBuilder(string text) {
            return new TitleBuilderImpl(this, text);
        }

        public string TitleOf(object obj, string format = null) {
            var adapter = AdapterFor(obj);
            if (format == null) {
                return adapter.TitleString();
            }
            return adapter.Spec.GetFacet<ITitleFacet>().GetTitleWithMask(format, adapter, framework.NakedObjectManager);
        }

        #endregion
    }

    // Copyright (c) Naked Objects Group Ltd.
}