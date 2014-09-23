﻿// Copyright © Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System;
using System.Collections.Generic;
using System.Linq;
using NakedObjects.Architecture;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Persist;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Resolve;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.Util;
using NakedObjects.Core.Context;
using NakedObjects.Core.Persist;
using NUnit.Framework;
using TestData;

namespace NakedObjects.Persistor.TestSuite {
    /// <summary>
    /// Prerequisite - TestData Fixture run and nakedobjects framework setup
    /// </summary>
    public  class PersistorTestSuite {
        private readonly INakedObjectsFramework framework;

        public PersistorTestSuite(INakedObjectsFramework framework) {
            this.framework = framework;
        }

        #region helpers

        private  ILifecycleManager Persistor {
            get {
                return framework.ObjectPersistor;
            }
        }

        private INakedObjectReflector Reflector {
            get {
                return framework.Reflector;
            }
        }

        private  IUpdateNotifier Notifier {
            get {
                return framework.UpdateNotifier;
            }
        }

        private  INakedObject AdapterFor(object domainObject) {
            return framework.ObjectPersistor.CreateAdapter(domainObject, null, null);
        }


        private  Person GetPerson(int id) {
            return framework.ObjectPersistor.Instances<Person>().Single(p => p.PersonId == id);
        }


        private  void AssertIsPerson(Person person, int id) {
            Assert.IsNotNull(person, "Failed to get instance");
            Assert.AreEqual(id, person.PersonId);
        }

        private  Person ChangeScalarOnPerson(int id) {
            Person person = GetPerson(id);
            string originalName = person.Name;
            Persistor.StartTransaction();
            person.Name = Guid.NewGuid().ToString();
            Persistor.EndTransaction();
            Notifier.AllChangedObjects();
            person.ResetEvents();
            Persistor.StartTransaction();
            person.Name = originalName;
            Persistor.EndTransaction();
            return person;
        }

        private  Product GetProduct(int id) {
            return Persistor.Instances<Product>().Single(p => p.Id == id);
        }

        private  Person ChangeReferenceOnPerson(int id) {
            Person person = GetPerson(id);
            Notifier.AllChangedObjects();
            person.ResetEvents();
            Persistor.StartTransaction();
            person.FavouriteProduct = GetProduct(3);
            Persistor.EndTransaction();
            return person;
        }

        private  Person AddToCollectionOnPersonOne(Person personToAdd) {
            Person person = GetPerson(1);
            Notifier.AllChangedObjects();
            person.ResetEvents();
            Persistor.StartTransaction();
            person.AddToRelatives(personToAdd);
            Persistor.EndTransaction();
            return person;
        }

        private  Person RemoveFromCollectionOnPersonOne(Person personToRemove) {
            Person person = GetPerson(1);
            Notifier.AllChangedObjects();
            person.ResetEvents();
            Persistor.StartTransaction();
            person.RemoveFromRelatives(personToRemove);
            Persistor.EndTransaction();
            return person;
        }

        private  Person ClearCollectionOnPerson(int id) {
            Person person = GetPerson(id);
            Notifier.AllChangedObjects();
            person.ResetEvents();
            Persistor.StartTransaction();
            person.ClearRelatives();
            Persistor.EndTransaction();
            return person;
        }

        private  Product GetProductFromPersonOne() {
            Persistor.StartTransaction();
            Person person1 = GetPerson(1);
            Product product = person1.FavouriteProduct;
            var name = product.Name; // to ensure product is resolved
            Persistor.EndTransaction();
            return product;
        }

        private  Person GetPersonFromPersonOneCollection() {
            Persistor.StartTransaction();
            Person relative = GetPerson(1).Relatives.First();
            Persistor.EndTransaction();
            return relative;
        }

        private  Person CreateNewTransientPerson() {
            int nextIndex = Persistor.Instances<Person>().Select(p => p.PersonId).Max() + 1;
            INakedObjectSpecification spec = Reflector.LoadSpecification(typeof (Person));
            INakedObject newPersonAdapter = Persistor.CreateInstance(spec);
            Person person = (Person)newPersonAdapter.Object;
            person.PersonId = nextIndex;
            return person;
        }

        private  Order CreateNewTransientOrder() {
            INakedObjectSpecification spec = Reflector.LoadSpecification(typeof(Order));
            INakedObject newOrderAdapter = Persistor.CreateInstance(spec);
            Order order = (Order)newOrderAdapter.Object;
            order.OrderId = 0;
            return order;
        }

        private  OrderFail CreateNewTransientOrderFail() {
            INakedObjectSpecification spec = Reflector.LoadSpecification(typeof(OrderFail));
            INakedObject newOrderAdapter = Persistor.CreateInstance(spec);
            OrderFail order = (OrderFail)newOrderAdapter.Object;
            order.OrderFailId = 0;
            return order;
        }


