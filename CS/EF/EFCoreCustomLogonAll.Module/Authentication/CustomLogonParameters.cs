#nullable enable
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace EFCoreCustomLogonAll.Module.Authentication;

[DataContract]
[DomainComponent]
[DisplayName("Log In")]
[Appearance("CompanyIsNull", TargetItems = $"{nameof(Password)}, {nameof(ApplicationUser)}", Criteria = "IsNull([Company])", Enabled = false)]
public class CustomLogonParameters : IAuthenticationStandardLogonParameters, ISupportClearPassword, INotifyPropertyChanged, IObjectSpaceLink {
    private CompanyDTO? company;
    private ApplicationUserDTO? applicationUser;
    private string? password;
    private IReadOnlyList<CompanyDTO>? _companies = null;
    private IObjectSpace? objectSpace;
    private ILogonDataProvider? logonDataProvider;

    [Browsable(false)]
    public string? UserName { get; set; }

    [JsonIgnore]
    [ImmediatePostData]
    [DataSourceProperty("Companies", DataSourcePropertyIsNullMode.SelectAll)]
    [IgnoreDataMember]
    public CompanyDTO? Company {
        get { return company; }
        set {
            if(value == company) return;
            company = value;
            if(company == null || ApplicationUser?.CompanyID != company.ID) {
                ApplicationUser = null;
            }
            OnPropertyChanged(nameof(CompanyDTO));
        }
    }
    [Browsable(false)] // hide from UI
    [JsonIgnore]
    [IgnoreDataMember]
    public IReadOnlyList<CompanyDTO>? Companies {
        get {
            if(_companies == null) {
                if(logonDataProvider != null) {
                    _companies = logonDataProvider.GetCompanies().AsReadOnly();
                } else {
                    _companies = Array.Empty<CompanyDTO>();
                }
            }
            return _companies;
        }
    }

    [JsonIgnore]
    [DataSourceProperty($"{nameof(Company)}.{nameof(Company.ApplicationUsers)}"), ImmediatePostData]
    [System.Runtime.Serialization.IgnoreDataMember]
    public ApplicationUserDTO? ApplicationUser {
        get { return applicationUser; }
        set {
            if(value == applicationUser)
                return;
            applicationUser = value;
            UserName = applicationUser?.UserName;
            OnPropertyChanged(nameof(ApplicationUser));
        }
    }

    [PasswordPropertyText(true)]
    [DataMember]
    public string? Password {
        get { return password; }
        set {
            if(password == value)
                return;
            password = value;
            OnPropertyChanged(nameof(Password));
        }
    }

    IObjectSpace? IObjectSpaceLink.ObjectSpace {
        get => objectSpace;
        set {
            objectSpace = value;
            if(objectSpace != null) {
                logonDataProvider = objectSpace.ServiceProvider.GetService<ILogonDataProvider>();
            }
            ApplicationUser = null;
            Company = null;
            _companies = null;
        }
    }

    private void OnPropertyChanged(string propertyName) {
        if(PropertyChanged != null) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    void ISupportClearPassword.ClearPassword() {
        password = null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

#nullable restore