// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Architecture.Util {
    public static class CollectionUtils {
        #region public

        public const int IncompleteCollection = -1;

        public static bool IsDictionary(Type type) {
            return IsGenericEnumerable(type) && IsGenericOfKeyValuePair(type);
        }

        /// <summary>IEnumerable of T where T is not a value type</summary>
        public static bool IsGenericEnumerableOfRefType(Type type) {
            return IsGenericEnumerable(type) && IsGenericOfRefType(type);
        }

        public static bool IsGenericEnumerable(Type type) {
            return IsGenericType(type, typeof (IEnumerable<>)) && type.GetGenericArguments().Count() == 1;
        }

        public static bool IsGenericCollection(Type type) {
            return IsGenericType(type, typeof (ICollection<>)) && type.GetGenericArguments().Count() == 1;
        }

        /// <summary>IEnumerable of T where T is not a value type</summary>
        public static bool IsGenericQueryable(Type type) {
            return IsGenericType(type, typeof (IQueryable<>)) && type.GetGenericArguments().Count() == 1;
        }

        /// <summary>IQueryable or IQueryable of T</summary>
        public static bool IsQueryable(Type type) {
            return typeof (IQueryable).IsAssignableFrom(type);
        }

        /// <summary> ICollection but not Array or IEnumerable of T where T is not a value type </summary>
        public static bool IsCollectionButNotArray(Type type) {
            return IsCollection(type) && !type.IsArray;
        }

        /// <summary> ICollection or IEnumerable of T or Array of T where T is not a value type </summary>
        public static bool IsCollection(Type type) {
            return IsNonGenericCollection(type) || IsGenericCollection(type);
        }

        public static Type ElementType(Type type) {
            return IsGenericEnumerable(type) ? GetGenericEnumerableType(type).GetGenericArguments().Single() : typeof (object);
        }

        public static bool IsBlobOrClob(Type type) {
            return type.IsArray && (type.GetElementType() == typeof (byte) || type.GetElementType() == typeof (sbyte) || type.GetElementType() == typeof (char));
        }

        public static bool IsGenericOfEnum(Type type) {
            return type.GetGenericArguments().Count() == 1 && type.GetGenericArguments().All(t => t.IsEnum);
        }

        public static List<T> InList<T>(this T item) {
            if (item != null) {
                return new List<T> {item};
            }
            return new List<T>();
        }

        public static void ForEach<T>(this T[] toIterate, Action<T> action) {
            Array.ForEach(toIterate, action);
        }

        public static void ForEach<T>(this IEnumerable<T> toIterate, Action<T> action) {
            foreach (T item in toIterate) {
                action(item);
            }
        }

        public static string ListOut(this IEnumerable<string> toIterate) {
            return toIterate.Aggregate("", (s, t) => s + (string.IsNullOrWhiteSpace(s) ? "" : ", ") + t);
        }


        public static IList CloneCollection(object toClone) {
            Type collectionType = MakeCollectionType(toClone, typeof (List<>));
            return Activator.CreateInstance(collectionType) as IList;
        }

        public static IList CloneCollectionAndPopulate(object toClone, IEnumerable<object> contents) {
            IList newCollection = CloneCollection(toClone);
            contents.ForEach(o => newCollection.Add(o));
            return newCollection;
        }

        public static bool IsSet(Type type) {
            return IsGenericType(type, typeof (ISet<>));
        }

        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator) {
            return new EnumeratorWrapper<T>(enumerator);
        }

        public static IEnumerable ToEnumerable(this IEnumerator enumerator) {
            return new EnumeratorWrapper(enumerator);
        }

        public static IList ToTypedIList(IEnumerable<object> toWrap, Type instanceType) {
            Type typedListType = typeof (List<>).MakeGenericType(instanceType);
            var typedList = (IList) Activator.CreateInstance(typedListType);
            toWrap.ForEach(o => typedList.Add(o));
            return typedList;
        }

        public static string CollectionTitleString(INakedObjectSpecification elementSpecification, int size) {
            if (elementSpecification == null || elementSpecification.FullName.Equals(typeof (object).FullName)) {
                return CollectionTitleStringUnknownType(size);
            }
            return CollectionTitleStringKnownType(elementSpecification, size);
        }

        #endregion

        #region private

        private static bool IsGenericOfKeyValuePair(Type type) {
            return type.GetGenericArguments().Any(t => t != null && IsGenericType(t, typeof (KeyValuePair<,>)));
        }


        private static bool IsGenericOfRefType(Type type) {
            return type.GetGenericArguments().Count() == 1 && type.GetGenericArguments().All(t => !t.IsValueType);
        }

        private static bool IsNonGenericCollection(Type type) {
            return typeof (ICollection).IsAssignableFrom(type);
        }


        /// <summary> ICollection but not Array </summary>
        private static bool IsNonGenericCollectionClassButNotArray(Type type) {
            return IsNonGenericCollection(type) && !type.IsArray;
        }

        private static Type MakeCollectionType(object toClone, Type genericCollectionType) {
            Type itemType = toClone.GetType().IsGenericType ? toClone.GetType().GetGenericArguments().Single() : typeof (object);
            return genericCollectionType.MakeGenericType(itemType);
        }

        private static IEnumerable WrapWithSizedCollection(object toClone, int size) {
            Type collectionType = MakeCollectionType(toClone, typeof (WrappedSizedEnumerable<>));
            return Activator.CreateInstance(collectionType, toClone, size) as IEnumerable;
        }


        private static bool IsGenericType(Type type, Type toMatch) {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == toMatch || type.GetInterfaces().Any(interfaceType => IsGenericType(interfaceType, toMatch)));
        }

        private static Type GetGenericEnumerableType(Type type) {
            return type.IsGenericType ? (type.GetGenericTypeDefinition() == typeof (IEnumerable<>) ? type : type.GetInterfaces().FirstOrDefault(IsGenericEnumerable)) : null;
        }

        private static string CollectionTitleStringKnownType(INakedObjectSpecification elementSpecification, int size) {
            switch (size) {
                case IncompleteCollection:
                    return string.Format(Resources.NakedObjects.CollectionTitleUnloaded, elementSpecification.PluralName);

                case 0:
                    return string.Format(Resources.NakedObjects.CollectionTitleEmpty, elementSpecification.PluralName);

                case 1:
                    return string.Format(Resources.NakedObjects.CollectionTitleOne, elementSpecification.SingularName);

                default:
                    return string.Format(Resources.NakedObjects.CollectionTitleMany, size, elementSpecification.PluralName);
            }
        }

        private static string CollectionTitleStringUnknownType(int size) {
            switch (size) {
                case IncompleteCollection:
                    return string.Format(Resources.NakedObjects.CollectionTitleUnloaded, "");

                case 0:
                    return string.Format(Resources.NakedObjects.CollectionTitleEmpty, Resources.NakedObjects.Objects);

                case 1:
                    return string.Format(Resources.NakedObjects.CollectionTitleOne, Resources.NakedObjects.Object);

                default:
                    return string.Format(Resources.NakedObjects.CollectionTitleMany, size, Resources.NakedObjects.Objects);
            }
        }

        #endregion

        #region Nested type: EnumeratorWrapper

        private class EnumeratorWrapper<T> : IEnumerable<T> {
            private readonly IEnumerator<T> enumerator;

            public EnumeratorWrapper(IEnumerator<T> enumerator) {
                this.enumerator = enumerator;
            }

            #region IEnumerable<T> Members

            public IEnumerator<T> GetEnumerator() {
                return enumerator;
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            #endregion
        }

        private class EnumeratorWrapper : IEnumerable {
            private readonly IEnumerator enumerator;

            public EnumeratorWrapper(IEnumerator enumerator) {
                this.enumerator = enumerator;
            }

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator() {
                return enumerator;
            }

            #endregion
        }

        #endregion

        #region Nested type: WrappedSizedEnumerable

        private class WrappedSizedEnumerable<T> : IEnumerable<T> where T : class {
            private readonly int enumerableSize;
            private readonly int size;
            private readonly IEnumerable wrappedEnumerable;

            public WrappedSizedEnumerable(IEnumerable wrappedEnumerable, int size) {
                this.wrappedEnumerable = wrappedEnumerable;
                this.size = size;
                enumerableSize = wrappedEnumerable.Cast<object>().Count();
            }

            #region IEnumerable<T> Members

            IEnumerator<T> IEnumerable<T>.GetEnumerator() {
                for (int i = 0; i < size; i++) {
                    yield return i < enumerableSize ? wrappedEnumerable.Cast<T>().Skip(i).First() : null;
                }
            }

            public IEnumerator GetEnumerator() {
                return ((IEnumerable<T>) this).GetEnumerator();
            }

            #endregion
        }

        #endregion
    }
}