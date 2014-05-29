// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;

namespace NakedObjects.Architecture.Reflect {
    public abstract class ConsentAbstract : IConsent {
        private readonly Exception exception;
        private readonly string reason;

        protected internal ConsentAbstract() {
            exception = null;
            reason = null;
        }

        protected internal ConsentAbstract(string reason) {
            exception = null;
            this.reason = reason;
        }

        protected internal ConsentAbstract(Exception exception) {
            this.exception = exception;
            reason = exception != null ? exception.Message : null;
        }

        #region IConsent Members

        /// <summary>
        ///     Returns the permission's reason
        /// </summary>
        public virtual string Reason {
            get { return reason ?? ""; }
        }

        public virtual Exception Exception {
            get { return exception; }
        }

        /// <summary>
        ///     Returns true if this object is giving permission
        /// </summary>
        public abstract bool IsAllowed { get; }

        /// <summary>
        ///     Returns <c>true</c> if this object is NOT giving permission
        /// </summary>
        public abstract bool IsVetoed { get; }

        #endregion

        /// <summary>
        ///     Returns an Allow (Allow.Default) object if true; Veto (Veto.Default) if false
        /// </summary>
        public static IConsent GetAllow(bool allow) {
            return allow ? (IConsent) Allow.Default : Veto.Default;
        }

        /// <summary>
        ///     Returns a new Allow object if <c>allow</c> is <c>true</c>; a new Veto if <c>false</c>. The respective reason
        ///     is passed to the newly created object.
        /// </summary>
        public static IConsent Create(bool allow, string reasonAllowed, string reasonVeteod) {
            return allow ? (IConsent) new Allow(reasonAllowed) : new Veto(reasonVeteod);
        }

        public static IConsent Create(string vetoReason) {
            return vetoReason == null ? (IConsent) Allow.Default : new Veto(vetoReason);
        }

        public override string ToString() {
            return "Permission [type=" + (IsVetoed ? "VETOED" : "ALLOWED") + ", reason=" + reason + "]";
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}