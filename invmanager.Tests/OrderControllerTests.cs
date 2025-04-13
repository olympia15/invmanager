using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using invmanager.Controllers;
using invmanager.Models;
using invmanager.Data;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;

public class OrderControllerTests
{
    [Fact]
    public void Index_ReturnsViewResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("OrderControllerTestDb")
            .Options;

        using var context = new ApplicationDbContext(options);

        context.Products.AddRange(
            new Product { ProductName = "Test1", ProductCategory = "Cat1", ProductPrice = 10, Quantity = 5, Stock = 10 },
            new Product { ProductName = "Test2", ProductCategory = "Cat2", ProductPrice = 15, Quantity = 3, Stock = 5 }
        );
        context.SaveChanges();

        var logger = new Mock<ILogger<OrderController>>();
        var controller = new OrderController(context, logger.Object);

        // Mock TempData
        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        controller.TempData = tempData;

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }
}