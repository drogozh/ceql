using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Ceql.Composition;
using Ceql.Expressions;
using Ceql.Model;
using Ceql.Contracts.Attributes;
using Ceql.Contracts;

namespace Ceql.Execution
{
    internal class ParameterResultInfo
    {
        public Type Type;
        public List<ArgumentAlias> ArgumentMapping;
        public List<Tuple<string, string, MemberInfo>> MemberMapping;
        public ConstructorInfo Constructor;
        public object[] ConstArgumentsBuffer;

        public object CreateInstance(IVirtualDataReader reader, IConnectorFormatter formatter)
        {
            
            //set contructor arguments
            for (var i = 0; i < ConstArgumentsBuffer.Length; i++)
            {
                var argumentAlias = ArgumentMapping[i];
                var resultObject = formatter.FormatFrom(reader[argumentAlias.Alias]);

                if(resultObject is DBNull) {
                    ConstArgumentsBuffer[i] = null;
                } else {
                    ConstArgumentsBuffer[i] = resultObject;
                }
            }

            //create instance
            var instance = Constructor.Invoke(ConstArgumentsBuffer);

            //set properties on an instance
            for (var i = 0; i < MemberMapping.Count; i++)
            {
                var tuple = MemberMapping[i];
                var property = tuple.Item3 as PropertyInfo;
                var v = formatter.FormatFrom(property,reader[tuple.Item1]);

                // null values are skipped
                // property vlues will be set to their defaults
                if (v == DBNull.Value) continue;

                //set field
                var field = tuple.Item3 as FieldInfo;
                if(field != null) SetValue(instance,field,v);

                //set property only if set method is available
                if(property != null && property.SetMethod != null) SetValue(instance, property, v);
            }

            return instance;
        }


        /// <summary>
        /// Sets value on a property
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="info"></param>
        /// <param name="value"></param>
        private void SetValue(object instance, PropertyInfo info, object value)
        {
            if (IsNullable(info))
            {
                info.SetValue(instance, value);
                return;
            }
            info.SetValue(instance,value);
        }

        /// <summary>
        /// Sets value on a field info
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="info"></param>
        /// <param name="value"></param>
        private void SetValue(object instance, FieldInfo info, object value)
        {
            if (IsNullable(info))
            {
                info.SetValue(instance, value);
                return;
            }
            info.SetValue(instance,value);
        }



        public Boolean IsNullable(PropertyInfo pInfo)
        {
            return pInfo.PropertyType.GetTypeInfo().IsGenericType &&
                   pInfo.PropertyType.GetGenericTypeDefinition() == typeof (Nullable<>);
        }

        public Boolean IsNullable(FieldInfo fInfo)
        {
            return fInfo.FieldType.GetTypeInfo().IsGenericType &&
                   fInfo.FieldType.GetGenericTypeDefinition() == typeof (Nullable<>);
        }

    }

}