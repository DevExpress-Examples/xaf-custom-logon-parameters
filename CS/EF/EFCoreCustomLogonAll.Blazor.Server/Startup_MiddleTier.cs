using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.Security;
using EFCoreCustomLogonAll.Blazor.Server.Authentication;
using EFCoreCustomLogonAll.Blazor.Server.Services;
using EFCoreCustomLogonAll.Module.Authentication;
using EFCoreCustomLogonAll.Module.BusinessObjects;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace EFCoreCustomLogonAll.Blazor.Server;

public class Startup {
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services) {
        services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

        //services.AddScoped<ILogonDataProvider, BlazorLogonDataProvider>();
        services.AddScoped<ILogonDataProvider, MiddleTierClientLogonDataProvider>();

        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddScoped<CircuitHandler, CircuitHandlerProxy>();
        services.AddXaf(Configuration, builder => {
            builder.UseApplication<EFCoreCustomLogonAllBlazorApplication>();
            builder.Modules
                .AddConditionalAppearance()
                .AddValidation(options => {
                    options.AllowValidationDetailsAccess = false;
                })
                .Add<EFCoreCustomLogonAll.Module.EFCoreCustomLogonAllModule>()
                .Add<EFCoreCustomLogonAllBlazorModule>();

            builder.ObjectSpaceProviders
                .AddEFCore(options => options.PreFetchReferenceProperties())
                    .WithDbContext<EFCoreCustomLogonAllEFCoreDbContext>((IServiceProvider serviceProvider, DbContextOptionsBuilder options) => {
                        options.UseMiddleTier(serviceProvider.GetRequiredService<ISecurityStrategyBase>());
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
        });
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => {
                options.LoginPath = "/LoginPage";
            })
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters() {
                    ValidIssuer = Configuration["Authentication:Jwt:ValidIssuer"],
                    ValidAudience = Configuration["Authentication:Jwt:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:Jwt:IssuerSigningKey"])),
                    AuthenticationType = JwtBearerDefaults.AuthenticationScheme
                };
            });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        if(env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        } else {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseRequestLocalization();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseXaf();
        app.UseEndpoints(endpoints => {
            endpoints.MapXafEndpoints();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapControllers();
        });
    }
}
