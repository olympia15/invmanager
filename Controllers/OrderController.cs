using Microsoft.AspNetCore.Mvc;
using invmanager.Models;
using invmanager.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace invmanager.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        

        public OrderController(ApplicationDbContext context, ILogger<OrderController> loggerObject)
        {
            _context = context;
            
        }

        [HttpGet]
        public IActionResult Index()
        {
            //display products 
            var products = _context.Products.ToList();
            if (products == null || !products.Any())
            {
                return NotFound("No products available.");
            }
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToOrderConfirmed(string CustomerName, string CustomerEmail, int ProductId, int Quantity)
        {
            //checks if customer name or email is empty 
            if (string.IsNullOrEmpty(CustomerName) || string.IsNullOrEmpty(CustomerEmail))
            {
                return BadRequest("Customer name and email are required.");
            }
            
            //stores data for use in other requests (this was an alternative to sessions and cookies)
            TempData["CustomerName"] = CustomerName;
            TempData["CustomerEmail"] = CustomerEmail;
            
            //find product using id 
            var product = _context.Products.Find(ProductId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }
            
            //find orders that are "inprogress"
            var order = _context.Orders.Include(o => o.OrderProducts)
                .FirstOrDefault(o => o.Status == "InProgress" && o.CustomerName == CustomerName && o.CustomerEmail == CustomerEmail);
            
            //check if order exists 
            if (order == null)
            {
                //create new if not exist 
                order = new Order
                {
                    Status = "InProgress",
                    CustomerName = CustomerName,
                    CustomerEmail = CustomerEmail,
                    OrderProducts = new List<OrderProduct>()
                };
                //save it to the database 
                _context.Orders.Add(order);
                _context.SaveChanges();
            }
            
            
            if (order.OrderProducts == null)
            {
                //create the ordersproduct collection 
                order.OrderProducts = new List<OrderProduct>();
            }
            
            //  find exisiting orders 
            var existingOrderProduct = order.OrderProducts.FirstOrDefault(op => op.ProductId == ProductId);
            if (existingOrderProduct != null)
            {
                //if the order exists increase the quantity 
                existingOrderProduct.Quantity += Quantity;
            }
            else
            {
                //if it doesnt exist create a new order 
                var orderProduct = new OrderProduct
                {
                    OrderId = order.OrderId,
                    ProductId = product.ProductId,
                    Quantity = Quantity
                };
                _context.OrderProducts.Add(orderProduct);
                order.OrderProducts.Add(orderProduct);
            }
            
            //save to database 
            _context.SaveChanges();
            
            //display success message if product added to order succcessfully 
            TempData["SuccessMessage"] = "Product added to order successfully!";
            return RedirectToAction("Index", new { CustomerName, CustomerEmail });
        }
        

        [HttpGet]
        public IActionResult SummaryOfOrder(string CustomerName, string CustomerEmail)
        {
            //check if customer name or email are empty 
            if (string.IsNullOrEmpty(CustomerName) || string.IsNullOrEmpty(CustomerEmail))
            {
                return BadRequest("Customer name and email are required.");
            }
            
            //retrieves orders using name and email
            var orders = _context.Orders.Include(o => o.OrderProducts).ThenInclude(op => op.Product)
                .Where(o => o.CustomerName == CustomerName && o.CustomerEmail == CustomerEmail)
                .ToList();
            
            //if no orders are found for the customer 
            if (orders == null || !orders.Any())
            {
                return NotFound($"No orders found for customer {CustomerName}.");
            }

            // Check if there are any unconfirmed orders
            ViewBag.HasUnconfirmedOrders = orders.Any(o => o.Status == "InProgress");
            ViewBag.CustomerName = CustomerName;
            return View(orders);
        }

        [HttpGet]
        public IActionResult ConfirmOrder(int OrderId)
        {
            // Get order from database using its id
            var order = _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefault(o => o.OrderId == OrderId);
    
            // Check if order is found
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            // Check stock availability and update stock
            foreach (var orderProduct in order.OrderProducts)
            {
                var product = orderProduct.Product;
        
                // Adding logging for product stock details
                Console.WriteLine($"Product ID: {product.ProductId}, Available Stock: {product.Stock}, Ordered Quantity: {orderProduct.Quantity}");

                if (product.Stock < orderProduct.Quantity)
                {
                    return BadRequest($"Not enough stock for product {product.ProductName}. Available: {product.Stock}, Requested: {orderProduct.Quantity}.");
                }

                // Decrease stock
                product.Stock -= orderProduct.Quantity;
                _context.Products.Update(product);
            }

            // Update status to confirmed
            order.Status = "Confirmed";
            _context.SaveChanges();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") // AJAX request detection
            {
                var orders = _context.Orders.Include(o => o.OrderProducts).ThenInclude(op => op.Product).ToList();
                return PartialView("_OrderSummaryPartial", orders);
            }
            else
            {
                return RedirectToAction("SummaryOfOrder", new { CustomerName = order.CustomerName, CustomerEmail = order.CustomerEmail });
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromOrder(int OrderId, int ProductId)
        {
            //get order using its id 
            var order = _context.Orders.Include(o => o.OrderProducts).FirstOrDefault(o => o.OrderId == OrderId);
            if (order == null)
            {
                return NotFound("Order not found.");
            }
            
            //find product using product id 
            var orderProduct = order.OrderProducts.FirstOrDefault(op => op.ProductId == ProductId);
            if (orderProduct == null)
            {
                return NotFound("Product not found in order.");
            }

            _context.OrderProducts.Remove(orderProduct);

            if (!order.OrderProducts.Any())
            {
                _context.Orders.Remove(order);
            }

            _context.SaveChanges();

            return RedirectToAction("SummaryOfOrder", new { CustomerName = order.CustomerName, CustomerEmail = order.CustomerEmail });
        }
    }
}
