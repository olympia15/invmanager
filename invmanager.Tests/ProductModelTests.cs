using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using invmanager.Models;
using Xunit;

namespace invmanager.Tests
{
    public class ProductModelTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void Product_WithValidData_ShouldBeValid()
        {
            var product = new Product
            {
                ProductName = "Laptop",
                ProductCategory = "Electronics",
                ProductPrice = 666.99,
                Quantity = 10,
                Stock = 5
            };

            var results = ValidateModel(product);
            Assert.Empty(results);
        }

        [Fact]
        public void Product_WithoutName_ShouldBeInvalid()
        {
            var product = new Product
            {
                ProductName = null,
                ProductCategory = "Electronics",
                ProductPrice = 100,
                Quantity = 5,
                Stock = 2
            };

            var results = ValidateModel(product);
            Assert.Contains(results, v => v.MemberNames.Contains("ProductName"));
        }
    }
}