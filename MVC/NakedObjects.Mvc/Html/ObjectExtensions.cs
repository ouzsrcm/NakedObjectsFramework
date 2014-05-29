﻿// Copyright © Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facets.Presentation;
using NakedObjects.Architecture.Resolve;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core.Context;
using NakedObjects.Core.Persist;
using NakedObjects.Core.Util;
using NakedObjects.Resources;
using NakedObjects.Web.Mvc.Models;

namespace NakedObjects.Web.Mvc.Html {
    public static class ObjectExtensions {
        /// <summary>
        ///     Get the object id
        /// </summary>
        public static MvcHtmlString GetObjectId(this HtmlHelper html, object model) {
            Assert.AssertFalse("Cannot get Adapter for Adapter", model is INakedObject);
            INakedObject nakedObject = FrameworkHelper.GetNakedObject(model);
            return MvcHtmlString.Create(FrameworkHelper.GetObjectId(nakedObject));
        }

        /// <summary>
        ///     Get the object type in a format suitable for use as a css id
        /// </summary>
        public static MvcHtmlString ObjectTypeAsCssId(this HtmlHelper html, object model) {
            if (model.GetType().IsGenericType) {
                var gType = new StringBuilder(FrameworkHelper.GetObjectType(model.GetType().GetGenericTypeDefinition()));

                gType.Append("[[");
                foreach (Type gTypeParm in model.GetType().GetGenericArguments()) {
                    gType.Append(FrameworkHelper.GetObjectType(gTypeParm)).Append(", ");
                }
                gType.Remove(gType.Length - 2, 2);
                gType.Append("]]");

                return MvcHtmlString.Create(gType.ToString());
            }

            return MvcHtmlString.Create(FrameworkHelper.GetObjectType(model));
        }

        private static string GetPresentationHint(object model) {
            INakedObject nakedObject = FrameworkHelper.GetNakedObject(model);
            var facet = nakedObject.Specification.GetFacet<IPresentationHintFacet>();
            return facet == null ? "" : " " + facet.Value;
        }

        /// <summary>
        ///     Get classes for an object view
        /// </summary>
        public static MvcHtmlString ObjectViewClass(this HtmlHelper html, object model) {
            return MvcHtmlString.Create(IdHelper.ObjectViewName + GetPresentationHint(model));
        }

        /// <summary>
        ///     Get classes for an view model edit 
        /// </summary>
        public static MvcHtmlString ViewModelClass(this HtmlHelper html, object model) {
            return MvcHtmlString.Create(IdHelper.ViewModelName + GetPresentationHint(model));
        }

        /// <summary>
        ///     Get classes for an object edit
        /// </summary>
        public static MvcHtmlString ObjectEditClass(this HtmlHelper html, object model) {
            return MvcHtmlString.Create(IdHelper.ObjectEditName + GetPresentationHint(model));
        }

        /// <summary>
        ///     Indicate if object has any visible fields
        /// </summary>
        public static bool ObjectHasVisibleFields(this HtmlHelper html, object domainObject) {
            INakedObject nakedObject = FrameworkHelper.GetNakedObject(domainObject);
            return nakedObject.Specification.Properties.Any(p => p.IsVisible(NakedObjectsContext.Session, nakedObject));
        }


        /// <summary>
        ///     Indicate if object is a not persistent  object
        /// </summary>
        public static bool ObjectIsNotPersistent(this HtmlHelper html, object domainObject) {
            INakedObject nakedObject = FrameworkHelper.GetNakedObject(domainObject);
            return nakedObject.IsNotPersistent();
        }

        /// <summary>
        ///     Indicate if object is a transient object
        /// </summary>
        public static bool ObjectIsTransient(this HtmlHelper html, object domainObject) {
            INakedObject nakedObject = FrameworkHelper.GetNakedObject(domainObject);
            return nakedObject.ResolveState.IsTransient();
        }

        /// <summary>
        ///     Indicate if object is a transient object
        /// </summary>
        public static string TransientFlag(this HtmlHelper html, object domainObject) {
            INakedObject nakedObject = FrameworkHelper.GetNakedObject(domainObject);
            return nakedObject.ResolveState.IsTransient() ? " " + IdHelper.TransientName : "";
        }

        /// <summary>
        ///     Display name of object
        /// </summary>
        public static MvcHtmlString ObjectTitle(this HtmlHelper html, object model) {
            INakedObject nakedObject = PersistorUtils.CreateAdapter(model);
            return html.ObjectTitle(nakedObject);
        }

        public static MvcHtmlString ObjectTitle(this HtmlHelper html, INakedObject nakedObject) {
            string title = nakedObject.TitleString();
            return MvcHtmlString.Create(string.IsNullOrWhiteSpace(title) ? nakedObject.Specification.UntitledName : title);
        }


