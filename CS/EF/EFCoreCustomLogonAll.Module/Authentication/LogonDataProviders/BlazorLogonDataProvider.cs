#nullable enable
using DevExpress.ExpressApp;
using EFCoreCustomLogonAll.Module.BusinessObjects;

namespace EFCoreCustomLogonAll.Module.Authentication;

public class BlazorLogonDataProvider : ILogonDataProvider {
    readonly INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory;
    public BlazorLogonDataProvider(INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory) {
        this.nonSecuredObjectSpaceFactory = nonSecuredObjectSpaceFactory;
    }

    public IList<CompanyDTO> GetCompanies() {
        List<CompanyDTO> companies = new List<CompanyDTO>();
        using(var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<Company>()) {
            foreach(var company in nonSecuredObjectSpace.GetObjects<Company>()) {
                companies.Add(new CompanyDTO() { ID = company.ID, Name = company.Name, LogonDataProvider = this });
            }
        }
        return companies.AsReadOnly();
    }

    public IList<ApplicationUserDTO> GetCompanyUsers(Guid companyID) {
        var users = new List<ApplicationUserDTO>();
        using(var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<Company>()) {
            foreach(var user in nonSecuredObjectSpace.GetObjectsQuery<ApplicationUser>().Where(user => user.Company != null && user.Company.ID == companyID)) {
                users.Add(new ApplicationUserDTO() { ID = user.ID, UserName = user.UserName, CompanyID = companyID });
            }
        }
        return users.AsReadOnly();
    }
}
#nullable restore