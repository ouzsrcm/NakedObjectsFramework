﻿// Copyright © Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System.Collections;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Web.Mvc.Html;

namespace NakedObjects.Web.Mvc.Models {
    public class FindViewModel {
        public enum ViewTypes {
            Edit,
            Dialog
        } ;

        public ViewTypes ViewType {
            get { return ContextAction == null ? ViewTypes.Edit : ViewTypes.Dialog; }
        }

        public IEnumerable ActionResult { get; set; }
        public object TargetObject { get; set; }
        public INakedObjectAction TargetAction { get; set; }

        public object ContextObject { get; set; }
        public INakedObjectAction ContextAction { get; set; }

        public string PropertyName { get; set; }

        public string DialogClass() {

            if (ViewType == ViewTypes.Dialog) {
                return ContextAction.ReturnType.IsFile() ? IdHelper.DialogNameFileClass : IdHelper.DialogNameClass;
            }

            return IdHelper.EditName;
        }
    }
}