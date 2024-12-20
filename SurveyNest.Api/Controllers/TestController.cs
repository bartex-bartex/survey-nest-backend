using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyNest.Api.Constants;

namespace SurveyNest.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = RoleNames.Administrator)]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }
    }
}
