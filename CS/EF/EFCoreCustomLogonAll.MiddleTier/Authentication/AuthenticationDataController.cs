using DevExpress.ExpressApp;
using EFCoreCustomLogonAll.Module.Authentication;
using EFCoreCustomLogonAll.Module.BusinessObjects;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCustomLogonAll.MiddleTier.Authentication {
    // TODO description
    public class AuthenticationDataController : ControllerBase, IDisposable {
        IObjectSpace nonSecuredObjectSpace;
        private readonly INonSecuredObjectSpaceFactory? nonSecuredObjectSpaceFactory;
        public AuthenticationDataController(INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory) {
            this.nonSecuredObjectSpaceFactory = nonSecuredObjectSpaceFactory;
        }

        [HttpGet("/GetCompanies")]
        public ActionResult GetCompanies() {
            List<CompanyDTO> companies = new List<CompanyDTO>();
            foreach(var company in GetNonSecuredObjectSpace().GetObjects<Company>()) {
                companies.Add(new CompanyDTO() { ID = company.ID, Name = company.Name });
            }
            return Ok(companies);
        }
        [HttpGet("/GetApplicationUsers({companyKey})")]
        public ActionResult GetApplicationUsers(string companyKey) {
            var users = new List<ApplicationUserDTO>();
            Guid guid = new Guid(companyKey);
            foreach(var user in GetNonSecuredObjectSpace().GetObjectsQuery<ApplicationUser>().Where(user => user.Company != null && user.Company.ID == guid)) {
                users.Add(new ApplicationUserDTO() { ID = user.ID, UserName = user.UserName });
            }

            return Ok(users);
        }
        private IObjectSpace GetNonSecuredObjectSpace() {
            if(nonSecuredObjectSpace == null) {
                nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<Company>();
            }
            return nonSecuredObjectSpace;
        }

        public void Dispose() {
            if(nonSecuredObjectSpace != null) {
                nonSecuredObjectSpace.Dispose();
                nonSecuredObjectSpace = null;
            }
        }
    }
}