        private  Product CreateNewTransientProduct() {
            int nextIndex = Persistor.Instances<Product>().Select(p => p.Id).Max() + 1;
            INakedObjectSpecification spec = Reflector.LoadSpecification(typeof(Product));
            INakedObject newProductAdapter = Persistor.CreateInstance(spec);
            Product product = (Product)newProductAdapter.Object;
            product.Id = nextIndex;
            return product;
        }

        private  Pet CreateNewTransientPet() {
            int nextIndex = Persistor.Instances<Pet>().Select(p => p.PetId).Max() + 1;
            INakedObjectSpecification spec = Reflector.LoadSpecification(typeof(Pet));
            INakedObject newPetAdapter = Persistor.CreateInstance(spec);
            Pet pet = (Pet)newPetAdapter.Object;
            pet.PetId = nextIndex;
            return pet;
        }

        private  INakedObject Save(object toSave) {
            Persistor.StartTransaction();
            INakedObject adapterForToSave = AdapterFor(toSave);
            Persistor.MakePersistent(adapterForToSave);
            Persistor.EndTransaction();
            return adapterForToSave;
        }

        private  void Delete(object toDelete) {
            Persistor.StartTransaction();
            INakedObject adapterForToDelete = AdapterFor(toDelete);
            Persistor.DestroyObject(adapterForToDelete);
            Persistor.EndTransaction();
           
        }


        private  Address ChangeScalarOnAddress() {
            INakedObject adaptedAddress = GetAdaptedAddress(GetPerson(1));
            Address address = (Address)adaptedAddress.Object;
            string original1 = address.Line1;     
            Persistor.StartTransaction();
            address.Line1 = Guid.NewGuid().ToString();      
            Persistor.EndTransaction();
            Notifier.AllChangedObjects();
            address.ResetEvents();
            Persistor.StartTransaction();
            address.Line1 = original1;       
            Persistor.EndTransaction();
            return address;
        }

        private  INakedObject GetAdaptedAddress(Person person) {
            INakedObject personAdapter = AdapterFor(person);
            return personAdapter.Specification.GetProperty("Address").GetNakedObject(personAdapter, Persistor);
        }

        private  INakedObject GetAdaptedRelatives(Person person) {
            Persistor.StartTransaction();
            INakedObject personAdapter = AdapterFor(person);
            Persistor.EndTransaction();
            return personAdapter.Specification.GetProperty("Relatives").GetNakedObject(personAdapter, Persistor);
        }


        #endregion

        #region tests

        public  void GetInstanceFromInstancesOfT() {
            Person person = GetPerson(1);
            AssertIsPerson(person, 1);
        }

        public  void GetInstanceFromInstancesOfType() {
            Person person = Persistor.Instances(typeof(Person)).Cast<Person>().Where(p => p.PersonId == 1).Single();
            AssertIsPerson(person, 1);
        }

        public  void GetInstanceFromInstancesOfSpecification() {
            INakedObjectSpecification spec = Reflector.LoadSpecification(typeof(Person));
            Person person = Persistor.Instances(spec).Cast<Person>().Where(p => p.PersonId == 1).Single();
            AssertIsPerson(person, 1);
        }

        public  void GetInstanceIsAlwaysSameObject() {
            INakedObjectSpecification spec = Reflector.LoadSpecification(typeof(Person));
            Person person1 = GetPerson(1);
            Person person2 = Persistor.Instances(typeof(Person)).Cast<Person>().Where(p => p.PersonId == 1).Single();
            Person person3 = Persistor.Instances(spec).Cast<Person>().Where(p => p.PersonId == 1).Single();
            Assert.AreSame(person1, person2);
            Assert.AreSame(person2, person3);
        }

