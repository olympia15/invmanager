using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using invmanager.Controllers;
using invmanager.Models;

using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using invmanager.Data;

namespace invmanager.Tests {
    public class ProductControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewWithProducts()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Index")
                .Options;

            using var context = new ApplicationDbContext(options); // <-- FIXED

            context.Products.AddRange(
                new Product { ProductName = "Item 1", ProductCategory = "Cat A", ProductPrice = 10.0, Quantity = 5, Stock = 10 },
                new Product { ProductName = "Item 2", ProductCategory = "Cat B", ProductPrice = 20.0, Quantity = 3, Stock = 5 }
            );
            context.SaveChanges();

            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(context, logger.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Product>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }
    }


}