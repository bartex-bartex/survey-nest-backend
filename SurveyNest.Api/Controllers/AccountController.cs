using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SurveyNest.Api.Models;
using SurveyNest.Api.DTO;
using SurveyNest.Api.Constants;

namespace SurveyNest.Api.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    private readonly ILogger<AccountController> _logger;

    private readonly IConfiguration _configuration;

    // for user Registration
    private readonly UserManager<ApiUser> _userManager;

    // for user Login
    private readonly SignInManager<ApiUser> _signInManager;

    public AccountController(
        ApplicationDbContext context,
        ILogger<AccountController> logger,
        IConfiguration configuration,
        UserManager<ApiUser> userManager,
        SignInManager<ApiUser> signInManager)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost]
    public async Task<ActionResult> Register(RegisterDTO input)
    {
        if (!ModelState.IsValid)
        {
            var details = new ValidationProblemDetails(ModelState);
            details.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            details.Status = StatusCodes.Status400BadRequest;
            return new BadRequestObjectResult(details);
        }


        try
        {
            var newUser = new ApiUser
            {
                UserName = input.UserName,
                Email = input.Email
            };

            var result = await _userManager.CreateAsync(newUser, input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserName} ({Email}) has been created", newUser.UserName, newUser.Email);

                await _userManager.AddToRoleAsync(newUser, RoleNames.User);

                return StatusCode(StatusCodes.Status201Created, $"User {newUser.UserName} has been created");
            }
            else
            {
                throw new Exception(
                string.Format("Error: {0}", string.Join(" ",
                    result.Errors.Select(e => e.Description))));
            }
        }
        catch (Exception e)
        {
            var exceptionDetails = new ProblemDetails();
            exceptionDetails.Detail = e.Message;
            exceptionDetails.Status = StatusCodes.Status500InternalServerError;
            exceptionDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
        }
    }


    [HttpPost]
    public async Task<ActionResult> Login(LoginDTO input)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(input.UserName);
                if (user == null
                    || !await _userManager.CheckPasswordAsync(
                           user, input.Password))
                    throw new Exception("Invalid login attempt.");
                else
                {
                    // stores encrypted JWT signature
                    var signingCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(
                                System.Text.Encoding.UTF8.GetBytes(
                                    _configuration["JWT:SigningKey"])),
                            SecurityAlgorithms.HmacSha256);

                    // claim = part of info about user included in JWT
                    var claims = new List<Claim>();
                    claims.Add(new Claim(
                        ClaimTypes.Name, user.UserName));
                    claims.AddRange((await _userManager.GetRolesAsync(user)).Select(role => new Claim(ClaimTypes.Role, role)));

                    // object represented jwt token
                    var jwtObject = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                        audience: _configuration["JWT:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddSeconds(300),
                        signingCredentials: signingCredentials);

                    // object to string
                    var jwtString = new JwtSecurityTokenHandler()
                .WriteToken(jwtObject);

                    // return
                    return StatusCode(
                StatusCodes.Status200OK, jwtString);
                }
            }
            else
            {
                var details = new ValidationProblemDetails(ModelState);
                details.Type =
                        "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                details.Status = StatusCodes.Status400BadRequest;
                return new BadRequestObjectResult(details);
            }
        }
        catch (Exception e)
        {
            var exceptionDetails = new ProblemDetails();
            exceptionDetails.Detail = e.Message;
            exceptionDetails.Status =
                StatusCodes.Status401Unauthorized;
            exceptionDetails.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            return StatusCode(
                StatusCodes.Status401Unauthorized,
                exceptionDetails);
        }
    }
}