        public  void GetInstanceResolveStateIsPersistent() {
            INakedObject adapter = AdapterFor(GetPerson(1));
            Assert.IsTrue(adapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(adapter.Oid.IsTransient, "is transient");
        }

        public  void GetInstanceHasVersion() {
            INakedObject adapter = AdapterFor(GetPerson(1));
            Assert.IsNotNull(adapter.Version, "should have version");
        }

        public  void ChangeScalarOnPersistentNotifiesUi() {
            Person person = ChangeScalarOnPerson(1);
            Assert.Contains(person, Notifier.AllChangedObjects().ToEnumerable().Select(no => no.Object).ToList(), "no person in notifier");
        }

        public  void ChangeScalarOnPersistentCallsUpdatingUpdated() {
            Person person = ChangeScalarOnPerson(1);
            Assert.AreEqual(1, person.GetEvents()["Updating"], "updating");
            Assert.AreEqual(1, person.GetEvents()["Updated"], "updated");
        }

        public  void ChangeReferenceOnPersistentNotifiesUi() {
            Person person = ChangeReferenceOnPerson(6);
            Assert.Contains(person, Notifier.AllChangedObjects().ToEnumerable().Select(no => no.Object).ToList(), "no person in notifier");
        }

        public  void ChangeReferenceOnPersistentCallsUpdatingUpdated() {
            Person person = ChangeReferenceOnPerson(7);
            Assert.AreEqual(1, person.GetEvents()["Updating"], "updating");
            Assert.AreEqual(1, person.GetEvents()["Updated"], "updated");
        }

        public  void AddToCollectionOnPersistent() {
            Person person1 = GetPerson(1);
            Person person4 = GetPerson(4);
            int countbefore = person1.Relatives.Count();
            AddToCollectionOnPersonOne(person4);
            Assert.AreEqual(countbefore + 1, person1.Relatives.Count());
        }

        public  void AddToCollectionOnPersistentNotifiesUi() {
            Person person5 = GetPerson(5);
            Person person1 = AddToCollectionOnPersonOne(person5);
            Assert.Contains(person1, Notifier.AllChangedObjects().ToEnumerable().Select(no => no.Object).ToList(), "no person in notifier");
        }

        public  void AddToCollectionOnPersistentCallsUpdatingUpdated() {
            Person person6 = GetPerson(6);
            Person person1 = AddToCollectionOnPersonOne(person6);
            Assert.AreEqual(1, person1.GetEvents()["Updating"], "updating");
            Assert.AreEqual(1, person1.GetEvents()["Updated"], "updated");
        }

        public  void RemoveFromCollectionOnPersistent() {
            Person person1 = GetPerson(1);
            Person person2 = GetPerson(2);
            int countbefore = person1.Relatives.Count();
            RemoveFromCollectionOnPersonOne(person2);
            Assert.AreEqual(countbefore -1, person1.Relatives.Count());
        }

        public  void RemoveFromCollectionOnPersistentNotifiesUi() {
            Person person7 = GetPerson(7);
            Person person1 = RemoveFromCollectionOnPersonOne(person7);
            Assert.Contains(person1, Notifier.AllChangedObjects().ToEnumerable().Select(no => no.Object).ToList(), "no person in notifier");
        }

        public  void RemoveFromCollectionOnPersistentCallsUpdatingUpdated() {
            Person person8 = GetPerson(8);
            Person person1 = RemoveFromCollectionOnPersonOne(person8);
            Assert.AreEqual(1, person1.GetEvents()["Updating"], "updating");
            Assert.AreEqual(1, person1.GetEvents()["Updated"], "updated");
        }

        public  void ClearCollectionOnPersistent() {
            Person person = ClearCollectionOnPerson(6);
            Assert.AreEqual(0, person.Relatives.Count);
        }

        public  void ClearCollectionOnPersistentNotifiesUi() {
            Person person = ClearCollectionOnPerson(7);
            Assert.Contains(person, Notifier.AllChangedObjects().ToEnumerable().Select(no => no.Object).ToList(), "no person in notifier");
        }

        public  void ClearCollectionOnPersistentCallsUpdatingUpdated() {         
            Person person = ClearCollectionOnPerson(8);
            Assert.AreEqual(1, person.GetEvents()["Updating"], "updating");
            Assert.AreEqual(1, person.GetEvents()["Updated"], "updated");
        }

        public  void LoadObjectReturnSameObject() {
            Person person1 = GetPerson(1);
            INakedObject adapter1 = AdapterFor(person1);
            INakedObject adapter2 = Persistor.LoadObject(adapter1.Oid, adapter1.Specification);
            Assert.AreSame(person1, adapter2.Object);
        }

        public  void PersistentObjectHasContainerInjected() {
            Person person1 = GetPerson(1);
            Assert.IsTrue(person1.HasContainer, "no container injected");
        }

        public  void PersistentObjectHasServiceInjected() {
            Person person1 = GetPerson(1);
            Assert.IsTrue(person1.HasMenuService, "no service injected");
        }

        public  void PersistentObjectHasLoadingLoadedCalled() {
            Persistor.StartTransaction();
            Person person1 = GetPerson(1);
            Persistor.EndTransaction();
            Assert.AreEqual(1, person1.GetEvents()["Loading"], "loading");
            Assert.AreEqual(1, person1.GetEvents()["Loaded"], "loaded");
        }

        public  void CanAccessReferenceProperty() {
            Person person1 = GetPerson(1);
            Product product = person1.FavouriteProduct;
            Assert.IsNotNull(product, "Failed to access instance");
            Assert.AreEqual("ProductOne", product.Name);
        }

        public  void CanAccessCollectionProperty() {
            ICollection<Person> relatives = GetPerson(1).Relatives;
            Assert.IsNotNull(relatives, "Failed to access collection");
            Assert.Greater(relatives.Count, 0, "no items in collection");
        }

        public  void CollectionPropertyCollectionResolveStateIsPersistent() {
            INakedObject relativesAdapter = GetAdaptedRelatives(GetPerson(1)); 
            Assert.IsTrue(relativesAdapter.ResolveState.IsPersistent(), "should be persistent");
          //  Assert.IsFalse(relativesAdapter.ResolveState.IsResolved(), "should not be resolved");
            Assert.IsNotNull(relativesAdapter.Oid, "is  null");
            Assert.IsInstanceOf(typeof(AggregateOid),  relativesAdapter.Oid, "is not aggregate");
        }

        public  void EmptyCollectionPropertyCollectionResolveStateIsPersistent() {
            INakedObject relativesAdapter = GetAdaptedRelatives(GetPerson(2));
            Assert.IsTrue(relativesAdapter.ResolveState.IsPersistent(), "should be persistent");
          //  Assert.IsFalse(relativesAdapter.ResolveState.IsResolved(), "should not be resolved");
            Assert.IsNotNull(relativesAdapter.Oid, "is  null");
            Assert.IsInstanceOf(typeof(AggregateOid), relativesAdapter.Oid, "is not aggregate");
        }     

        public  void ReferencePropertyHasLoadingLoadedCalled() {
            Product product = GetProductFromPersonOne();
            Assert.AreEqual(1, product.GetEvents()["Loading"], "loading");
            Assert.AreEqual(1, product.GetEvents()["Loaded"], "loaded");
        }

        public  void ReferencePropertyObjectHasContainerInjected() {
            Product product = GetProductFromPersonOne();
            Assert.IsTrue(product.HasContainer, "no container injected");
        }

        public  void ReferencePropertyObjectHasServiceInjected() {
            Product product = GetProductFromPersonOne();
            Assert.IsTrue(product.HasMenuService, "no service injected");
        }

        public  void ReferencePropertyObjectResolveStateIsPersistent() {
            INakedObject adapter = AdapterFor(GetProductFromPersonOne());
            Assert.IsTrue(adapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(adapter.Oid.IsTransient, "is transient");
        }

        public  void ReferencePropertyObjectHasVersion() {
            INakedObject adapter = AdapterFor(GetProductFromPersonOne());
            Assert.IsNotNull(adapter.Version, "should have version");
        }

        public  void CollectionPropertyHasLoadingLoadedCalled() {
            Person person = GetPersonFromPersonOneCollection();
            Assert.AreEqual(1, person.GetEvents()["Loading"], "loading");
            Assert.AreEqual(1, person.GetEvents()["Loaded"], "loaded");
        }

        public  void CollectionPropertyObjectHasContainerInjected() {
            Person person = GetPersonFromPersonOneCollection();
            Assert.IsTrue(person.HasContainer, "no container injected");
        }

        public  void CollectionPropertyObjectHasMenuServiceInjected() {
            Person person = GetPersonFromPersonOneCollection();
            Assert.IsTrue(person.HasMenuService, "no menu service injected");
        }

        public  void CollectionPropertyObjectHasSystemServiceInjected() {
            Person person = GetPersonFromPersonOneCollection();
            Assert.IsTrue(person.HasSystemService, "no system service injected");
        }

        public  void CollectionPropertyObjectHasContributedServiceInjected() {
            Person person = GetPersonFromPersonOneCollection();
            Assert.IsTrue(person.HasContributedActions, "no contributed service injected");
        }


        public  void CollectionPropertyObjectResolveStateIsPersistent() {
            INakedObject adapter = AdapterFor(GetPersonFromPersonOneCollection());
            Assert.IsTrue(adapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(adapter.Oid.IsTransient, "is transient");
        }

        public  void CollectionPropertyObjectHasVersion() {
            INakedObject adapter = AdapterFor(GetPersonFromPersonOneCollection());
            Assert.IsNotNull(adapter.Version, "should have version");
        }
    
        public  void NewObjectIsCreated () {
            Person person = CreateNewTransientPerson();
            Assert.IsNotNull(person);
        }

        public  void NewObjectHasCreatedCalled() {
            Person person = CreateNewTransientPerson();
            Assert.AreEqual(1, person.GetEvents()["Created"], "created");
        }

        public  void NewObjectIsTransient() {
            INakedObject adapter = AdapterFor(CreateNewTransientPerson());
            Assert.IsTrue(adapter.ResolveState.IsTransient(), "should be transient");
            Assert.IsTrue(adapter.Oid.IsTransient, "is persistent");
        }

        public  void NewObjectHasVersion() {
            INakedObject adapter = AdapterFor(CreateNewTransientPerson());
            Assert.IsNotNull(adapter.Version, "should have version");
        }

        public  void NewObjectHasContainerInjected() {
            Person person = CreateNewTransientPerson();
            Assert.IsTrue(person.HasContainer, "no container injected");
        }

        public  void NewObjectHasServiceInjected() {
            Person person = CreateNewTransientPerson();
            Assert.IsTrue(person.HasMenuService, "no service injected");
        }

        public  void SaveNewObjectWithScalars() {
            Person person = CreateNewTransientPerson();
            person.Name = Guid.NewGuid().ToString();
            INakedObject adapter = Save(person);
            Assert.IsTrue(adapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(adapter.Oid.IsTransient, "is transient");
        }

        public  void SaveNewObjectWithValidate() {
            Person person = CreateNewTransientPerson();
            person.ChangeName("fail");

            try {
                Save(person);
                Assert.Fail();
            }
            catch (Exception /*expected*/) {}
        }

        public  void ChangeObjectWithValidate() {
            Person person = CreateNewTransientPerson();
            person.Name = Guid.NewGuid().ToString();
            person = Save(person).GetDomainObject<Person>();

            try {
                Persistor.StartTransaction();
                person.Name = "fail";
                Persistor.EndTransaction();
                Assert.Fail();
            }
            catch (PersistFailedException /*expected*/) { }
        }


        public  void SaveNewObjectWithTransientReference() {    
            Person person = CreateNewTransientPerson();
            Product product = CreateNewTransientProduct();
            person.Name = Guid.NewGuid().ToString();
            product.Name = Guid.NewGuid().ToString();
            person.FavouriteProduct = product;
            INakedObject personAdapter = Save(person);
            // use new person to avoid EF quirk 
            person = personAdapter.GetDomainObject<Person>();

            Assert.IsTrue(personAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(personAdapter.Oid.IsTransient, "is transient");
            INakedObject productAdapter = AdapterFor(person.FavouriteProduct);
            Assert.IsTrue(productAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(productAdapter.Oid.IsTransient, "is transient");
        }

        public  void SaveNewObjectWithTransientReferenceInvalid() {
            Person person = CreateNewTransientPerson();
            Product product = CreateNewTransientProduct();
            person.Name = Guid.NewGuid().ToString();
         
            try {
         
                // should fail as product name is not set 
                person.FavouriteProduct = product;

                // for EF 
                INakedObject adapter = AdapterFor(person);

                if (adapter.ValidToPersist() != null) {
                    throw new PersistFailedException("");
                }

                Assert.Fail();
            }
            catch (PersistFailedException /*expected*/) { }
        }

        public  void SaveNewObjectWithTransientReferenceObjectInvalid() {
            Person person = CreateNewTransientPerson();
            Pet pet = CreateNewTransientPet();
       
            pet.Name = Guid.NewGuid().ToString();
            person.Name = Guid.NewGuid().ToString();

            try {
            
                // should fail as owner is not set 
                person.Pet = pet;

                // for EF 
                INakedObject adapter = AdapterFor(person);

                if (adapter.ValidToPersist() != null) {
                    throw new PersistFailedException("");
                }

                Assert.Fail();
            }
            catch (PersistFailedException /*expected*/) { }
        }

        public  void SaveNewObjectWithTransientReferenceValidateAssocInvalid() {
            Person person = CreateNewTransientPerson();
            Pet pet = CreateNewTransientPet();

            pet.Name = Guid.NewGuid().ToString();
            person.Name = "Cruella"; 

            try {
           
                pet.Owner = person;
                person.Pet = pet;

                // for EF 
                INakedObject adapter = AdapterFor(person);

                if (adapter.ValidToPersist() != null ) {
                    throw new PersistFailedException("");
                }

                Assert.Fail();
            }
            catch (PersistFailedException /*expected*/) { }
        }

        public  void SaveNewObjectWithPersistentReference() {
            Person person = CreateNewTransientPerson();
            Product product = GetProduct(2);
            person.Name = Guid.NewGuid().ToString();
            person.FavouriteProduct = product;
            INakedObject personAdapter = Save(person);
            Assert.IsTrue(personAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(personAdapter.Oid.IsTransient, "is transient");
            INakedObject productAdapter = AdapterFor(product);
            Assert.IsTrue(productAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(productAdapter.Oid.IsTransient, "is transient");
        }

        public  void SaveNewObjectWithPersistentReferenceInSeperateTransaction() {
            Persistor.StartTransaction();
            Person person = CreateNewTransientPerson();
            Product product = GetProduct(2);
            person.Name = Guid.NewGuid().ToString();
            person.FavouriteProduct = product;
            Persistor.EndTransaction();
            INakedObject personAdapter = Save(person);
            Assert.IsTrue(personAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(personAdapter.Oid.IsTransient, "is transient");
            INakedObject productAdapter = AdapterFor(product);
            Assert.IsTrue(productAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(productAdapter.Oid.IsTransient, "is transient");
        }

        public  void SaveNewObjectWithTransientCollectionItem() {
            Person person1 = CreateNewTransientPerson();
            Person person2 = CreateNewTransientPerson();
            person1.Name = Guid.NewGuid().ToString();
            person2.Name = Guid.NewGuid().ToString();
            person1.Relatives.Add(person2);
            INakedObject person1Adapter = Save(person1);
            // use new person to avoid EF quirk 
            person1 = person1Adapter.GetDomainObject<Person>();
            person2 = person1.Relatives.Single();
            Assert.IsTrue(person1Adapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(person1Adapter.Oid.IsTransient, "is transient");
            INakedObject person2Adapter = AdapterFor(person2);
            Assert.IsTrue(person2Adapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(person2Adapter.Oid.IsTransient, "is transient");

            INakedObject collectionAdapter = person1Adapter.Specification.GetProperty("Relatives").GetNakedObject(person1Adapter, Persistor);
            Assert.IsTrue(collectionAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(collectionAdapter.ResolveState.IsGhost(), "should not be ghost");
        }

        public  void SaveNewObjectWithPersistentItemCollectionItem() {
            Person person1 = CreateNewTransientPerson();
            Person person2 = GetPerson(2);
            person1.Name = Guid.NewGuid().ToString();
            person1.Relatives.Add(person2);
            INakedObject personAdapter = Save(person1);
            Assert.IsTrue(personAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(personAdapter.Oid.IsTransient, "is transient");
            INakedObject productAdapter = AdapterFor(person2);
            Assert.IsTrue(productAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(productAdapter.Oid.IsTransient, "is transient");

            INakedObject collectionAdapter = personAdapter.Specification.GetProperty("Relatives").GetNakedObject(personAdapter, Persistor);
            Assert.IsTrue(collectionAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(collectionAdapter.ResolveState.IsGhost(), "should not be ghost");
        }

        public  void SaveNewObjectWithPersistentItemCollectionItemInSeperateTransaction() {
            Persistor.StartTransaction();
            Person person1 = CreateNewTransientPerson();
            Person person2 = GetPerson(2);
            person1.Name = Guid.NewGuid().ToString();
            person1.Relatives.Add(person2);
            Persistor.EndTransaction();
            INakedObject personAdapter = Save(person1);
            Assert.IsTrue(personAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(personAdapter.Oid.IsTransient, "is transient");
            INakedObject productAdapter = AdapterFor(person2);
            Assert.IsTrue(productAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(productAdapter.Oid.IsTransient, "is transient");

            INakedObject collectionAdapter = personAdapter.Specification.GetProperty("Relatives").GetNakedObject(personAdapter, Persistor);
            Assert.IsTrue(collectionAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(collectionAdapter.ResolveState.IsGhost(), "should not be ghost");
        }

        public  void SaveNewObjectCallsPersistingPersisted() {
            Person person = CreateNewTransientPerson();
            person.Name = Guid.NewGuid().ToString();
            person.UpdateInPersisting();
            INakedObject adapter = Save(person);
            Assert.AreEqual(1, person.GetEvents()["Persisting"], "persisting");
            Assert.AreEqual(0, person.GetEvents()["Updating"], "persisting");

            // handle quirk in EF which swaps out object on save 
            // fix this when EF updated
            Person personAfter = (Person) adapter.Object;
            Assert.AreEqual(1, personAfter.GetEvents()["Persisted"], "persisted");
            Assert.AreEqual(0, personAfter.GetEvents()["Updated"], "persisted");
        }

        public  void SaveNewObjectCallsPersistingPersistedRecursively() {

            Assert.AreEqual(0, Persistor.Instances(typeof(Order)).Count());

            Order order = CreateNewTransientOrder();
            order.Name = Guid.NewGuid().ToString();
            INakedObject adapter = Save(order);
            Assert.AreEqual(1, order.GetEvents()["Persisting"], "persisting");
            Assert.AreEqual(5, Persistor.Instances(typeof(Order)).Count());

            Assert.IsTrue(Persistor.Instances(typeof(Order)).Cast<Order>().ToList().All(i => i.PersistingCalled));

            // handle quirk in EF which swaps out object on save 
            // fix this when EF updated
            Order orderAfter = (Order)adapter.Object;
            Assert.AreEqual(1, orderAfter.GetEvents()["Persisted"], "persisted");
            Assert.AreEqual(1, orderAfter.GetEvents()["Updating"], "updating");
            Assert.AreEqual(1, orderAfter.GetEvents()["Updated"], "updated");
        }

        public  void SaveNewObjectCallsPersistingPersistedRecursivelyExceedsMax() {

          
            OrderFail order = CreateNewTransientOrderFail();
            order.Name = Guid.NewGuid().ToString();

            try {        
                Save(order);
                Assert.Fail("Expect exception");
            }
            catch (NakedObjectDomainException e) {
                // expected
                Assert.AreEqual("Max number of commit cycles exceeded. Either increase MaxCommitCycles on installer or identify Updated/Persisted loop. Possible types : TestData.OrderFail", e.Message);
            }
           
        }

        public  void SaveNewObjectCallsPersistingPersistedRecursivelyFails() {

            Assert.AreEqual(0, Persistor.Instances(typeof(OrderFail)).Count());

            OrderFail order = CreateNewTransientOrderFail();
            order.Name = Guid.NewGuid().ToString();

            try {
                Save(order);
                Assert.Fail("Expect exception");
            }
            catch (Exception ) {
                // expected
            }

            Assert.AreEqual(0, Persistor.Instances(typeof(OrderFail)).Count());
       
        }



        public  void SaveNewObjectTransientReferenceCallsPersistingPersisted() {
            Person person = CreateNewTransientPerson();
            Product product = CreateNewTransientProduct();
            person.Name = Guid.NewGuid().ToString();
            product.Name = Guid.NewGuid().ToString();
            person.FavouriteProduct = product;
            INakedObject adapter = Save(person);
            Assert.AreEqual(1, product.GetEvents()["Persisting"], "persisting");
            
            // handle quirk in EF which swaps out object on save 
            // fix this when EF updated
            Person personAfter = (Person)adapter.Object;
            Product productAfter = personAfter.FavouriteProduct;
            Assert.AreEqual(1, productAfter.GetEvents()["Persisted"], "persisted");
        }

        public  void SaveNewObjectTransientCollectionItemCallsPersistingPersisted() {
            Person person1 = CreateNewTransientPerson();
            Person person2 = CreateNewTransientPerson();
            person1.Name = Guid.NewGuid().ToString();
            person2.Name = Guid.NewGuid().ToString();
            person1.Relatives.Add(person2);
            INakedObject adapter = Save(person1);
            Assert.AreEqual(1, person2.GetEvents()["Persisting"], "persisting");

            // handle quirk in EF which swaps out object on save 
            // fix this when EF updated
            Person personAfter = (Person)adapter.Object;
            Person personAfter1 = personAfter.Relatives.Single();
            Assert.AreEqual(1, personAfter1.GetEvents()["Persisted"], "persisted");
        }

        public  void GetInlineInstance() {
            INakedObject addressAdapter = GetAdaptedAddress(GetPerson(1));
            Assert.IsTrue(addressAdapter.ResolveState.IsPersistent(), "should be persistent");
            Assert.IsFalse(addressAdapter.Oid.IsTransient, "is transient");
        }

        public  void InlineObjectHasContainerInjected() {
            Address address = GetPerson(1).Address;
            Assert.IsTrue(address.HasContainer, "no container injected");
        }

        public  void InlineObjectHasServiceInjected() {
            Address address = GetPerson(1).Address;
            Assert.IsTrue(address.HasMenuService, "no service injected");
        }

        public  void InlineObjectHasParentInjected() {
            Address address = GetPerson(1).Address;
            Assert.IsTrue(address.HasParent, "no parent injected");
            Assert.IsTrue(address.ParentIsType(typeof(Person)), "parent wrong type");
        }

        public  void InlineObjectHasVersion() {
            INakedObject addressAdapter = GetAdaptedAddress(GetPerson(1));
            Assert.IsNotNull(addressAdapter.Version, "should have version");
        }

        public  void InlineObjectHasLoadingLoadedCalled() {
            Persistor.StartTransaction();
            INakedObject addressAdapter = GetAdaptedAddress(GetPerson(1));
            Address address = addressAdapter.GetDomainObject<Address>();
            Persistor.EndTransaction();
            Assert.AreEqual(1, address.GetEvents()["Loading"], "loading");
            Assert.AreEqual(1, address.GetEvents()["Loaded"], "loaded");
        }

        public  void CreateTransientInlineInstance() {
            INakedObject addressAdapter = GetAdaptedAddress(CreateNewTransientPerson());
            Assert.IsTrue(addressAdapter.ResolveState.IsResolved(), "should be resolved");
            Assert.IsTrue(addressAdapter.Oid.IsTransient, "is persistent");
        }

        public  void TransientInlineObjectHasContainerInjected() {
            Address address = CreateNewTransientPerson().Address;
            Assert.IsTrue(address.HasContainer, "no container injected");
        }

        public  void TransientInlineObjectHasServiceInjected() {
            Address address = CreateNewTransientPerson().Address;
            Assert.IsTrue(address.HasMenuService, "no service injected");
        }

        public  void TransientInlineObjectHasParentInjected() {
            Address address = CreateNewTransientPerson().Address;
            Assert.IsTrue(address.HasParent, "no parent injected");
            Assert.IsTrue(address.ParentIsType(typeof(Person)), "parent wrong type");
        }

        public  void TrainsientInlineObjectHasVersion() {
            INakedObject addressAdapter = GetAdaptedAddress(CreateNewTransientPerson());
            Assert.IsNotNull(addressAdapter.Version, "should have version");
        }
     
        public  void InlineObjectCallsCreated() {
            Person person = CreateNewTransientPerson();
            Assert.AreEqual(1, person.Address.GetEvents()["Created"], "created");
        }
   
        public  void SaveInlineObjectCallsPersistingPersisted() {
            Person person = CreateNewTransientPerson();
            person.Address.Line1 = Guid.NewGuid().ToString();
            person.Address.Line2 = Guid.NewGuid().ToString();
            INakedObject adapter = Save(person);
            Assert.AreEqual(1, person.GetEvents()["Persisting"], "persisting");

            // handle quirk in EF which swaps out object on save 
            // fix this when EF updated
            Person personAfter = (Person)adapter.Object;
            Assert.AreEqual(1, personAfter.GetEvents()["Persisted"], "persisted");
        }

        public  void ChangeScalarOnInlineObjectCallsUpdatingUpdated() {
            Address address = ChangeScalarOnAddress();
            Assert.AreEqual(1, address.GetEvents()["Updating"], "updating");
            Assert.AreEqual(1, address.GetEvents()["Updated"], "updated");
        }

        public  void UpdateInlineObjectUpdatesUi() {
            Address address = ChangeScalarOnAddress();
            Assert.Contains(address, Notifier.AllChangedObjects().ToEnumerable().Select(no => no.Object).ToList(), "no address in notifier");
        }

        public  void RefreshResetsObject() {
            Person person1 = GetPerson(1);
            string name = person1.Name;
            person1.Name = Guid.NewGuid().ToString();
            Persistor.Refresh(AdapterFor(person1));
            Assert.AreEqual(name, person1.Name);
            Assert.AreEqual(1, person1.GetEvents()["Updating"], "updating");
            Assert.AreEqual(1, person1.GetEvents()["Updated"], "updated");
        }

        public  void GetKeysReturnsKeys() {
            Person person1 = GetPerson(1);
            var key = Persistor.GetKeys(person1.GetType());

            Assert.AreEqual(1, key.Count());
            Assert.AreEqual(person1.GetType().GetProperty("PersonId").Name,  key[0].Name);       
        }

        public  void FindByKey() {
            Person person1 = GetPerson(1);
            var person = Persistor.FindByKeys(typeof(Person), new object[] {1}).Object;

            Assert.AreEqual(person1, person);      
        }

        public  void CreateAndDeleteNewObjectWithScalars() {
            Person person = CreateNewTransientPerson();
            string name = Guid.NewGuid().ToString();
            person.Name = name;

            var adapter = Save(person);

            Person person1 = Persistor.Instances<Person>().Where(p => p.Name == name).SingleOrDefault();
            Assert.IsNotNull(person1);

            Delete(adapter.Object);

            Person person2 = Persistor.Instances<Person>().Where(p => p.Name == name).SingleOrDefault();

            Assert.IsNull(person2);
        }

        public  void DeleteObjectCallsDeletingDeleted() {
            Person person = CreateNewTransientPerson();
            string name = Guid.NewGuid().ToString();
            person.Name = name;

            var adapter = Save(person);
            person = adapter.GetDomainObject<Person>();   

            Delete(person);

            Assert.AreEqual(1, person.GetEvents()["Deleting"], "deleting");
            Assert.AreEqual(1, person.GetEvents()["Deleted"], "deleted");
        }

        public  void CountCollectionOnPersistent() {
            Person person1 = GetPerson(1);
            int count1 = person1.Relatives.Count();
            var adapter = AdapterFor(person1);
            var count2 = Persistor.CountField(adapter, "Relatives");
            Assert.AreEqual(count1, count2);
        }

        public  void CountUnResolvedCollectionOnPersistent() {
            Person person1 = GetPerson(1);
            var adapter = AdapterFor(person1);
            var count1 = Persistor.CountField(adapter, "Relatives");
            int count2 = person1.Relatives.Count();
            Assert.AreEqual(count1, count2);
        }


        public  void CountEmptyCollectionOnTransient() {
            Person person1 = CreateNewTransientPerson();
            int count1 = person1.Relatives.Count();
            var adapter = AdapterFor(person1);
            var count2 = Persistor.CountField(adapter, "Relatives");
            Assert.AreEqual(count1, count2);
        }

        public  void CountCollectionOnTransient() {
            Person person1 = CreateNewTransientPerson();
            Person person4 = GetPerson(4);
            AddToCollectionOnPersonOne(person4);
            int count1 = person1.Relatives.Count();
            var adapter = AdapterFor(person1);
            var count2 = Persistor.CountField(adapter, "Relatives");
            Assert.AreEqual(count1, count2);
        }

      

        #endregion
    }
}