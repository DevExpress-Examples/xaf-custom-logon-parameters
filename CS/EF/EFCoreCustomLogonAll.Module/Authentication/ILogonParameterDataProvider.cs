#nullable enable
namespace EFCoreCustomLogonAll.Module.Authentication;

public interface ILogonDataProvider {
    IList<CompanyDTO> GetCompanies();
    IList<ApplicationUserDTO> GetCompanyUsers(Guid companyID);
}
#nullable restore
