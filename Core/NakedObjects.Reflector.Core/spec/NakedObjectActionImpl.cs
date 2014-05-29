// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Facets.Actions.Contributed;
using NakedObjects.Architecture.Facets.Actions.Executed;
using NakedObjects.Architecture.Facets.Actions.Invoke;
using NakedObjects.Architecture.Interactions;
using NakedObjects.Architecture.Persist;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Security;
using NakedObjects.Architecture.Services;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core.Context;
using NakedObjects.Core.Util;
using NakedObjects.Reflector.Peer;

namespace NakedObjects.Reflector.Spec {
    public class NakedObjectActionImpl : NakedObjectMemberSessionAware, INakedObjectAction {
        private static readonly ILog Log;
        private readonly INakedObjectActionPeer nakedObjectActionPeer;
        private readonly object parameterLock = true;
        private INakedObjectActionParameter[] parameters;

        static NakedObjectActionImpl() {
            Log = LogManager.GetLogger(typeof (NakedObjectActionImpl));
        }

        public NakedObjectActionImpl(string methodId, INakedObjectActionPeer nakedObjectActionPeer)
            : base(methodId, nakedObjectActionPeer) {
            this.nakedObjectActionPeer = nakedObjectActionPeer;
        }

        private IActionInvocationFacet ActionInvocationFacet {
            get { return nakedObjectActionPeer.GetFacet<IActionInvocationFacet>(); }
        }

        #region INakedObjectAction Members

        public virtual INakedObjectSpecification ReturnType {
            get { return ActionInvocationFacet.ReturnType; }
        }

        public virtual INakedObjectSpecification OnType {
            get { return ActionInvocationFacet.OnType; }
        }

        public virtual INakedObjectAction[] Actions {
            get { return new INakedObjectAction[0]; }
        }

        public override Type[] FacetTypes {
            get { return nakedObjectActionPeer.FacetTypes; }
        }

        public override IIdentifier Identifier {
            get { return nakedObjectActionPeer.Identifier; }
        }

        public virtual int ParameterCount {
            get { return nakedObjectActionPeer.Parameters.Length; }
        }

        public virtual Target Target {
            get {
                Architecture.Facets.Where executeWhere = GetFacet<IExecutedFacet>().ExecutedWhere();
                if (executeWhere == Architecture.Facets.Where.Locally) {
                    return Target.Local;
                }
                if (executeWhere == Architecture.Facets.Where.Remotely) {
                    return Target.Remote;
                }
                if (executeWhere == Architecture.Facets.Where.Default) {
                    return Target.Default;
                }
                throw new UnknownTypeException(executeWhere);
            }
        }

        public virtual bool IsContributedMethod {
            get {
                if (OnType.IsService && ParameterCount > 0 &&
                    (!ContainsFacet(typeof (INotContributedActionFacet)) || !GetFacet<INotContributedActionFacet>().NeverContributed())) {
                    return Parameters.Any(p => p.IsObject || p.IsCollection);
                }
                return false;
            }
        }


        public bool IsContributedTo(INakedObjectSpecification spec) {
            return IsContributedMethod
                   && Parameters.Any(parm => ContributeTo(parm.Specification, spec))
                   && !(IsCollection(spec) && IsCollection(ReturnType));
        }

        public bool IsFinderMethod {
            get { return HasReturn() && !ContainsFacet(typeof (IExcludeFromFindMenuFacet)); }
        }

        public virtual bool PromptForParameters(INakedObject nakedObject) {
            if (IsContributedMethod && !nakedObject.Specification.IsService) {
                return ParameterCount > 1 || !IsContributedTo(parameters[0].Specification);
            }
            return ParameterCount > 0;
        }

        /// <summary>
        ///     Always returns <c>null</c>
        /// </summary>
        public override INakedObjectSpecification Specification {
            get { return null; }
        }

        public virtual INakedObject Execute(INakedObject nakedObject, INakedObject[] parameterSet) {
            Log.DebugFormat("Execute action {0}.{1}", nakedObject, Id);
            INakedObject[] parms = RealParameters(nakedObject, parameterSet);
            INakedObject target = RealTarget(nakedObject);
            return GetFacet<IActionInvocationFacet>().Invoke(target, parms);
        }

        public virtual INakedObject RealTarget(INakedObject target) {
            if (target == null) {
                return FindService();
            }
            if (target.Specification.IsService) {
                return target;
            }
            if (IsContributedMethod) {
                return FindService();
            }
            return target;
        }

        public override bool ContainsFacet(Type facetType) {
            return nakedObjectActionPeer.ContainsFacet(facetType);
        }

        public override IFacet GetFacet(Type type) {
            return nakedObjectActionPeer.GetFacet(type);
        }

        public override IFacet[] GetFacets(IFacetFilter filter) {
            return nakedObjectActionPeer.GetFacets(filter);
        }

        public override void AddFacet(IFacet facet) {
            nakedObjectActionPeer.AddFacet(facet);
        }

        public override void RemoveFacet(IFacet facet) {
            nakedObjectActionPeer.RemoveFacet(facet);
        }

        public override void RemoveFacet(Type facetType) {
            nakedObjectActionPeer.RemoveFacet(facetType);
        }


