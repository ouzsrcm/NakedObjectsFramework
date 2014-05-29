﻿// Copyright © Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Facets.Collections.Modify;
using NakedObjects.Architecture.Util;
using NakedObjects.Testing;

namespace NakedObjects.Reflector.DotNet.Facets.Collections {
    [TestFixture]
    public class CollectionFacetsTest {
        private readonly ProgrammableTestSystem testSystem = new ProgrammableTestSystem();
        private readonly FacetHolderImpl facetHolder = new FacetHolderImpl();

        private static void Size(ICollectionFacet collectionFacet, INakedObject collection) {
            Assert.AreEqual(2, collectionFacet.AsEnumerable(collection).Count());
        }

        private static void ValidateCollection(ICollectionFacet collectionFacet, INakedObject collection, IEnumerable<object> objects) {
            IEnumerable<INakedObject> collectionAsEnumerable = collectionFacet.AsEnumerable(collection);
            Assert.AreEqual(collectionAsEnumerable.Count(), objects.Count());
            IEnumerable<Tuple<object, object>> zippedCollections = collectionAsEnumerable.Zip(objects, (no, o1) => new Tuple<object, object>(no.Object, o1));
            zippedCollections.ForEach(t => Assert.AreSame(t.Item1, t.Item2));
        }

        private static void FirstElement(ICollectionFacet collectionFacet, INakedObject collection, object first) {
            Assert.AreSame(first, collectionFacet.AsEnumerable(collection).First().Object);
        }

        private void Contains(ICollectionFacet collectionFacet, INakedObject collection, object first, object second) {
            Assert.IsTrue(collectionFacet.Contains(collection, testSystem.AdapterFor(first)));
            Assert.IsFalse(collectionFacet.Contains(collection, testSystem.AdapterFor(second)));
        }

        private void Init(ICollectionFacet collectionFacet, INakedObject collection, IEnumerable<object> data1, IEnumerable<object> data2) {
            collectionFacet.Init(collection, data1.Cast<object>().Select(x => testSystem.AdapterFor(x)).ToArray());
            ValidateCollection(collectionFacet, collection, data1);
            collectionFacet.Init(collection, data2.Cast<object>().Select(x => testSystem.AdapterFor(x)).ToArray());
            ValidateCollection(collectionFacet, collection, data2);
        }

        private void Page(ICollectionFacet testArrayFacet, INakedObject collection, object first) {
            INakedObject pagedCollection = testArrayFacet.Page(1, 1, collection, false);
            var pagedCollectionFacet = new DotNetGenericIEnumerableFacet<object>(facetHolder, typeof (object), false);

            Assert.IsTrue(pagedCollectionFacet.AsEnumerable(pagedCollection).Count() == 1);
            Assert.AreSame(pagedCollectionFacet.AsEnumerable(pagedCollection).First().Object, first);
        }

        [Test]
        public void ArrayContains() {
            var testArray = new[] {"element1", "element2"};
            var testArrayFacet = new DotNetArrayFacet(facetHolder);
            INakedObject testAdaptedArray = testSystem.AdapterFor(testArray);
            Contains(testArrayFacet, testAdaptedArray, "element1", "element3");
        }


        [Test]
        public void ArrayFirstElement() {
            var testArray = new[] {"element1", "element2"};
            var testArrayFacet = new DotNetArrayFacet(facetHolder);
            INakedObject testAdaptedArray = testSystem.AdapterFor(testArray);
            FirstElement(testArrayFacet, testAdaptedArray, "element1");
        }

        [Test]
        public void ArrayGetEnumeratorFor() {
            var testArray = new[] {"element1", "element2"};
            var testArrayFacet = new DotNetArrayFacet(facetHolder);
            INakedObject testAdaptedArray = testSystem.AdapterFor(testArray);
            ValidateCollection(testArrayFacet, testAdaptedArray, testArray);
        }

        [Test]
        public void ArrayInit() {
            var testArray = new[] {"element1", "element2"};
            var testArray1 = new[] {"element2", "element3"};
            var testArrayFacet = new DotNetArrayFacet(facetHolder);
            INakedObject testAdaptedArray = testSystem.AdapterFor(testArray);
            Init(testArrayFacet, testAdaptedArray, testArray, testArray1);
        }

