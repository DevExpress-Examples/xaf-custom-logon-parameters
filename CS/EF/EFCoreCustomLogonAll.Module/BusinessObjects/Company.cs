using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using EFCoreCustomLogonAll.Module.BusinessObjects;
using System.Collections.ObjectModel;

namespace EFCoreCustomLogonAll.Module.BusinessObjects;

[DefaultClassOptions]
public class Company : BaseObject {
    public virtual string Name { get; set; }
    public virtual IList<ApplicationUser> ApplicationUsers { get; set; } = new ObservableCollection<ApplicationUser>();
}
