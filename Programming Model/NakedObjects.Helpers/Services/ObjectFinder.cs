// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NakedObjects.Resources;
using NakedObjects.Util;

namespace NakedObjects.Services {
    /// <summary>
    ///     An implementation of IObjectFinder. Works with multiple keys, of type Integer, String, Short, or Char
    /// </summary>
    public class ObjectFinder : IObjectFinder {
        #region Injected Services

        public IDomainObjectContainer Container { set; protected get; }

        #endregion

        #region Implementation of IObjectFinder

        public virtual T FindObject<T>(string compoundKey) {
            Type type = GetAssociatedObjectType(compoundKey);

            if (typeof (IHasGuid).IsAssignableFrom(type)) {
                MethodInfo m = GetType().GetMethod("FindObjectByGuid", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo gm = m.MakeGenericMethod(new[] {type});
                return (T) gm.Invoke(this, new[] {compoundKey});
            }
            Dictionary<string, object> keyDict = CreateKeyDictionary(type, compoundKey);
            if (keyDict.Any()) {
                return FindByKeys<T>(type, keyDict);
            }
            throw new DomainException(string.Format(ProgrammingModel.CompoundKeyHasNoKeyValues, compoundKey));
        }


        public virtual string GetCompoundKey<T>(T obj) {
            var compoundKey = new StringBuilder();
            //In all cases, the key starts with the fully-qualified type name
            compoundKey.Append((obj == null) ? null : obj.GetType().GetProxiedType());

            if (typeof (IHasGuid).IsAssignableFrom(typeof (T))) {
                compoundKey.Append("|");
                var objWithGuid = (IHasGuid) obj;
                compoundKey.Append(objWithGuid.Guid.ToString()); //but must generate this dynamically
            }
            else {
                PropertyInfo[] keyProperties = Container.GetKeys(obj.GetType());
                if (!keyProperties.Any()) {
                    throw new DomainException(string.Format(ProgrammingModel.NoKeysDefined, obj));
                }
                foreach (PropertyInfo key in keyProperties) {
                    compoundKey.Append("|");
                    compoundKey.Append((obj == null) ? null : key.GetValue(obj, null).ToString());
                }
            }
            return compoundKey.ToString();
        }

        public object FindBySingleIntegerKey(Type type, int key) {
            return FindByKey<object>(type, key);
        }

        public T FindBySingleIntegerKey<T>(int key) where T : class {
            return (T) FindByKeyGeneric<T>(key);
        }

        private TActual FindObjectByGuid<TActual>(string compoundKey) where TActual : class, IHasGuid {
            var guidFromKey = new Guid(compoundKey.Split('|').ElementAt(1));
            IQueryable<TActual> q = from t in Container.Instances<TActual>()
                                    where t.Guid == guidFromKey
                                    select t;
            return q.FirstOrDefault();
        }

        #endregion

        protected object FindByKeyGeneric<TActual>(object id) where TActual : class {
            string keyName = Container.GetSingleKey(typeof (TActual)).Name;
            return Container.Instances<TActual>().FindByKey(keyName, id);
        }

        private T FindByKey<T>(Type type, object id) {
            if (type != null && id != null) {
                MethodInfo m = GetType().GetMethod("FindByKeyGeneric", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo gm = m.MakeGenericMethod(new[] {type});
                return (T) gm.Invoke(this, new[] {id});
            }
            return default(T);
        }

        private object GetSingleKey(object obj) {
            object key = Container.GetSingleKey(obj.GetType()).GetValue(obj, null);
            return key;
        }

        protected object FindByKeysGeneric<TActual>(Dictionary<string, object> keyDict) where TActual : class {
            return Container.Instances<TActual>().FindByKeys(keyDict);
        }

        private T FindByKeys<T>(Type type, Dictionary<string, object> keyDict) {
            if (type != null && keyDict != null) {
                MethodInfo m = GetType().GetMethod("FindByKeysGeneric", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo gm = m.MakeGenericMethod(new[] {type});
                return (T) gm.Invoke(this, new[] {keyDict});
            }
            return default(T);
        }

        private Type GetAssociatedObjectType(string compoundKey) {
            string typeAsString = compoundKey.Split('|').ElementAt(0);
            if (string.IsNullOrEmpty(typeAsString)) {
                throw new DomainException(string.Format(ProgrammingModel.CompoundKeyDoesNotContainType, compoundKey));
            }
            Type type = TypeUtils.GetType(typeAsString);
            if (type == null) {
                throw new DomainException(string.Format(ProgrammingModel.TypeCannotBeFound, typeAsString));
            }
            return type;
        }

        // Creates a dictionary of the keys of the form <keyPropertyName, keyPropertyValue) where the type of
        // the keyPropertyValue is the correct type based on key properties info extracted from the input type.
        private Dictionary<string, object> CreateKeyDictionary(Type type, string compoundKey) {
            PropertyInfo[] keyProperties = Container.GetKeys(type);
            string[] valuesAsStrings = ExtractKeyValuesAsStrings(compoundKey);

            //test that the number of key values is correct for the type
            if (valuesAsStrings.Count() != keyProperties.Count()) {
                throw new DomainException(string.Format(ProgrammingModel.NumberOfkeysMismatch, type));
            }

            //Create the dictionary
            var keyDict = new Dictionary<string, object>();
            for (int i = 0; i < keyProperties.Count(); i++) {
                string stringValue = valuesAsStrings[i];
                object value = null;
                Type propType = keyProperties[i].PropertyType;
                try {
                    if (propType == typeof (string)) {
                        value = stringValue;
                    }
                    else if (propType == typeof (int)) {
                        value = int.Parse(stringValue);
                    }
                    else if (propType == typeof (short)) {
                        value = short.Parse(stringValue);
                    }
                    else if (propType == typeof (char)) {
                        value = char.Parse(stringValue);
                    }
                    else {
                        throw new DomainException(string.Format(ProgrammingModel.InvalidKeyType, propType));
                    }
                }
                catch (FormatException) {
                    throw new DomainException(string.Format(ProgrammingModel.KeyTypeMismatch, stringValue, propType));
                }
                keyDict.Add(keyProperties[i].Name, value);
            }

            return keyDict;
        }


        private string[] ExtractKeyValuesAsStrings(string compoundKey) {
            string[] keyStrings = compoundKey.Split('|');

            var keyValues = new List<string>();
            //Skip 1 as the first item is the type rather than a key
            foreach (string keyString in keyStrings.Skip(1)) {
                keyValues.Add(keyString);
            }
            return keyValues.ToArray();
        }
    }
}