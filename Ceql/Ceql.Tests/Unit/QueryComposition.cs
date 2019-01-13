
namespace Ceql.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Ceql.AbstractComposer;
    using Ceql.Tests.Model;
    using Ceql.Configuration;
    using Ceql.Statements;

    [TestClass]
    public class QueryComposition
    {
        [TestInitialize]
        public void Init() 
        {
            CeqlConfiguration.Load("Configs/ceql.mysql.json");
        }


        [TestMethod]
        public void Select_SingleField()
        {
            var sql = From<Customer>().Select(c => c.Name).Sql;
            Assert.IsTrue(sql == "SELECT T0.NAME T0_NAME FROM `CEQL_TEST`.`CUSTOMER` T0");
        }


        [TestMethod]
        public void Select_MultipleFields()
        {
            var sql = From<Customer>().Select(c => new 
            { 
                c.CustomerId,
                c.Name,
                c.CreationDate
            }).Sql;
            Assert.IsTrue(sql == "SELECT T0.CUSTOMER_ID T0_CUSTOMER_ID,T0.NAME T0_NAME,T0.CREATION_DT T0_CREATION_DT FROM `CEQL_TEST`.`CUSTOMER` T0");
        }


        [TestMethod]
        public void Update_SingleField_EntireTable()
        {
            var sql =
            new UpdateStatement<Customer>(customer => customer.CreationDate)
            .Values("2019-01-01").Sql;
           
            Assert.IsTrue(sql == "UPDATE `CEQL_TEST`.`CUSTOMER` T0 SET T0.CREATION_DT = '2019-01-01'");
        }


        [TestMethod]
        public void Update_TypeCast()
        {
            var sql =
            new UpdateStatement<Customer>(customer => customer.CreationDate)
            .Values("2019-01-01").Sql;

            Assert.IsTrue(sql == "UPDATE `CEQL_TEST`.`CUSTOMER` T0 SET T0.CREATION_DT = '2019-01-01'");
        }


        [TestMethod]
        public void Update_SingleField_SingleRecord()
        {
            var sql =
            new UpdateStatement<Customer>(customer => customer.CreationDate)
            .Values("2019-01-01")
            .Where(customer => customer.CustomerId == 1).Sql;

            Assert.IsTrue(sql == "UPDATE `CEQL_TEST`.`CUSTOMER` T0 SET T0.CREATION_DT = '2019-01-01' WHERE T0.CUSTOMER_ID=1");
        }
    }
}
