using DevExpress.ExpressApp.Security.ClientServer.WebApi;
using DevExpress.Persistent.Base;
using DevExpress.XtraEditors;
using EFCoreCustomLogonAll.Module.Authentication;

namespace EFCoreCustomLogonAll.Win;

static class Program {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    public static int Main(string[] args) {
        // TODO add description! IMPORTANT
        WebApiDataServerHelper.AddKnownType(typeof(CustomLogonParameters));

        DevExpress.ExpressApp.FrameworkSettings.DefaultSettingsCompatibilityMode = DevExpress.ExpressApp.FrameworkSettingsCompatibilityMode.Latest;
        DevExpress.ExpressApp.Security.SecurityStrategy.AutoAssociationReferencePropertyMode = DevExpress.ExpressApp.Security.ReferenceWithoutAssociationPermissionsMode.AllMembers;
#if EASYTEST
        DevExpress.ExpressApp.Win.EasyTest.EasyTestRemotingRegistration.Register();
#endif
        WindowsFormsSettings.LoadApplicationSettings();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        DevExpress.Utils.ToolTipController.DefaultController.ToolTipType = DevExpress.Utils.ToolTipType.SuperTip;
        if(Tracing.GetFileLocationFromSettings() == DevExpress.Persistent.Base.FileLocation.CurrentUserApplicationDataFolder) {
            Tracing.LocalUserAppDataPath = Application.LocalUserAppDataPath;
        }
        Tracing.Initialize();

        var winApplication = ApplicationBuilder.BuildApplication();

        try {
            winApplication.Setup();
            winApplication.Start();
        } catch(Exception e) {
            winApplication.StopSplash();
            winApplication.HandleException(e);
        }
        return 0;
    }
}
