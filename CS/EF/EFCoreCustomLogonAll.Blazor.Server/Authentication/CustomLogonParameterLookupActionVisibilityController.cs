using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using EFCoreCustomLogonAll.Module.Authentication;

namespace EFCoreCustomLogonAll.Blazor.Server.Authentication {
    // https://supportcenter.devexpress.com/ticket/details/t1164456/blazor-lookup-list-view-s-allowedit-property-does-not-affect-visibility-of
    public class CustomLogonParameterLookupActionVisibilityController : ObjectViewController<DetailView, CustomLogonParameters> {
        protected override void OnActivated() {
            base.OnActivated();
            View.CustomizeViewItemControl<LookupPropertyEditor>(this, e => {
                e.HideEditButton();
            });
        }
    }
}
