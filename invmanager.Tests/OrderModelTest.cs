
using Xunit;
using invmanager.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public class OrderModelTests
{
    [Fact]
    public void Order_WithAllValidProperties_ShouldBeValid()
    {
        // Arrange
        var order = new Order
        {
            Quantity = 10,
            OrderTotal = 150.0,
            Status = "Confirmed",
            CustomerName = "Jane Doe",
            CustomerEmail = "jane@example.com"
        };

        // Act
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(order, new ValidationContext(order), results, true);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Order_MissingRequiredFields_ShouldBeInvalid()
    {
        
        var order = new Order(); // No fields set

        
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(order, new ValidationContext(order), results, true);

        
        Assert.False(isValid);
        Assert.True(results.Count >= 1);
    }

    [Fact]
    public void Order_QuantityOutOfRange_ShouldBeInvalid()
    {
       
        var order = new Order
        {
            Quantity = 150, //
            OrderTotal = 50.0,
            Status = "Pending",
            CustomerName = "Bob",
            CustomerEmail = "bob@example.com"
        };

        
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(order, new ValidationContext(order), results, true);

        
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Quantity"));
    }
}
