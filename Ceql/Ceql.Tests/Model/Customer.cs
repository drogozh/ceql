using System;
using Ceql.Contracts;
using Ceql.Contracts.Attributes;

namespace Ceql.Tests.Model
{
    [Schema("CEQL_TEST")]
    [Table("CUSTOMER")]
    public class Customer : ITable
    {
        [Field("CUSTOMER_ID")]
        [AutoSequence]
        [PrimaryKey]
        public int CustomerId { get; set; }

        [Field("NAME")]
        public string Name { get; set; }

        [Field("CREATION_DT")]
        public DateTime CreationDate { get; set; }
    }
}
