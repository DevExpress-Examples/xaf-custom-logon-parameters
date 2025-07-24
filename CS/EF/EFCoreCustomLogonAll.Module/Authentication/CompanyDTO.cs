#nullable enable
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;

namespace EFCoreCustomLogonAll.Module.Authentication;

[DomainComponent]
public class CompanyDTO {
    IList<ApplicationUserDTO>? users;

    public virtual string? Name { get; set; }

    [DevExpress.ExpressApp.Data.Key]
    [VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
    public virtual Guid ID { get; set; }

    public IList<ApplicationUserDTO>? ApplicationUsers {
        get {
            if(users == null && LogonDataProvider != null) {
                users = LogonDataProvider.GetCompanyUsers(ID);
            }
            return users;
        }
    }
    public ILogonDataProvider? LogonDataProvider { get; set; }
}

#nullable restore