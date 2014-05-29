// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using NakedObjects.Surface;
using NakedObjects.Surface.Utility;
using RestfulObjects.Snapshot.Constants;
using RestfulObjects.Snapshot.Utility;

namespace RestfulObjects.Snapshot.Representations {
    [DataContract]
    public class CollectionTypeRepresentation : MemberTypeRepresentation {
        protected CollectionTypeRepresentation(HttpRequestMessage req, PropertyTypeContextSurface propertyContext, RestControlFlags flags) : base(req, propertyContext, flags) {
            SetScalars(propertyContext);
            SetLinks(req, propertyContext);
        }

        [DataMember(Name = JsonPropertyNames.PluralName)]
        public string PluralName { get; set; }

        private void SetScalars(PropertyTypeContextSurface propertyContext) {
            PluralName = propertyContext.Property.Specification.PluralName();
        }

        private void SetLinks(HttpRequestMessage req, PropertyTypeContextSurface propertyContext) {
            IList<LinkRepresentation> tempLinks = CreateLinks(req, propertyContext);
            tempLinks.Add(LinkRepresentation.Create(new DomainTypeRelType(RelValues.ReturnType, new UriMtHelper(req, propertyContext.Property)), Flags));
            tempLinks.Add(LinkRepresentation.Create(new DomainTypeRelType(RelValues.ElementType, new UriMtHelper(req, propertyContext.Property.Specification)), Flags));
            Links = tempLinks.ToArray();
        }

        public new static CollectionTypeRepresentation Create(HttpRequestMessage req, PropertyTypeContextSurface propertyContext, RestControlFlags flags) {
            return new CollectionTypeRepresentation(req, propertyContext, flags);
        }
    }
}