using DevExpress.ExpressApp.Security;
using Microsoft.Extensions.Options;

namespace EFCustomLogon.Module.BusinessObjects;

public class CustomAuthenticationStandardProvider : AuthenticationStandardProviderV2 {
    public CustomAuthenticationStandardProvider(IOptions<AuthenticationStandardProviderOptions> options,
    IOptions<SecurityOptions> securityOptions) :
        base(options, securityOptions) { }
    protected override AuthenticationBase CreateAuthentication(Type userType, Type logonParametersType) {
        return new CustomAuthentication();
    }
}
