using DevExpress.ExpressApp.Core;

namespace EFCoreCustomLogonAll.Blazor.Server.Security;

static class ApplicationBuilderExtensions {
    //public static IApplicationBuilder CreateDB(this IApplicationBuilder app)
    //{
    //    using var scope = app.ApplicationServices.CreateScope();
    //    //Update DB schema
    //    scope.ServiceProvider.GetRequiredService<IObjectSpaceProviderFactory>()
    //        .CreateObjectSpaceProviders().ToList().ForEach(p => {
    //            if (!(p is DevExpress.ExpressApp.NonPersistentObjectSpaceProvider))
    //            {
    //                p.UpdateSchema();
    //            }
    //        });
    //    return app;
    //}
    public static IApplicationBuilder UseDemoData(this IApplicationBuilder app) {
        using var scope = app.ApplicationServices.CreateScope();

        var updatingObjectSpaceFactory = scope.ServiceProvider.GetRequiredService<IUpdatingObjectSpaceFactory>();
        using var objectSpace = updatingObjectSpaceFactory.CreateUpdatingObjectSpace(typeof(Module.BusinessObjects.ApplicationUser), true);
        new Module.DatabaseUpdate.Updater(objectSpace, new Version()).UpdateDatabaseAfterUpdateSchema();

        return app;
    }
}
