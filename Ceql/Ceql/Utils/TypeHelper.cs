using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ceql.Contracts.Attributes;

namespace Ceql.Utils
{
    public static class TypeHelper
    {
        /// <summary>
        /// Enumerates the type properties.
        /// </summary>
        /// <returns>The type properties.</returns>
        /// <param name="type">Type.</param>
        public static IEnumerable<String> EnumerateTypeProperties(this Type type)
        {
            return type.GetProperties().Select(x => x.Name);
        }

        /// <summary>
        /// Searches inheritance chain for the attribute
        /// </summary>
        /// <typeparam name="T">Attribute type to look for</typeparam>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(Type sourceType) where T : System.Attribute
        {
            while (sourceType != null)
            {
                var attr = sourceType.GetTypeInfo().GetCustomAttribute<T>();
                if (attr != null)
                {
                    return attr;
                }
                sourceType = sourceType.GetTypeInfo().BaseType;
            }

            return default(T);
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <returns>The type.</returns>
        /// <param name="sourceType">Source type.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static Type GetType<T>(Type sourceType) where T : System.Attribute
        {
            while (sourceType != null)
            {
                var attr = sourceType.GetTypeInfo().GetCustomAttribute<T>();
                if (attr != null)
                {
                    return sourceType;
                }
                sourceType = sourceType.GetTypeInfo().BaseType;
            }

            return null;
        }

        /// <summary>
        /// Gets properties of a type that have attributes of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetPropertiesForAttribute<T>(Type sourceType) where T : System.Attribute
        {
            var properties = sourceType.GetTypeInfo().GetProperties();
            return properties.Where(p => p.GetCustomAttribute<T>() != null).ToList();
        }

        /// <summary>
        /// Gets the primary key properties decorated with [PrimaryKey] attribute
        /// </summary>
        /// <returns>The primary key.</returns>
        /// <param name="type">Source type.</param>
        public static List<PropertyInfo> GetPrimaryKeyProperties(this Type type)
        {
            return type.GetProperties().Where(property => property.GetCustomAttribute(typeof(PrimaryKey)) != null).ToList();
        }

        /// <summary>
        /// Loads the type.
        /// </summary>
        /// <returns>The type.</returns>
        /// <param name="typeName">Type name.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T LoadType<T>(string typeName) where T : class
        {
            var type = Type.GetType(typeName);
            var instance = Activator.CreateInstance(type) as T;
            return instance;
        }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <returns>The field name.</returns>
        /// <param name="property">Property.</param>
        public static string GetFieldName(PropertyInfo property) 
        {
            var fieldAttr = property.GetCustomAttribute<Field>();
            if(fieldAttr == null) 
            {
                return null;
            }
            return fieldAttr.Name;
        }

        /// <summary>
        /// Returns default value for Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetDefaultValue(Type type)
        {
            if(type.IsValueType)
            {
                Activator.CreateInstance(type);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDefaultValue(object value)
        {
            var type = value.GetType();
            if(type.IsValueType)
            {
                var defaultValue = Activator.CreateInstance(type);
                return value.Equals(defaultValue);
            } 
            else 
            {
                return value == null;
            }
        }
    }
}
