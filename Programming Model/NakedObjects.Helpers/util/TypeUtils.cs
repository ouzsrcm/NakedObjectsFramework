// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NakedObjects.Util {
    /// <summary>
    /// Utility methods for safely obtaining and using types defined within
    /// a domain model.  The Naked Objects framework makes extensive
    /// use of these utils, but they are provided within the NakedObjects.Helpers
    /// assembly to permit optional use within domain code.
    /// </summary>
    public static class TypeUtils {
        private const string SystemTypePrefix = "System.";
        private const string NakedObjectsTypePrefix = "NakedObjects.";
        private const string NakedObjectsProxyPrefix = "NakedObjects.Proxy.";
        private const string EntityProxyPrefix = "System.Data.Entity.DynamicProxies.";
        private const string EntityTypePrefix = "NakedObjects.EntityObjectStore.";
        private const string CastleProxyPrefix = "Castle.Proxies.";

        public static object NewInstance(Type type) {
            return Activator.CreateInstance(type);
        }

        public static Type ImplementingTypeOrNull(Type typeCandidate, Type requiredType) {
            if (typeCandidate == null) {
                return null;
            }
            if (!requiredType.IsAssignableFrom(typeCandidate)) {
                return null;
            }

            if (typeCandidate.GetConstructor(Type.EmptyTypes) == null) {
                return null;
            }

            if (typeCandidate.IsPublic || typeCandidate.IsNestedPublic) {
                return typeCandidate;
            }
            return null;
        }
    
        private static readonly HashSet<Assembly> AssemblyCache = new HashSet<Assembly>();
        private static readonly IDictionary<string, Type> TypeCache = new Dictionary<string, Type>();

        internal static void ClearCache() {
            lock (TypeCache) {
                TypeCache.Clear();
            }
            lock (AssemblyCache) {
                AssemblyCache.Clear();
            }
        }

        internal static Type GetTypeFromLoadedAssembliesInternal(string typeName) {
            lock (TypeCache) {
                if (TypeCache.ContainsKey(typeName)) {
                    return TypeCache[typeName];
                }
            }

            Assembly[] cachedAssemblies; 

            lock (AssemblyCache) {
                cachedAssemblies = new Assembly[AssemblyCache.Count];
                AssemblyCache.CopyTo(cachedAssemblies, 0);
            }

            foreach (Assembly assembly in cachedAssemblies) {
                Type type = assembly.GetType(typeName);
                if (type != null) {
                    lock (TypeCache) {
                        if (TypeCache.ContainsKey(typeName)) {
                            return TypeCache[typeName];
                        }

                        TypeCache[typeName] = type;
                        return type;
                    }
                }
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                Type type = assembly.GetType(typeName);
                if (type != null) {
                    lock (TypeCache) {
                        if (TypeCache.ContainsKey(typeName)) {
                            return TypeCache[typeName];
                        }

                        lock (AssemblyCache) {
                            AssemblyCache.Add(assembly);
                        }

                        TypeCache[typeName] = type;
                        return type;
                    }
                }
            }
            return null;
        }


        public static Type GetType(string typeName) {
            return GetTypeFromLoadedAssembliesInternal(typeName);
        }

        public static Type ImplementingTypeOrNull(string classCandidateName, Type requiredType) {
            if (classCandidateName == null) {
                return null;
            }
            try {
                Type classCandidate = GetType(classCandidateName);
                return ImplementingTypeOrNull(classCandidate, requiredType);
            }
            catch (Exception) {
                return null;
            }
        }

        public static string[] GetInterfaces(Type type) {
            return type.GetInterfaces().Select(x => x.FullName).ToArray();
        }

        public static string GetBaseType(Type type) {
            return type.BaseType == null ? null : type.BaseType.FullName;
        }

        public static bool IsString(Type type) {
            return type.Equals(typeof (string));
        }

        public static bool IsPublic(Type type) {
            return type.IsPublic || type.IsNestedPublic || type.IsByRef || IsSystem(type);
        }

        public static bool IsSystem(Type type) {
            return IsSystem(type.FullName ?? "") && !IsEntityProxy(type);
        }

        public static bool IsSystem(string typeName) {
            return typeName.StartsWith(SystemTypePrefix);
        }

        public static bool IsNakedObjectsProxy(Type type) {
            return IsNakedObjectsProxy(type.FullName ?? "");
        }

        public static bool IsNakedObjectsProxy(string typeName) {
            return typeName.StartsWith(NakedObjectsProxyPrefix);
        }

        
        public static bool IsCastleProxy(Type type) {
            return IsCastleProxy(type.FullName ?? "");
        }

        public static bool IsCastleProxy(string typeName) {
            return typeName.StartsWith(CastleProxyPrefix);
        }

        public static bool IsEntityProxy(Type type) {
            return IsEntityProxy(type.FullName ?? "");
        }

        public static bool IsEntityProxy(string typeName) {
            return typeName.StartsWith(EntityProxyPrefix);
        }

        public static bool IsProxy(Type type) {
            return IsProxy(type.FullName ?? "");
        }

        public static bool IsProxy(string typeName) {
            return IsEntityProxy(typeName) || IsNakedObjectsProxy(typeName) || IsCastleProxy(typeName);
        }

        public static bool IsNakedObjects(Type type) {
            return IsNakedObjects(type.FullName ?? "");
        }

        public static bool IsNakedObjects(string typeName) {
            return typeName.StartsWith(NakedObjectsTypePrefix);
        }

        public static bool IsEntityDomainObject(Type type) {
            return type != null && (IsEntityDomainObject(type.FullName ?? "") || IsEntityDomainObject(type.BaseType));
        }

        public static bool IsEntityDomainObject(string typeName) {
            return typeName.StartsWith(EntityTypePrefix);
        }

        public static string GetProxiedTypeFullName(this Type type) {
            return IsProxy(type) ? type.BaseType.FullName : type.FullName;
        }

        public static Type GetProxiedType(this Type type) {
            return IsProxy(type) ? type.BaseType : type;
        }


        public static T CreateGenericInstance<T>(Type genericTypeDefinition,
                                                 Type[] genericTypeParms,
                                                 object[] constructorParms) {
            Type genericType = genericTypeDefinition.MakeGenericType(genericTypeParms);
            var constructorTypes = new List<Type>();
            foreach (object obj in constructorParms) {
                constructorTypes.Add(obj.GetType());
            }
            ConstructorInfo ctor = genericType.GetConstructor(constructorTypes.ToArray());
            var constructedObject = (T) ctor.Invoke(constructorParms);
            return constructedObject;
        }

        public static bool IsNullableType(Type type) {
            return type.IsGenericType && (typeof (Nullable<>).Equals(type.GetGenericTypeDefinition()));
        }

        public static Type GetNulledType(Type type) {
            if (IsNullableType(type)) {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        public static bool IsEnum(Type type) {
            return (typeof (Enum).IsAssignableFrom(type));
        }

        public static bool IsIntegralValueForEnum(object obj) {
            // except char as not valid integral type for enum 
            return obj is sbyte || obj is byte || obj is short || obj is ushort || obj is int || obj is uint || obj is long || obj is ulong;
        }

        internal static MemberInfo GetProperty(LambdaExpression expression) {
            if (expression == null) {
                throw new ArgumentNullException("expression");
            }

            if (expression.Body.NodeType == ExpressionType.MemberAccess) {
                return ((MemberExpression) expression.Body).Member;
            }

            if (expression.Body.NodeType == ExpressionType.Convert) {
                Expression op = ((UnaryExpression) expression.Body).Operand;

                if (op.NodeType == ExpressionType.MemberAccess) {
                    return ((MemberExpression) op).Member;
                }
            }

            throw new ArgumentException("must be member access");
        }

        public static string PropertyName<TTarget, TProperty>(this TTarget target, Expression<Func<TTarget, TProperty>> expr) {
            return GetProperty(expr).Name;
        }

        public static bool IsPropertyMatch<TTarget, TProperty>(this TTarget target, string memberName, Expression<Func<TTarget, TProperty>> expr) {
            return target.PropertyName(expr) == memberName;
        }

        public static bool IsPropertyMatch<TTarget, TProperty>(this object target, string memberName, Expression<Func<TTarget, TProperty>> expr) {
            return target is TTarget && GetProperty(expr).Name == memberName;
        }


        // ** Lifted from MSDN **  
        //    The identity most users would expect MemberInfos (other than Type) to have is not what Reflection provides.
        //    So for example most folks would expect the following program to print true instead of false:
        //
        //    public class B { public void M() { } }
        //    public class D : B { }
        //    public class Program
        //    {
        //        public static void Main
        //        {
        //            Console.WriteLine(typeof(B).GetMethod("M") == typeof(D).GetMethod("M"));
        //        }
        //    }
        //
        // The MethodInfos are not equal because ReflectedType is included in the identity of MethodInfos.
        // The identity Reflection uses for MethdInfo can be expressed as:
        // DeclaringType + MethodName + Signature + DeclaringType instantiation + Method Instantiation + ReflectedType
        // And what most users would expect it to be can be expressed as:
        // DeclaringType + MethodName + Signature + DeclaringType Instantiation + Method Instantiation
        // We can�t make them match because that would be a breaking change. However here is a little code snippet
        // that will do the comparison using the latter definition of identity. This will work for all subclasses of
        // MemberInfo (Type, MethodBase, MethodInfo, ConstructorInfo, FieldInfo, PropertyInfo and EventInfo):


        public static bool MemberInfoEquals(this MemberInfo lhs, MemberInfo rhs) {
            if (lhs == rhs) {
                return true;
            }

            if (lhs.DeclaringType != rhs.DeclaringType) {
                return false;
            }

            // Methods on arrays do not have metadata tokens but their ReflectedType
            // always equals their DeclaringType

            if (lhs.DeclaringType != null && lhs.DeclaringType.IsArray) {
                return false;
            }

            if (lhs.MetadataToken != rhs.MetadataToken || lhs.Module != rhs.Module) {
                return false;
            }

            if (lhs is MethodInfo) {
                var lhsMethod = lhs as MethodInfo;
                if (lhsMethod.IsGenericMethod) {
                    var rhsMethod = rhs as MethodInfo;
                    Type[] lhsGenArgs = lhsMethod.GetGenericArguments();
                    Type[] rhsGenArgs = rhsMethod.GetGenericArguments();
                    for (int i = 0; i < rhsGenArgs.Length; i++) {
                        if (lhsGenArgs[i] != rhsGenArgs[i]) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}