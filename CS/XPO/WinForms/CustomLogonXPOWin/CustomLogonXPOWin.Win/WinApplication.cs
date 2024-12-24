using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Win;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Win.Utils;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using CustomLogonXPOWin.Module.BusinessObjects;
using CustomLogonXPOWin.Module.Security;

namespace CustomLogonXPOWin.Win;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Win.WinApplication._members
public class CustomLogonXPOWinWindowsFormsApplication : WinApplication {
    public CustomLogonXPOWinWindowsFormsApplication() {
        SplashScreen = new DXSplashScreen(typeof(XafSplashScreen), new DefaultOverlayFormOptions());
        ApplicationName = "CustomLogonXPOWin";
        CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
        UseOldTemplates = false;
        DatabaseVersionMismatch += CustomLogonXPOWinWindowsFormsApplication_DatabaseVersionMismatch;
        CustomizeLanguagesList += CustomLogonXPOWinWindowsFormsApplication_CustomizeLanguagesList;
        this.CreateCustomLogonWindowObjectSpace += application_CreateCustomLogonWindowObjectSpace;
    }
    private static void application_CreateCustomLogonWindowObjectSpace(object sender, CreateCustomLogonWindowObjectSpaceEventArgs e) {
        var application = (XafApplication)sender;
        e.ObjectSpace = application.CreateObjectSpace(typeof(CustomLogonParameters));
        CompositeObjectSpace nonPersistentObjectSpace = e.ObjectSpace as CompositeObjectSpace;
        if(nonPersistentObjectSpace != null) {
            nonPersistentObjectSpace.PopulateAdditionalObjectSpaces(application);
        }
        ((CustomLogonParameters)e.LogonParameters).RefreshPersistentObjects(e.ObjectSpace);
    }
    private void CustomLogonXPOWinWindowsFormsApplication_CustomizeLanguagesList(object sender, CustomizeLanguagesListEventArgs e) {
        string userLanguageName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
        if(userLanguageName != "en-US" && e.Languages.IndexOf(userLanguageName) == -1) {
            e.Languages.Add(userLanguageName);
        }
    }
    private void CustomLogonXPOWinWindowsFormsApplication_DatabaseVersionMismatch(object sender, DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs e) {
#if EASYTEST
        e.Updater.Update();
        e.Handled = true;
#else
        if(System.Diagnostics.Debugger.IsAttached) {
            e.Updater.Update();
            e.Handled = true;
        } else {
            string message = "The application cannot connect to the specified database, " +
                "because the database doesn't exist, its version is older " +
                "than that of the application or its schema does not match " +
                "the ORM data model structure. To avoid this error, use one " +
                "of the solutions from the https://www.devexpress.com/kb=T367835 KB Article.";

            if(e.CompatibilityError != null && e.CompatibilityError.Exception != null) {
                message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
            }
            throw new InvalidOperationException(message);
        }
#endif
    }
}