        /// <summary>
        ///     Build lazily by <see cref="GetParameters" />
        /// </summary>
        // TODO :REVIEW is it a good idea to lazily load this as it a static object and more than one thread might call
        public virtual INakedObjectActionParameter[] Parameters {
            get {
                lock (parameterLock) {
                    if (parameters == null) {
                        var list = new List<INakedObjectActionParameter>();
                        INakedObjectActionParamPeer[] paramPeers = nakedObjectActionPeer.Parameters;
                        for (int i = 0; i < paramPeers.Length; i++) {
                            INakedObjectSpecification specification = paramPeers[i].Specification;
                            if (specification.IsParseable) {
                                list.Add(new NakedObjectActionParameterParseable(i, this, paramPeers[i]));
                            }
                            else if (specification.IsObject) {
                                list.Add(new OneToOneActionParameterImpl(i, this, paramPeers[i]));
                            }
                            else if (specification.IsCollection) {
                                list.Add(new OneToManyActionParameterImpl(i, this, paramPeers[i]));
                            }
                            else {
                                throw new UnknownTypeException(specification);
                            }
                        }
                        parameters = list.ToArray();
                    }
                    return parameters;
                }
            }
        }

        public virtual INakedObjectActionParameter[] GetParameters(INakedObjectActionParameterFilter filter) {
            return Parameters.Where(filter.Accept).ToArray();
        }

        public virtual NakedObjectActionType ActionType {
            get { return NakedObjectActionType.User; }
        }

        /// <summary>
        ///     Returns true if the represented action returns something, else returns false
        /// </summary>
        public virtual bool HasReturn() {
            return ReturnType != null;
        }

        /// <summary>
        ///     Checks declarative constraints, and then checks imperatively.
        /// </summary>
        public virtual IConsent IsParameterSetValid(INakedObject nakedObject, INakedObject[] parameterSet) {
            InteractionContext ic;
            var buf = new InteractionBuffer();
            if (parameterSet != null) {
                INakedObject[] parms = RealParameters(nakedObject, parameterSet);
                for (int i = 0; i < parms.Length; i++) {
                    ic = InteractionContext.ModifyingPropParam(NakedObjectsContext.Session, false, RealTarget(nakedObject), Identifier, parameterSet[i]);
                    InteractionUtils.IsValid(GetParameter(i), ic, buf);
                }
            }
            INakedObject target = RealTarget(nakedObject);
            ic = InteractionContext.InvokingAction(NakedObjectsContext.Session, false, target, Identifier, parameterSet);
            InteractionUtils.IsValid(this, ic, buf);
            return InteractionUtils.IsValid(buf);
        }

        public override IConsent IsUsable(ISession session, INakedObject target) {
            InteractionContext ic = InteractionContext.InvokingAction(session, false, RealTarget(target), Identifier, new[] {target});
            return InteractionUtils.IsUsable(this, ic);
        }

        public override bool IsVisible(ISession session, INakedObject target) {
            return base.IsVisible(session, RealTarget(target));
        }

        private bool ContributeTo(INakedObjectSpecification parmSpec, INakedObjectSpecification contributeeSpec) {
            var ncf = GetFacet<INotContributedActionFacet>();

            if (ncf == null) {
                return contributeeSpec.IsOfType(parmSpec);
            }

            return contributeeSpec.IsOfType(parmSpec) && !ncf.NotContributedTo(contributeeSpec);
        }

        private bool IsCollection(INakedObjectSpecification spec) {
            return spec.IsCollection && !spec.IsParseable;
        }

        #endregion

        public INakedObject[] RealParameters(INakedObject target, INakedObject[] parameterSet) {
            return parameterSet ?? (IsContributedMethod ? new[] {target} : new INakedObject[0]);
        }

        private bool FindServiceOnSpecOrSpecSuperclass(IHierarchical spec) {
            if (spec == null) {
                return false;
            }
            if (spec == OnType) {
                return true;
            }
            return FindServiceOnSpecOrSpecSuperclass(spec.Superclass);
        }

        private INakedObject FindService() {
            foreach (INakedObject serviceAdapter in NakedObjectsContext.ObjectPersistor.GetServices(ServiceTypes.Menu | ServiceTypes.Contributor)) {
                if (FindServiceOnSpecOrSpecSuperclass(serviceAdapter.Specification)) {
                    return serviceAdapter;
                }
            }
            throw new FindObjectException("failed to find service for action " + Name);
        }

        private INakedObjectActionParameter GetParameter(int position) {
            if (position >= Parameters.Length) {
                throw new ArgumentException("GetParameter(int): only " + Parameters.Length + " parameters, position=" + position);
            }
            return Parameters[position];
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("Action [");
            sb.Append(base.ToString());
            sb.Append(",type=");
            sb.Append(ActionType);
            sb.Append(",returns=");
            sb.Append(ReturnType);
            sb.Append(",parameters={");
            for (int i = 0; i < ParameterCount; i++) {
                if (i > 0) {
                    sb.Append(",");
                }
                sb.Append(Parameters[i].Specification.ShortName);
            }
            sb.Append("}]");
            return sb.ToString();
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}