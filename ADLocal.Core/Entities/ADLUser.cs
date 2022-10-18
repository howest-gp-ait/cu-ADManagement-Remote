using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text;

namespace ADLocal.Core.Entities
{
    public class ADLUser
    {
        public string SamAccountName { get; set; } // bv jan.de.deurwaerder
        public UserPrincipal UserPrincipal { get; set; }  // het user-object op de AD
        public DirectoryEntry DirectoryEntry { get; set; }  // de OU waar user lid van is
        public ADLUser()
        {
        }
        public ADLUser(string samAccountName)
        {
            SamAccountName = samAccountName;
            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, 
                ADLContext.contextAddress, ADLContext.contextRootDomain, ADLContext.contextUsername, ADLContext.contextPw);
            UserPrincipal = UserPrincipal.FindByIdentity(principalContext, IdentityType.SamAccountName, samAccountName);
            if (UserPrincipal == null)
            {
                throw new Exception($"{samAccountName} kon niet gevonden worden in AD");
            }
            DirectoryEntry = (DirectoryEntry)UserPrincipal.GetUnderlyingObject();
        }
        public override string ToString()
        {
            return UserPrincipal.DisplayName;
        }
        public string DisplayInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Displayname : {UserPrincipal.DisplayName}");
            stringBuilder.AppendLine($"Firstname : {UserPrincipal.GivenName}");
            stringBuilder.AppendLine($"Lastname : {UserPrincipal.Surname}");
            stringBuilder.AppendLine($"Username : {UserPrincipal.UserPrincipalName}");
            if (UserPrincipal.AccountExpirationDate == null)
                stringBuilder.AppendLine("Account never expires");
            else
                stringBuilder.AppendLine("Expires on " + ((DateTime)UserPrincipal.AccountExpirationDate).ToString("dd/MM/yyyy"));
            if (UserPrincipal.Enabled == true)
            {
                stringBuilder.AppendLine($"User is ENABLED");
            }
            else
            {
                stringBuilder.AppendLine($"User is DISABLED");
            }
            return stringBuilder.ToString();
        }
    }
}
