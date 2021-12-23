using System.Linq;
using System.Threading.Tasks;
using BitFleet.Contracts.V1;
using BitFleet.Contracts.V1.Requests;
using BitFleet.Contracts.V1.Responses.AuthenticationResponses;
using BitFleet.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BitFleet.Controllers.V1
{
    public class IdentityController : Controller
    {
        private readonly IIdentityService _identityService;
        
        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost(ApiRoutes.Identity.Register)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage))
                });
            }
            
            var authResponse = await _identityService.RegisterAsync(request.Username, request.Password, request.FirstName, request.LastName, request.Email, request.Role);

            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authResponse.Errors
                });
            }
            
            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token
            });
        }
        
        [HttpPost(ApiRoutes.Identity.Login)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var authResponse = await _identityService
                .LoginAsync(request.Username, request.Password);

            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authResponse.Errors
                });
            }

            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token
            });
        }
    }
}