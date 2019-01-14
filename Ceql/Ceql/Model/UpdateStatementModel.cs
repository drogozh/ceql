using Ceql.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ceql.Model
{
    public class UpdateStatementModel<T> : StatementModel<T>
    {
        
        public UpdateStatementModel(IConnectorFormatter formatter) : base(formatter)
        { }


        protected override string GetSql()
        {
            return null;
        }
    }
}