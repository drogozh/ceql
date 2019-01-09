namespace Ceql.Generation
{
    using Ceql.Composition;
    using Ceql.Statements;
    using Ceql.Contracts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Ceql.Model;
    using Attributes = Ceql.Contracts.Attributes;
    using Ceql.Utils;
    using Ceql.Configuration;
    using System.Reflection;

    public static class UpdateStatementGenerator
    {
        public static UpdateStatementModel<T> Generate<T>(UpdateStatement<T> statement) where T : ITable
        {
            return null;
        }
    }
}