using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Win;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Win.Utils;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using Microsoft.EntityFrameworkCore;
using DevExpress.ExpressApp.EFCore;
using DevExpress.EntityFrameworkCore.Security;
using EFCoreCustomLogonAll.Module;
using EFCoreCustomLogonAll.Module.BusinessObjects;
using System.Data.Common;

namespace EFCoreCustomLogonAll.Win;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Win.WinApplication._members
public class EFCoreCustomLogonAllWindowsFormsApplication : WinApplication {
    public EFCoreCustomLogonAllWindowsFormsApplication() {
		SplashScreen = new DXSplashScreen(typeof(XafSplashScreen), new DefaultOverlayFormOptions());
        ApplicationName = "EFCoreCustomLogonAll";
        CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
        UseOldTemplates = false;
        DatabaseVersionMismatch += EFCoreCustomLogonAllWindowsFormsApplication_DatabaseVersionMismatch;
        CustomizeLanguagesList += EFCoreCustomLogonAllWindowsFormsApplication_CustomizeLanguagesList;
        this.CreateCustomLogonWindowObjectSpace += application_CreateCustomLogonWindowObjectSpace;
    }
    private void application_CreateCustomLogonWindowObjectSpace(object sender, CreateCustomLogonWindowObjectSpaceEventArgs e) {
        e.ObjectSpace = CreateObjectSpace(typeof(CustomLogonParameters));
        NonPersistentObjectSpace nonPersistentObjectSpace = e.ObjectSpace as NonPersistentObjectSpace;
        if(nonPersistentObjectSpace != null) {
            if(!nonPersistentObjectSpace.IsKnownType(typeof(Company), true)) {
                IObjectSpace additionalObjectSpace = CreateObjectSpace(typeof(Company));
                nonPersistentObjectSpace.AdditionalObjectSpaces.Add(additionalObjectSpace);
                nonPersistentObjectSpace.Disposed += (s2, e2) => {
                    additionalObjectSpace.Dispose();
                };
            }
        }
    	((CustomLogonParameters)e.LogonParameters).RefreshPersistentObjects(e.ObjectSpace);
    }
    private void EFCoreCustomLogonAllWindowsFormsApplication_CustomizeLanguagesList(object sender, CustomizeLanguagesListEventArgs e) {
        string userLanguageName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
        if(userLanguageName != "en-US" && e.Languages.IndexOf(userLanguageName) == -1) {
            e.Languages.Add(userLanguageName);
        }
    }
    private void EFCoreCustomLogonAllWindowsFormsApplication_DatabaseVersionMismatch(object sender, DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs e) {
#if EASYTEST
        e.Updater.Update();
        e.Handled = true;
#else
        if(System.Diagnostics.Debugger.IsAttached) {
            e.Updater.Update();
            e.Handled = true;
        }
        else {
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
