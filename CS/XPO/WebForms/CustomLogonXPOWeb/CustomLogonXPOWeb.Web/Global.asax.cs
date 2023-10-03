using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using System.Web.Routing;

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Web;
using DevExpress.Web;
using CustomLogonXPOWeb.Module.BusinessObjects;
using CustomLogonXPOWeb.Module.Security;

namespace CustomLogonXPOWeb.Web {
    public class Global : System.Web.HttpApplication {
        public Global() {
            InitializeComponent();
        }
        protected void Application_Start(Object sender, EventArgs e) {
            DevExpress.ExpressApp.FrameworkSettings.DefaultSettingsCompatibilityMode = DevExpress.ExpressApp.FrameworkSettingsCompatibilityMode.Latest;
            RouteTable.Routes.RegisterXafRoutes();
            ASPxWebControl.CallbackError += new EventHandler(Application_Error);
#if EASYTEST
            DevExpress.ExpressApp.Web.TestScripts.TestScriptsManager.EasyTestEnabled = true;
#endif
        }
        protected void Session_Start(Object sender, EventArgs e) {
            Tracing.Initialize();
            WebApplication.SetInstance(Session, new CustomLogonXPOWebAspNetApplication());
            SecurityStrategy security = WebApplication.Instance.GetSecurityStrategy();
            security.RegisterXPOAdapterProviders();
            DevExpress.ExpressApp.Web.Templates.DefaultVerticalTemplateContentNew.ClearSizeLimit();
            WebApplication.Instance.SwitchToNewStyle();
            if(ConfigurationManager.ConnectionStrings["ConnectionString"] != null) {
                WebApplication.Instance.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            }
#if EASYTEST
            if(ConfigurationManager.ConnectionStrings["EasyTestConnectionString"] != null) {
                WebApplication.Instance.ConnectionString = ConfigurationManager.ConnectionStrings["EasyTestConnectionString"].ConnectionString;
            }
#endif
#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached && WebApplication.Instance.CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
                WebApplication.Instance.DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
            }
#endif
            WebApplication.Instance.CreateCustomLogonWindowObjectSpace +=
       application_CreateCustomLogonWindowObjectSpace;
            WebApplication.Instance.Setup();
            WebApplication.Instance.Start();
        }
        private static void application_CreateCustomLogonWindowObjectSpace(object sender, CreateCustomLogonWindowObjectSpaceEventArgs e) {
            e.ObjectSpace = ((XafApplication)sender).CreateObjectSpace(typeof(CustomLogonParameters));
            NonPersistentObjectSpace nonPersistentObjectSpace = e.ObjectSpace as NonPersistentObjectSpace;
            if (nonPersistentObjectSpace != null) {
                if (!nonPersistentObjectSpace.IsKnownType(typeof(Company), true)) {
                    IObjectSpace additionalObjectSpace = ((XafApplication)sender).CreateObjectSpace(typeof(Company));
                    nonPersistentObjectSpace.AdditionalObjectSpaces.Add(additionalObjectSpace);
                    nonPersistentObjectSpace.Disposed += (s2, e2) => {
                        additionalObjectSpace.Dispose();
                    };
                }
            }
            ((CustomLogonParameters)e.LogonParameters).RefreshPersistentObjects(e.ObjectSpace);
        }
        protected void Application_BeginRequest(Object sender, EventArgs e) {
        }
        protected void Application_EndRequest(Object sender, EventArgs e) {
        }
        protected void Application_AuthenticateRequest(Object sender, EventArgs e) {
        }
        protected void Application_Error(Object sender, EventArgs e) {
            ErrorHandling.Instance.ProcessApplicationError();
        }
        protected void Session_End(Object sender, EventArgs e) {
            WebApplication.LogOff(Session);
            WebApplication.DisposeInstance(Session);
        }
        protected void Application_End(Object sender, EventArgs e) {
        }
        #region Web Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
        }
        #endregion
    }
}
