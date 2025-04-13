using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using invmanager.Models;
using Microsoft.EntityFrameworkCore; // Assuming ApplicationUser is in this namespace

public class RoleManagerController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleManagerController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    // GET: RoleManager
    public async Task<IActionResult> Index()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return View(roles);
    }

    // POST: Add a new role
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRole(string roleName)
    {
        if (!string.IsNullOrEmpty(roleName))
        {
            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                var newRole = new IdentityRole(roleName);
                await _roleManager.CreateAsync(newRole);
            }
        }
        return RedirectToAction("Index");
    }
}