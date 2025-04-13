using invmanager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;

namespace invmanager.Controllers
{
    public class UserRolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserRolesController> _logger;

        // Constructor to inject dependencies
        public UserRolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserRolesController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: UserRoles/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = _userManager.Users.ToList();
                var userRolesViewModelList = new List<UserRolesViewModel>();

                foreach (var user in users)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var model = new UserRolesViewModel
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Roles = userRoles.ToList(),
                        AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()
                    };

                    userRolesViewModelList.Add(model);
                }

                _logger.LogInformation("Fetched {UserCount} users and their roles successfully.", users.Count);

                return View(userRolesViewModelList);
            }
            catch (System.Exception ex)
            {
                // Log the exception with a relevant message
                _logger.LogError(ex, "An error occurred while fetching users and their roles.");
                return StatusCode(500, "Internal server error while retrieving users and roles.");
            }
        }

        // GET: UserRoles/ManageRole/{userId}
        public async Task<IActionResult> ManageRole(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", userId);
                    return NotFound();
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

                var model = new UserRolesViewModel
                {
                    UserId = userId,
                    UserName = user.UserName,
                    Roles = userRoles.ToList(),
                    AvailableRoles = allRoles,
                    SelectedRoles = userRoles.ToList() // Pre-select the current roles
                };

                _logger.LogInformation("Managing roles for user {UserName} ({UserId}).", user.UserName, userId);

                return View(model);
            }
            catch (System.Exception ex)
            {
                // Log the exception with user details
                _logger.LogError(ex, "An error occurred while managing roles for user {UserId}.", userId);
                return StatusCode(500, "Internal server error while managing roles.");
            }
        }

        // POST: UserRoles/ManageRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRole(UserRolesViewModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", model.UserId);
                    return NotFound();
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                var selectedRoles = model.SelectedRoles ?? new List<string>();

                // Remove roles that are unchecked
                var rolesToRemove = currentRoles.Except(selectedRoles);
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                // Add new roles that were checked
                var rolesToAdd = selectedRoles.Except(currentRoles);
                await _userManager.AddToRolesAsync(user, rolesToAdd);

                _logger.LogInformation("Updated roles for user {UserName} ({UserId}). Roles added: {RolesToAdd}, Roles removed: {RolesToRemove}",
                    user.UserName, model.UserId, string.Join(",", rolesToAdd), string.Join(",", rolesToRemove));

                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                // Log the exception with user details
                _logger.LogError(ex, "An error occurred while updating roles for user {UserId}.", model.UserId);
                return StatusCode(500, "Internal server error while updating roles.");
            }
        }

        // POST: UserRoles/RemoveRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || string.IsNullOrEmpty(role))
                {
                    _logger.LogWarning("User with ID {UserId} or role {Role} not found.", userId, role);
                    return NotFound();
                }

                if (await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                    _logger.LogInformation("Removed role {Role} from user {UserName} ({UserId}).", role, user.UserName, userId);
                }

                return RedirectToAction("ManageRole", new { userId });
            }
            catch (System.Exception ex)
            {
                // Log the exception with user details
                _logger.LogError(ex, "An error occurred while removing role {Role} from user {UserId}.", role, userId);
                return StatusCode(500, "Internal server error while removing role.");
            }
        }
    }
}
