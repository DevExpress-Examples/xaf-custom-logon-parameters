using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base.Security;
using EFCoreCustomLogonAll.Module.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreCustomLogonAll.Module.Security;

public class CustomAuthentication : AuthenticationBase, IAuthenticationStandard {
    private CustomLogonParameters customLogonParameters;
    public CustomAuthentication() {
        customLogonParameters = new CustomLogonParameters();
    }

    public override void Logoff() {
        base.Logoff();
        customLogonParameters = new CustomLogonParameters();
    }

    public override void ClearSecuredLogonParameters() {
        customLogonParameters.Password = "";
        base.ClearSecuredLogonParameters();
    }

    public override object Authenticate(IObjectSpace objectSpace) {
        ApplicationUser applicationUser = objectSpace.FirstOrDefault<ApplicationUser>(e => e.UserName == customLogonParameters.UserName);

        if(applicationUser == null)
            throw new ArgumentNullException("applicationUser");

        var userLockoutService = objectSpace.ServiceProvider.GetRequiredService<IUserLockout>();
        if(userLockoutService.IsLockedOut(applicationUser)) {
            ISecurityUserLockout userLockout = (ISecurityUserLockout)applicationUser;
            throw new AuthenticationException(customLogonParameters.UserName, SecurityExceptionLocalizer.GetExceptionMessage(SecurityExceptionId.UserLockout, Math.Floor((userLockout.LockoutEnd - DateTime.UtcNow).TotalSeconds)));
        }

        if(!((IAuthenticationStandardUser)applicationUser).ComparePassword(customLogonParameters.Password)) {
            userLockoutService.AccessFailed(applicationUser, objectSpace);
            throw new AuthenticationException(
                applicationUser.UserName, "Password mismatch.");
        }

        if(!applicationUser.IsActive) {
            throw new AuthenticationException(applicationUser.UserName, SecurityExceptionLocalizer.GetExceptionMessage(SecurityExceptionId.UserIsNotActive));
        }

        if(!ValidateSecurityUser(applicationUser)) {
            throw new AuthenticationException(applicationUser.UserName);

        }

        userLockoutService.ResetLockout(applicationUser, objectSpace);
        return applicationUser;
    }

    public override void SetLogonParameters(object logonParameters) {
        customLogonParameters = (CustomLogonParameters)logonParameters;
    }

    public override IList<Type> GetBusinessClasses() {
        return new Type[] { typeof(CustomLogonParameters) };
    }

    public override bool AskLogonParametersViaUI {
        get { return true; }
    }

    public override object LogonParameters {
        get { return customLogonParameters; }
    }

    public override bool IsLogoffEnabled {
        get { return true; }
    }
}