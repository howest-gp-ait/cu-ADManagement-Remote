using ADLocal.Core.Services;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADLocal.Core.Entities
{
    public class ADLGroup
    {
        public string SamAccountName { get; set; }
        public GroupPrincipal GroupPrincipal { get; set; } // het Group object op de AD
        public DirectoryEntry DirectoryEntry { get; set; }  // de OU waar group lid van is


        public ADLGroup()
        {
        }
        public ADLGroup(string samAccountName)
        {
            SamAccountName = samAccountName;
            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, 
                ADLContext.contextAddress, ADLContext.contextRootDomain, 
                ADLContext.contextUsername, ADLContext.contextPw);
            GroupPrincipal = GroupPrincipal.FindByIdentity(principalContext, IdentityType.SamAccountName, samAccountName);
            if (GroupPrincipal == null)
            {
                throw new Exception($"{samAccountName} kon niet gevonden worden in AD");
            }
            DirectoryEntry = (DirectoryEntry)GroupPrincipal.GetUnderlyingObject();
        }
        public void SetObject(string path)
        {

        }

        public string DisplayInfo()
        {
            ADLQueryServices adlQueryServices = new ADLQueryServices();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"SamAccountName : {GroupPrincipal.SamAccountName}");
            stringBuilder.AppendLine($"Number of users : {adlQueryServices.NumberOfUsersInGroup(this)}");
            stringBuilder.AppendLine($"Number of groups : {adlQueryServices.NumberOfGroupsInGroup(this)}");
            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return GroupPrincipal.Name;
        }
    }
}
