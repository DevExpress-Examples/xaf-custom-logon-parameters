using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.Win;
using DevExpress.ExpressApp.Win.ApplicationBuilder;
using EFCoreCustomLogonAll.Module.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EFCoreCustomLogonAll.Win;

public class ApplicationBuilder : IDesignTimeApplicationFactory {
    public static WinApplication BuildApplication() {
        var builder = WinApplication.CreateBuilder();
        // Register custom services for Dependency Injection. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/404430/
        // builder.Services.AddScoped<CustomService>();
        // Register 3rd-party IoC containers (like Autofac, Dryloc, etc.)
        // builder.UseServiceProviderFactory(new DryIocServiceProviderFactory());
        // builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        builder.Services.AddScoped<ILogonDataProvider, MiddleTierClientLogonDataProvider>();

        builder.UseApplication<EFCoreCustomLogonAllWindowsFormsApplication>();
        builder.Modules
            .AddConditionalAppearance()
            .AddValidation(options => {
                options.AllowValidationDetailsAccess = false;
            })
            .Add<EFCoreCustomLogonAll.Module.EFCoreCustomLogonAllModule>()
            .Add<EFCoreCustomLogonAllWinModule>();
        builder.ObjectSpaceProviders
            .AddEFCore(options => options.PreFetchReferenceProperties())
                .WithDbContext<EFCoreCustomLogonAll.Module.BusinessObjects.EFCoreCustomLogonAllEFCoreDbContext>((application, options) => {
                    options.UseMiddleTier(application.Security);
                    options.UseChangeTrackingProxies();
                    options.UseObjectSpaceLinkProxies();
                })
            .AddNonPersistent();
        builder.Security
            .UseMiddleTierMode(options => {
#if DEBUG
                options.WaitForMiddleTierServerReady();
#endif
                options.BaseAddress = new Uri("https://localhost:44318/");
                options.Events.OnHttpClientCreated = client => client.DefaultRequestHeaders.Add("Accept", "application/json");
                options.Events.OnCustomAuthenticate = (sender, security, args) => {
                    args.Handled = true;
                    HttpResponseMessage msg = args.HttpClient.PostAsJsonAsync("api/Authentication/Authenticate", (CustomLogonParameters)args.LogonParameters).GetAwaiter().GetResult();
                    string token = (string)msg.Content.ReadFromJsonAsync(typeof(string)).GetAwaiter().GetResult();
                    if(msg.StatusCode == HttpStatusCode.Unauthorized) {
                        XafExceptions.Authentication.ThrowAuthenticationFailedFromResponse(token);
                    }
                    msg.EnsureSuccessStatusCode();
                    args.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                };
            })
            .AddPasswordAuthentication();
        builder.AddBuildStep(application => {
            application.DatabaseUpdateMode = DatabaseUpdateMode.Never;
        });
        var winApplication = builder.Build();
        return winApplication;
    }

    XafApplication IDesignTimeApplicationFactory.Create() {
        DevExpress.EntityFrameworkCore.Security.MiddleTier.ClientServer.MiddleTierClientSecurity.DesignModeUserType = typeof(EFCoreCustomLogonAll.Module.BusinessObjects.ApplicationUser);
        DevExpress.EntityFrameworkCore.Security.MiddleTier.ClientServer.MiddleTierClientSecurity.DesignModeRoleType = typeof(DevExpress.Persistent.BaseImpl.EF.PermissionPolicy.PermissionPolicyRole);
        return BuildApplication();
    }
}
