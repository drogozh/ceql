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
    class SelectEnumerator<TResult> : IEnumerator<TResult>
    {
        private IEnumerable<SelectAlias> _selectList; 

        private List<TResult> _result;

        private IEnumerator<TResult> listEnumerator;

        private IConnectorFormatter _formatter;

        public SelectEnumerator(SelectClause<TResult> selectClause, IConnectorFormatter formatter)
        {
            var rowLambda = selectClause.SelectExpression as LambdaExpression;
            var compiledRowLambda = rowLambda.Compile();
            var paramInfo = new List<ParameterResultInfo>();
            _formatter = formatter;

            var generatedSelect = selectClause.Model;
            _selectList = generatedSelect.SelectList;
            
            foreach (var x in rowLambda.Parameters)
            {
                //look for the type in the select list
                var sel = _selectList.FirstOrDefault(s => s.SourceType == x.Type);
                if (sel == null)
                {
                    paramInfo.Add(null);
                    continue;
                }

                paramInfo.Add(new ParameterResultInfo()
                {
                    Type = x.Type,
                    ArgumentMapping = ConstructorArgumentsMap(x.Type),
                    MemberMapping = PropertyMapping(x.Type),
                    Constructor = x.Type.GetConstructors()[0],
                    ConstArgumentsBuffer = new object[x.Type.GetConstructors()[0].GetParameters().Length]
                });

            }

            var connection = VirtualDataSource.GetConnection();
            
            _result = new List<TResult>();

            connection.Run(selectClause, (reader) =>
            {
                /*
                    for each row:
                    for each parameter
                    1. instantiate parameter
                    2. set property values
                    3. execute lambda expression
                    4. cast to result
                */

                var arguments = new object[paramInfo.Count];

                for (var i = 0; i < paramInfo.Count; i++)
                {
                    var p = paramInfo[i];
                    if (p != null) arguments[i] = p.CreateInstance(reader,_formatter);
                }
                _result.Add((TResult) compiledRowLambda.DynamicInvoke(arguments));
            });

            listEnumerator = _result.GetEnumerator();
        }

        public void Dispose()
        {
            listEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            return listEnumerator.MoveNext();
        }

        public void Reset()
        {
            listEnumerator.Reset();
        }
        public TResult Current {
            get { return listEnumerator.Current; }
        }
        object IEnumerator.Current
        {
            get { return listEnumerator.Current; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<ArgumentAlias> ConstructorArgumentsMap(Type type)
        {
            var constructor = type.GetConstructors()[0];
            var arguments = constructor.GetParameters();

            var argumentMap = new List<ArgumentAlias>();
            var selects = _selectList.Where(l => l.SourceType == type).ToList();

            foreach (var p in arguments)
            {
                var tsel = selects.FirstOrDefault(s => s.SourceMember.Name == p.Name);
                if (tsel != null)
                {
                    argumentMap.Add(new ArgumentAlias { 
                        Alias = tsel.Alias,
                        ArgumentType = p.ParameterType
                    });
                }
                else
                {
                    argumentMap.Add(null);
                }

            }

            return argumentMap;
        }


        private List<Tuple<string, string, MemberInfo>> PropertyMapping(Type type)
        {
            var result = new List<Tuple<string, string,MemberInfo>>();

            var properties = type.GetProperties();
            var fields = type.GetFields();

            var selects = _selectList.Where(l => l.SourceType == type).ToList();

            //with only a single select with name * index all class properties
            if (selects.Any(x=>x.Name == "*"))
            {

                foreach (var prop in properties)
                {
                    var tField = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof (Field));
                    if (tField == null) continue;
                    result.Add(new Tuple<string, string,MemberInfo>(tField.ConstructorArguments[0].Value.ToString(), prop.Name, prop));
                }
                return result;
            }


            foreach (var prop in properties)
            {
                var tsel = selects.FirstOrDefault(s => s.SourceMember.Name == prop.Name);
                if (tsel == null) continue;
                result.Add(new Tuple<string, string, MemberInfo>(tsel.Alias, prop.Name,prop));
            }

            
            foreach (var field in fields)
            {
                var tsel = selects.FirstOrDefault(s => s.SourceMember.Name == field.Name);
                if (tsel == null) continue;
                result.Add(new Tuple<string, string,MemberInfo>(tsel.Alias, field.Name,field));
            }
            

            return result;
        }
    }

    public class ArgumentAlias 
    {
        public Type ArgumentType {get; set;}
        public string Alias {get; set;}
    }
}