        /// <summary>
        ///     Display name of object with icon
        /// </summary>
        public static MvcHtmlString Object(this HtmlHelper html, object model) {
            INakedObject nakedObject = PersistorUtils.CreateAdapter(model);
            string title = nakedObject.Specification.IsCollection ? GetCollectionTitle(nakedObject, html) : nakedObject.TitleString();
            title = string.IsNullOrWhiteSpace(title) ? nakedObject.Specification.UntitledName : title;
            return CommonHtmlHelper.WrapInDiv(html.ObjectIcon(nakedObject) + title, IdHelper.ObjectName);
        }

        public static MvcHtmlString ActionResult(this HtmlHelper html, ActionResultModel model) {
            INakedObject nakedObject = PersistorUtils.CreateAdapter(model.Result);
            string title = GetCollectionTitle(nakedObject, html);
            title = model.Action.Name + ": " + (string.IsNullOrWhiteSpace(title) ? nakedObject.Specification.UntitledName : title);
            return CommonHtmlHelper.WrapInDiv(title, IdHelper.ObjectName);
        }

        private static string GetCollectionTitle(INakedObject nakedObject, HtmlHelper html) {
            int pageSize, maxPage, currentPage, total;
            int count = nakedObject.GetAsEnumerable().Count();
            if (!html.GetPagingValues(out pageSize, out maxPage, out currentPage, out total)) {
                total = count;
            }

            string queryInd = nakedObject.Specification.IsQueryable ? MvcUi.QueryResult + ": " : "";
            int viewSize = count;

            INakedObjectSpecification typeSpec = nakedObject.GetTypeOfFacetFromSpec().ValueSpec;
            string type = total == 1 ? typeSpec.SingularName : typeSpec.PluralName;

            return queryInd + string.Format(MvcUi.ViewingNofXType, viewSize, total, type);
        }

        /// <summary>
        ///     Display link to object with icon
        /// </summary>
        public static MvcHtmlString Object(this HtmlHelper html, string linkText, string actionName, object model) {
            return CommonHtmlHelper.WrapInDiv(html.ObjectIconAndLink(linkText, actionName, model), IdHelper.ObjectName);
        }

        public static MvcHtmlString Tab(this HtmlHelper html, string linkText, string actionName, object model) {
            return CommonHtmlHelper.WrapInDiv(html.ObjectIconAndLink(linkText, actionName, model, true), "nof-tab");
        }

        #region name 

        /// <summary>
        ///     Display name (Title) of object
        /// </summary>
        public static MvcHtmlString Name<TModel>(this HtmlHelper<TModel> html) {
            if (html.ViewData.Model == null) {
                throw new ArgumentException("html");
            }
            return html.Name(html.ViewData.Model);
        }

        /// <summary>
        ///     Display name (Title) of object
        /// </summary>
        public static MvcHtmlString Name<TModel>(this HtmlHelper html, TModel model) {
            INakedObject nakedObject = FrameworkHelper.GetNakedObject(model);
            return MvcHtmlString.Create(nakedObject.TitleString());
        }

        #endregion

        #region description 

        /// <summary>
        ///     Display description of object
        /// </summary>
        public static MvcHtmlString Description<TModel>(this HtmlHelper<TModel> html) {
            if (html.ViewData.Model == null) {
                throw new ArgumentException("html");
            }
            return html.Description(html.ViewData.Model);
        }

        /// <summary>
        ///     Display description of object
        /// </summary>
        public static MvcHtmlString Description<TModel>(this HtmlHelper html, TModel model) {
            INakedObject nakedObject = FrameworkHelper.GetNakedObject(model);
            return MvcHtmlString.Create(nakedObject.Specification.Description);
        }

        #endregion

        #region iconname 

        /// <summary>
        ///     Get icon name from object
        /// </summary>
        public static MvcHtmlString IconName<TModel>(this HtmlHelper<TModel> html) {
            if (html.ViewData.Model == null) {
                throw new ArgumentException("html");
            }
            return html.IconName(html.ViewData.Model);
        }

        /// <summary>
        ///     Get icon name from object
        /// </summary>
        public static MvcHtmlString IconName<TModel>(this HtmlHelper html, TModel model) {
            INakedObject nakedObject = FrameworkHelper.GetNakedObject(model);
            return MvcHtmlString.Create(nakedObject.Specification.GetIconName(nakedObject));
        }

        #endregion

        #region typename 

        /// <summary>
        ///     Get short type name from object
        /// </summary>
        public static MvcHtmlString TypeName<TModel>(this HtmlHelper<TModel> html) {
            if (html.ViewData.Model == null) {
                throw new ArgumentException("html");
            }
            return html.TypeName(html.ViewData.Model);
        }

        /// <summary>
        ///     Get short type name from object
        /// </summary>
        public static MvcHtmlString TypeName<TModel>(this HtmlHelper html, TModel model) {
            INakedObject nakedObject = FrameworkHelper.GetNakedObject(model);
            return MvcHtmlString.Create(nakedObject.Specification.ShortName);
        }

        #endregion
    }
}