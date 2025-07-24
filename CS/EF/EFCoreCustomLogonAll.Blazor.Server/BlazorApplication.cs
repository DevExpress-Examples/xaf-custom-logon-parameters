using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor;
using EFCoreCustomLogonAll.Blazor.Server.Authentication;

namespace EFCoreCustomLogonAll.Blazor.Server;

public class EFCoreCustomLogonAllBlazorApplication : BlazorApplication {
    public EFCoreCustomLogonAllBlazorApplication() {
        ApplicationName = "EFCoreCustomLogonAll";
    }
    protected override List<Controller> CreateLogonWindowControllers() {
        var result = base.CreateLogonWindowControllers();
        result.Add(CreateController<CustomLogonParameterLookupActionVisibilityController>());
        return result;
    }
}