        [Test]
        public void ArrayInitAllEmpty() {
            var testArray = new string[] {};
            var testArray1 = new string[] {};
            var testArrayFacet = new DotNetArrayFacet(facetHolder);
            INakedObject testAdaptedArray = testSystem.AdapterFor(testArray);
            Init(testArrayFacet, testAdaptedArray, testArray, testArray1);
        }

        [Test]
        public void ArrayInitEmpty() {
            var testArray = new string[] {};
            var testArray1 = new[] {"element2", "element3"};
            var testArrayFacet = new DotNetArrayFacet(facetHolder);
            INakedObject testAdaptedArray = testSystem.AdapterFor(testArray);
            Init(testArrayFacet, testAdaptedArray, testArray, testArray1);
        }

        [Test]
        public void ArrayInitToEmpty() {
            var testArray = new[] {"element1", "element2"};
            var testArray1 = new string[] {};
            var testArrayFacet = new DotNetArrayFacet(facetHolder);
            INakedObject testAdaptedArray = testSystem.AdapterFor(testArray);
            Init(testArrayFacet, testAdaptedArray, testArray, testArray1);
        }

        [Test]
        public void ArrayPage() {
            var testArray = new[] {"element1", "element2"};
            var testArrayFacet = new DotNetArrayFacet(facetHolder);
            INakedObject testAdaptedArray = testSystem.AdapterFor(testArray);
            Page(testArrayFacet, testAdaptedArray, "element1");
        }

        [Test]
        public void ArraySize() {
            var testArray = new[] {"element1", "element2"};
            var testArrayFacet = new DotNetArrayFacet(facetHolder);
            INakedObject testAdaptedArray = testSystem.AdapterFor(testArray);
            Size(testArrayFacet, testAdaptedArray);
        }

        [Test]
        public void CollectionContains() {
            var testCollection = new ArrayList {"element1", "element2"};
            var testCollectionFacet = new DotNetCollectionFacet(facetHolder);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Contains(testCollectionFacet, testAdaptedCollection, "element1", "element3");
        }

        [Test]
        public void CollectionFirstElement() {
            var testCollection = new ArrayList {"element1", "element2"};
            var testCollectionFacet = new DotNetCollectionFacet(facetHolder);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            FirstElement(testCollectionFacet, testAdaptedCollection, "element1");
        }

        [Test]
        public void CollectionGetEnumeratorFor() {
            var testCollection = new ArrayList {"element1", "element2"};
            var testCollectionFacet = new DotNetCollectionFacet(facetHolder);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            ValidateCollection(testCollectionFacet, testAdaptedCollection, testCollection.Cast<object>());
        }

        [Test]
        public void CollectionInit() {
            var testCollection = new ArrayList {"element1", "element2"};
            var testCollection1 = new[] {"element2", "element3"};
            var testCollectionFacet = new DotNetCollectionFacet(facetHolder);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection.Cast<object>(), testCollection1);
        }

