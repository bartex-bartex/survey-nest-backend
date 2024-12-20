using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SurveyNest.Api.Models;
using SurveyNest.Api.Constants;

namespace SurveyNest.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SeedController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApiUser> _userManager;

    public SeedController(ApplicationDbContext context, ILogger<SeedController> logger, IWebHostEnvironment env, RoleManager<IdentityRole> roleManager, UserManager<ApiUser> userManager)
    {
        _context = context;
        _logger = logger;
        _env = env;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpPut]
    public async Task<IActionResult> AuthData()
    {
        int rolesCreated = 0;
        int usersAddedToRoles = 0;

        if (!await _roleManager.RoleExistsAsync(RoleNames.Moderator))
        {
            await _roleManager.CreateAsync(
                new IdentityRole(RoleNames.Moderator));
            rolesCreated++;
        }
        if (!await _roleManager.RoleExistsAsync(RoleNames.Administrator))
        {
            await _roleManager.CreateAsync(
                new IdentityRole(RoleNames.Administrator));
            rolesCreated++;
        }
        if (!await _roleManager.RoleExistsAsync(RoleNames.User))
        {
            await _roleManager.CreateAsync(
                new IdentityRole(RoleNames.User));
            rolesCreated++;
        }
        //if (!await _roleManager.RoleExistsAsync(RoleNames.Guest))
        //{
        //    await _roleManager.CreateAsync(
        //        new IdentityRole(RoleNames.Guest));
        //    rolesCreated++;
        //}

        // TODO: By default they will have the User role -> should I remove it or not?
        var testModerator = await _userManager
            .FindByNameAsync("TestModerator");
        if (testModerator != null
            && !await _userManager.IsInRoleAsync(
                testModerator, RoleNames.Moderator))
        {
            await _userManager.AddToRoleAsync(testModerator, RoleNames.Moderator);
            usersAddedToRoles++;
        }

        var testAdministrator = await _userManager
            .FindByNameAsync("TestAdministrator");
        if (testAdministrator != null
            && !await _userManager.IsInRoleAsync(
                testAdministrator, RoleNames.Administrator))
        {
            await _userManager.AddToRoleAsync(
                testAdministrator, RoleNames.Moderator);
            await _userManager.AddToRoleAsync(
                testAdministrator, RoleNames.Administrator);
            usersAddedToRoles++;
        }

        return new JsonResult(new
        {
            RolesCreated = rolesCreated,
            UsersAddedToRoles = usersAddedToRoles
        });
    }
}
