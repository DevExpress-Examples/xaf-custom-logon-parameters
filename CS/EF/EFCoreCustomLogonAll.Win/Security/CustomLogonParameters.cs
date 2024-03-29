﻿using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp;
using System.ComponentModel;
using System.Runtime.Serialization;
using DevExpress.Persistent.Base;
using EFCoreCustomLogonAll.Module.BusinessObjects;

namespace EFCoreCustomLogonAll.Module.BusinessObjects;

[DomainComponent, Serializable]
[System.ComponentModel.DisplayName("Log In")]
public class CustomLogonParameters : INotifyPropertyChanged {
    private Company company;
    private ApplicationUser _applicationUser;
    private string password;

    [ImmediatePostData]
    public Company Company {
        get { return company; }
        set {
            if(value == company) return;
            company = value;
            if(ApplicationUser?.Company != company) {
                ApplicationUser = null;
            }
            OnPropertyChanged(nameof(Company));
        }
    }
    [DataSourceProperty("Company.ApplicationUsers"), ImmediatePostData]
    public ApplicationUser ApplicationUser {
        get { return _applicationUser; }
        set {
            if(value == _applicationUser) return;
            _applicationUser = value;
            Company = _applicationUser?.Company;
            UserName = _applicationUser?.UserName;
            OnPropertyChanged(nameof(ApplicationUser));
        }
    }
    [Browsable(false)]
    public String UserName { get; set; }
    [PasswordPropertyText(true)]
    public string Password {
        get { return password; }
        set {
            if(password == value) return;
            password = value;
        }
    }

    private void OnPropertyChanged(string propertyName) {
        if(PropertyChanged != null) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;

    public void RefreshPersistentObjects(IObjectSpace objectSpace) {
        ApplicationUser = (UserName == null) ? null : objectSpace.FirstOrDefault<ApplicationUser>(e => e.UserName == UserName);
    }
}