namespace Ceql.Tests.Unit
{
    using System.Linq;
    using System.Reflection;
    using Ceql.Contracts.Attributes;
    using Ceql.Tests.Model;
    using Ceql.Utils;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TypeHelper
    {
        [TestMethod]
        public void Get_PrimaryKey_FieldsCount()
        {
            var type = typeof(Customer);
            var properties = type.GetPrimaryKeyProperties().ToList();

            Assert.IsTrue(properties.Count == 1);
        }


        [TestMethod]
        public void Get_PrimaryKey_FieldsCount_DerivedClass()
        {
            var type = typeof(RepeatCustomer);
            var properties = type.GetPrimaryKeyProperties().ToList();

            Assert.IsTrue(properties.Count == 1);
        }


        [TestMethod]
        public void Validate_PrimaryKey_Fields() 
        {
            var type = typeof(Customer);
            var properties = type.GetPrimaryKeyProperties().ToList();

            var pkProperties = type.GetProperties().Where(prop =>
                prop.GetCustomAttribute(typeof(PrimaryKey)) != null)
            .ToList();

            Assert.IsTrue(pkProperties.Intersect(properties).Count() == properties.Count);
        }


        [TestMethod]
        public void Validate_PrimaryKey_Fields_DerivedClass()
        {
            var type = typeof(RepeatCustomer);
            var properties = type.GetPrimaryKeyProperties().ToList();

            var pkProperties = type.GetProperties().Where(prop =>
                prop.GetCustomAttribute(typeof(PrimaryKey)) != null)
            .ToList();

            Assert.IsTrue(pkProperties.Intersect(properties).Count() == properties.Count);
        }

        [TestMethod]
        public void Validate_FieldName()
        {
            var type = typeof(Customer);
            var customerIdProperty = type.GetPrimaryKeyProperties()
                .FirstOrDefault(prop => prop.Name == "CustomerId");

            var fieldName = Utils.TypeHelper.GetFieldName(customerIdProperty);
            Assert.IsTrue(fieldName == "CUSTOMER_ID");
        }

    }
}
