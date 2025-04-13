using System.ComponentModel.DataAnnotations;


namespace invmanager.Models;


public class Product
{
    public int ProductId { get; set; }
    
    [Required]

    public string ProductName { get; set; }

    [Required]
    
    public string ProductCategory { get; set; }
    
    public double ProductPrice { get; set; }
    
    public int Quantity { get; set; }
    
    public int Stock { get; set; }
    
    public ICollection<OrderProduct>? OrderProducts { get; set; }
    
}