        [Test]
        public void CollectionInitAllEmpty() {
            var testCollection = new ArrayList();
            var testCollection1 = new string[] {};
            var testCollectionFacet = new DotNetCollectionFacet(facetHolder);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection.Cast<object>(), testCollection1);
        }

        [Test]
        public void CollectionInitEmpty() {
            var testCollection = new ArrayList();
            var testCollection1 = new[] {"element2", "element3"};
            var testCollectionFacet = new DotNetCollectionFacet(facetHolder);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection.Cast<object>(), testCollection1);
        }

        [Test]
        public void CollectionInitToEmpty() {
            var testCollection = new ArrayList {"element1", "element2"};
            var testCollection1 = new string[] {};
            var testCollectionFacet = new DotNetCollectionFacet(facetHolder);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection.Cast<object>(), testCollection1);
        }

        [Test]
        public void CollectionNotIsSet() {
            ICollectionFacet testCollectionFacet = new DotNetCollectionFacet(facetHolder);
            Assert.IsFalse(testCollectionFacet.IsASet);
            testCollectionFacet = new DotNetArrayFacet(facetHolder);
            Assert.IsFalse(testCollectionFacet.IsASet);
            testCollectionFacet = new DotNetCollectionFacet(facetHolder);
            Assert.IsFalse(testCollectionFacet.IsASet);
        }

        [Test]
        public void CollectionPage() {
            var testCollection = new ArrayList {"element1", "element2"};
            var testCollectionFacet = new DotNetCollectionFacet(facetHolder);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Page(testCollectionFacet, testAdaptedCollection, "element1");
        }

        [Test]
        public void CollectionSize() {
            var testCollection = new ArrayList {"element1", "element2"};
            var testCollectionFacet = new DotNetCollectionFacet(facetHolder);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Size(testCollectionFacet, testAdaptedCollection);
        }

        [Test]
        public void GenericCollectionContains() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollectionFacet = new DotNetGenericCollectionFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Contains(testCollectionFacet, testAdaptedCollection, "element1", "element3");
        }

        [Test]
        public void GenericCollectionFirstElement() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollectionFacet = new DotNetGenericCollectionFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            FirstElement(testCollectionFacet, testAdaptedCollection, "element1");
        }

        [Test]
        public void GenericCollectionGetEnumeratorFor() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollectionFacet = new DotNetGenericCollectionFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            ValidateCollection(testCollectionFacet, testAdaptedCollection, testCollection);
        }

        [Test]
        public void GenericCollectionInit() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollection1 = new[] {"element2", "element3"};
            var testCollectionFacet = new DotNetGenericCollectionFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection, testCollection1);
        }

        [Test]
        public void GenericCollectionInitAllEmpty() {
            var testCollection = new List<string>();
            var testCollection1 = new string[] {};
            var testCollectionFacet = new DotNetGenericCollectionFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection, testCollection1);
        }

        [Test]
        public void GenericCollectionInitEmpty() {
            var testCollection = new List<string>();
            var testCollection1 = new[] {"element2", "element3"};
            var testCollectionFacet = new DotNetGenericCollectionFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection, testCollection1);
        }

        [Test]
        public void GenericCollectionInitToEmpty() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollection1 = new string[] {};
            var testCollectionFacet = new DotNetGenericCollectionFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection, testCollection1);
        }

        [Test]
        public void GenericCollectionIsSet() {
            ICollectionFacet testCollectionFacet = new DotNetGenericCollectionFacet<string>(facetHolder, typeof (string), true);
            Assert.IsTrue(testCollectionFacet.IsASet);
            testCollectionFacet = new DotNetGenericIEnumerableFacet<string>(facetHolder, typeof (string), true);
            Assert.IsTrue(testCollectionFacet.IsASet);
            testCollectionFacet = new DotNetGenericIQueryableFacet<string>(facetHolder, typeof (string), true);
            Assert.IsTrue(testCollectionFacet.IsASet);
        }

        [Test]
        public void GenericCollectionNotIsSet() {
            ICollectionFacet testCollectionFacet = new DotNetGenericCollectionFacet<string>(facetHolder, typeof (string), false);
            Assert.IsFalse(testCollectionFacet.IsASet);
            testCollectionFacet = new DotNetGenericIEnumerableFacet<string>(facetHolder, typeof (string), false);
            Assert.IsFalse(testCollectionFacet.IsASet);
            testCollectionFacet = new DotNetGenericIQueryableFacet<string>(facetHolder, typeof (string), false);
            Assert.IsFalse(testCollectionFacet.IsASet);
        }

        [Test]
        public void GenericCollectionPage() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollectionFacet = new DotNetGenericCollectionFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Page(testCollectionFacet, testAdaptedCollection, "element1");
        }

        [Test]
        public void GenericCollectionSize() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollectionFacet = new DotNetGenericCollectionFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Size(testCollectionFacet, testAdaptedCollection);
        }

        [Test]
        public void GenericEnumerableContains() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollectionFacet = new DotNetGenericIEnumerableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Contains(testCollectionFacet, testAdaptedCollection, "element1", "element3");
        }

        [Test]
        public void GenericEnumerableFirstElement() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollectionFacet = new DotNetGenericIEnumerableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            FirstElement(testCollectionFacet, testAdaptedCollection, "element1");
        }

        [Test]
        public void GenericEnumerableGetEnumeratorFor() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollectionFacet = new DotNetGenericIEnumerableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            ValidateCollection(testCollectionFacet, testAdaptedCollection, testCollection);
        }

        [Test]
        public void GenericEnumerableInit() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollection1 = new[] {"element2", "element3"};
            var testCollectionFacet = new DotNetGenericIEnumerableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection, testCollection1);
        }

        [Test]
        public void GenericEnumerableInitAllEmpty() {
            var testCollection = new List<string>();
            var testCollection1 = new string[] {};
            var testCollectionFacet = new DotNetGenericIEnumerableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection, testCollection1);
        }

        [Test]
        public void GenericEnumerableInitEmpty() {
            var testCollection = new List<string>();
            var testCollection1 = new[] {"element2", "element3"};
            var testCollectionFacet = new DotNetGenericIEnumerableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection, testCollection1);
        }

        [Test]
        public void GenericEnumerableInitToEmpty() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollection1 = new string[] {};
            var testCollectionFacet = new DotNetGenericIEnumerableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection, testCollection1);
        }

        [Test]
        public void GenericEnumerablePage() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollectionFacet = new DotNetGenericIEnumerableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Page(testCollectionFacet, testAdaptedCollection, "element1");
        }

        [Test]
        public void GenericEnumerableSize() {
            var testCollection = new List<string> {"element1", "element2"};
            var testCollectionFacet = new DotNetGenericIEnumerableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Size(testCollectionFacet, testAdaptedCollection);
        }

        [Test]
        public void GenericQueryableContains() {
            IQueryable<string> testCollection = new List<string> {"element1", "element2"}.AsQueryable();
            var testCollectionFacet = new DotNetGenericIQueryableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Contains(testCollectionFacet, testAdaptedCollection, "element1", "element3");
        }

        [Test]
        public void GenericQueryableFirstElement() {
            IQueryable<string> testCollection = new List<string> {"element1", "element2"}.AsQueryable();
            var testCollectionFacet = new DotNetGenericIQueryableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            FirstElement(testCollectionFacet, testAdaptedCollection, "element1");
        }

        [Test]
        public void GenericQueryableGetEnumeratorFor() {
            IQueryable<string> testCollection = new List<string> {"element1", "element2"}.AsQueryable();
            var testCollectionFacet = new DotNetGenericIQueryableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            ValidateCollection(testCollectionFacet, testAdaptedCollection, testCollection);
        }

        [Test]
        public void GenericQueryableInit() {
            IQueryable<string> testCollection = new List<string> {"element1", "element2"}.AsQueryable();
            IQueryable<string> testCollection1 = new[] {"element2", "element3"}.AsQueryable();
            var testCollectionFacet = new DotNetGenericIQueryableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection, testCollection1);
        }

        [Test]
        public void GenericQueryableInitAllEmpty() {
            IQueryable<string> testCollection = new List<string>().AsQueryable();
            IQueryable<string> testCollection1 = new string[] {}.AsQueryable();
            var testCollectionFacet = new DotNetGenericIQueryableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection, testCollection1);
        }

        [Test]
        public void GenericQueryableInitEmpty() {
            IQueryable<string> testCollection = new List<string>().AsQueryable();
            var testCollection1 = new[] {"element2", "element3"};
            var testCollectionFacet = new DotNetGenericIQueryableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection, testCollection1);
        }

        [Test]
        public void GenericQueryableInitToEmpty() {
            IQueryable<string> testCollection = new List<string> {"element1", "element2"}.AsQueryable();
            IQueryable<string> testCollection1 = new string[] {}.AsQueryable();
            var testCollectionFacet = new DotNetGenericIQueryableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Init(testCollectionFacet, testAdaptedCollection, testCollection, testCollection1);
        }

        [Test]
        public void GenericQueryablePage() {
            IQueryable<string> testCollection = new List<string> {"element1", "element2"}.AsQueryable();
            var testCollectionFacet = new DotNetGenericIQueryableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Page(testCollectionFacet, testAdaptedCollection, "element1");
        }

        [Test]
        public void GenericQueryableSize() {
            IQueryable<string> testCollection = new List<string> {"element1", "element2"}.AsQueryable();
            var testCollectionFacet = new DotNetGenericIQueryableFacet<string>(facetHolder, typeof (string), false);
            INakedObject testAdaptedCollection = testSystem.AdapterFor(testCollection);
            Size(testCollectionFacet, testAdaptedCollection);
        }
    }
}