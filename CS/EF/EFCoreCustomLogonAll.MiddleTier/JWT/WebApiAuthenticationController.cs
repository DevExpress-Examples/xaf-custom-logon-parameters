using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using EFCoreCustomLogonAll.Module.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCustomLogonAll.WebApi.JWT;

[ApiController]
[Route("api/[controller]")]
// This is a JWT authentication service sample.
public class AuthenticationController : ControllerBase {
    readonly IAuthenticationTokenProvider tokenProvider;
    public AuthenticationController(IAuthenticationTokenProvider tokenProvider) {
        this.tokenProvider = tokenProvider;
    }
    [HttpPost("Authenticate")]
    public IActionResult Authenticate(
        [FromBody]
        CustomLogonParameters logonParameters
    ) {
        try {
            return Ok(tokenProvider.Authenticate(logonParameters));
        } catch(AuthenticationException ex) {
            return Unauthorized(ex.GetJson());
        }
    }
}
