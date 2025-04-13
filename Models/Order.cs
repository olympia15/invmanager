using System.ComponentModel.DataAnnotations;


namespace invmanager.Models
{
    public class Order 
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [Range(1, 100)]
        
        public  int Quantity { get; set; }

        public double OrderTotal { get; set; } 

        [Required]
        public string Status { get; set; } = "Pending"; 

        [Required]
        
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; }
        
        public ICollection<OrderProduct>? OrderProducts { get; set; }
    }
}