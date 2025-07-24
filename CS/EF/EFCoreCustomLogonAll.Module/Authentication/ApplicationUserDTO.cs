#nullable enable
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;

namespace EFCoreCustomLogonAll.Module.Authentication;

[DomainComponent]
public class ApplicationUserDTO {
    public string? UserName { get; set; }

    [DevExpress.ExpressApp.Data.Key]
    [VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
    public virtual Guid ID { get; set; }

    [VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
    public virtual Guid CompanyID { get; set; }
}

#nullable restore