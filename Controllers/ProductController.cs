
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using invmanager.Models;
using invmanager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace invmanager.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ApplicationDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet] // Display products
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching all products at {Time}", DateTime.Now);
                var products = await _context.Products.ToListAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching products at {Time}", DateTime.Now);
                return RedirectToAction("Error", new { statusCode = 500 });
            }
        }

        // Search method
        [HttpGet]
        public async Task<IActionResult> SearchAndFilter(string query, string category, double? minPrice, double? maxPrice, bool lowStock, string sortBy)
        {
            try
            {
                var products = _context.Products.AsQueryable();

                // Search by name or category
                if (!string.IsNullOrEmpty(query))
                    products = products.Where(p => p.ProductName.Contains(query) || p.ProductCategory.Contains(query));

                // Filter by category
                if (!string.IsNullOrEmpty(category) && category != "All Categories")
                    products = products.Where(p => p.ProductCategory == category);

                // Filter by price range
                if (minPrice.HasValue)
                    products = products.Where(p => p.ProductPrice >= minPrice);

                if (maxPrice.HasValue)
                    products = products.Where(p => p.ProductPrice <= maxPrice);

                // Show only low-stock products
                if (lowStock)
                    products = products.Where(p => p.Quantity < p.Stock);

                // Sort results
                products = sortBy switch
                {
                    "Price" => products.OrderBy(p => p.ProductPrice),
                    "Quantity" => products.OrderBy(p => p.Quantity),
                    "Name" => products.OrderBy(p => p.ProductName),
                    _ => products.OrderBy(p => p.ProductName)
                };

                _logger.LogInformation("Fetching filtered products at {Time}", DateTime.Now);
                return PartialView("_ProductListPartial", await products.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching and filtering products at {Time}", DateTime.Now);
                return RedirectToAction("Error", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            _logger.LogInformation("Accessing Create Product page at {Time}", DateTime.Now);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Products.Add(product);
                    _context.SaveChanges();
                    _logger.LogInformation("Product created successfully at {Time}", DateTime.Now);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while creating a product at {Time}", DateTime.Now);
                    return RedirectToAction("Error", new { statusCode = 500 });
                }
            }
            _logger.LogWarning("Product creation failed due to invalid model at {Time}", DateTime.Now);
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product == null)
                {
                    _logger.LogWarning("Product not found during edit attempt. Product ID: {ProductId} at {Time}", id, DateTime.Now);
                    return NotFound();
                }
                _logger.LogInformation("Accessing Edit page for Product ID: {ProductId} at {Time}", id, DateTime.Now);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while accessing the Edit page for Product ID: {ProductId} at {Time}", id, DateTime.Now);
                return RedirectToAction("Error", new { statusCode = 500 });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit([Bind("ProductId,ProductName,ProductCategory,ProductPrice,Quantity,Stock")] Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            try
            {
                _context.Products.Update(product);
                _context.SaveChanges();
                _logger.LogInformation("Product edited successfully. Product ID: {ProductId} at {Time}", product.ProductId, DateTime.Now);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while editing Product ID: {ProductId} at {Time}", product.ProductId, DateTime.Now);
                if (!ProductExists(product.ProductId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing Product ID: {ProductId} at {Time}", product.ProductId, DateTime.Now);
                return RedirectToAction("Error", new { statusCode = 500 });
            }
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
                if (product == null)
                {
                    _logger.LogWarning("Product not found during delete attempt. Product ID: {ProductId} at {Time}", id, DateTime.Now);
                    return NotFound();
                }
                _logger.LogInformation("Accessing Delete page for Product ID: {ProductId} at {Time}", id, DateTime.Now);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while accessing the Delete page for Product ID: {ProductId} at {Time}", id, DateTime.Now);
                return RedirectToAction("Error", new { statusCode = 500 });
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
                if (product != null)
                {
                    _context.Products.Remove(product);
                    _context.SaveChanges();
                    _logger.LogInformation("Product deleted successfully. Product ID: {ProductId} at {Time}", id, DateTime.Now);
                    return RedirectToAction("Index");
                }
                _logger.LogWarning("Product not found during delete confirmation. Product ID: {ProductId} at {Time}", id, DateTime.Now);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting Product ID: {ProductId} at {Time}", id, DateTime.Now);
                return RedirectToAction("Error", new { statusCode = 500 });
            }
        }
    }
}
