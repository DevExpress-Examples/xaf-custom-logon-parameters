using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.ExceptionServices;

namespace EFCoreCustomLogonAll.WebApi.JWT;

public class JwtTokenProviderService : IAuthenticationTokenProvider {
    readonly SignInManager signInManager;
    readonly JwtBearerOptions options;
    public JwtTokenProviderService(SignInManager signInManager, IOptionsMonitor<JwtBearerOptions> optionsMonitor) {
        this.signInManager = signInManager;
        options = optionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);
    }
    public string Authenticate(object logonParameters) {
        var result = signInManager.AuthenticateByLogonParameters(logonParameters);
        if(result.Succeeded) {
            var issuerSigningKey = options.TokenValidationParameters.IssuerSigningKey;
            var token = new JwtSecurityToken(
                issuer: options.TokenValidationParameters.ValidIssuer,
                audience: options.TokenValidationParameters.ValidAudience,
                claims: result.Principal.Claims,
                expires: DateTime.Now.AddDays(2),
                signingCredentials: new SigningCredentials(issuerSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        if(result.Error is IUserFriendlyException) {
            ExceptionDispatchInfo.Throw(result.Error);
        }
        throw new AuthenticationException("Internal server error");
    }
}
