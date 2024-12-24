using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace EFCoreCustomLogonAll.Module.BusinessObjects;

[DomainComponent]
[DisplayName("Log In")]
[Appearance("CompanyIsNull", TargetItems = $"{nameof(Password)}, {nameof(ApplicationUser)}", Criteria = "IsNull([Company])", Enabled = false)]
public class CustomLogonParameters : IAuthenticationStandardLogonParameters, ISupportClearPassword, INotifyPropertyChanged, IObjectSpaceLink {
    private Company company;
    private ApplicationUser applicationUser;
    private string password;

    [JsonIgnore]
    [ImmediatePostData]
    public Company Company {
        get { return company; }
        set {
            if(value == company)
                return;
            company = value;
            if(ApplicationUser?.Company != company) {
                ApplicationUser = null;
            }
            OnPropertyChanged(nameof(Company));
        }
    }

    [JsonIgnore]
    [DataSourceProperty($"{nameof(Company)}.{nameof(Company.ApplicationUsers)}"), ImmediatePostData]
    public ApplicationUser ApplicationUser {
        get { return applicationUser; }
        set {
            if(value == applicationUser)
                return;
            applicationUser = value;
            Company = applicationUser?.Company;
            UserName = applicationUser?.UserName;
            OnPropertyChanged(nameof(ApplicationUser));
        }
    }

    [Browsable(false)]
    public string UserName { get; set; }

    [PasswordPropertyText(true)]
    public string Password {
        get { return password; }
        set {
            if(password == value)
                return;
            password = value;
            OnPropertyChanged(nameof(Password));
        }
    }
    private void OnPropertyChanged(string propertyName) {
        if(PropertyChanged != null) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void ClearPassword() {
        password = null;
    }

    private IObjectSpace objectSpace;
    IObjectSpace IObjectSpaceLink.ObjectSpace {
        get => objectSpace;
        set => objectSpace = value;
    }
}