using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ADLocal.Core.Entities;

namespace ADLocal.Core.Services
{
    public class ADLQueryServices
    {
        #region OU queries
        public IEnumerable<ADLOU> GetAllOUs()
        {

            List<ADLOU> ADLOUs = new List<ADLOU>();
            DirectorySearcher zoeker 
                = new DirectorySearcher(new DirectoryEntry(ADLContext.contextLDAPRoot, ADLContext.contextUsername, ADLContext.contextPw))
            {
                Filter = "(objectCategory=organizationalUnit)",
                SearchScope = SearchScope.Subtree
            };
            foreach (SearchResult searchResult in zoeker.FindAll())
            {
                ADLOUs.Add(new ADLOU(searchResult.Path));
            }
            return ADLOUs.OrderBy(o => o.DirectoryEntry.Name);
        }
        public IEnumerable<ADLOU> GetBaseOUs()
        {
            using (DirectoryEntry directoryEntry = new DirectoryEntry(ADLContext.contextLDAPRoot, ADLContext.contextUsername, ADLContext.contextPw))
            {
                List<ADLOU> ADLOUs = new List<ADLOU>();
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry)
                {
                    Filter = "(objectCategory=organizationalUnit)",
                    SearchScope = SearchScope.OneLevel
                };
                foreach (SearchResult searchResult in directorySearcher.FindAll())
                {
                    ADLOUs.Add(new ADLOU(searchResult.Path));
                }
                return ADLOUs.OrderBy(o => o.DirectoryEntry.Name);
            }
        }
        public IEnumerable<ADLOU> GetChildOUs(ADLOU parentOU)
        {
            // Deze methode zoekt alle OU's op (NIET recursief)
            // binnen de meegeleverde OU (ttz het pad van die OU)
            List<ADLOU> myOUs = new List<ADLOU>();
            DirectorySearcher directorySearcher = 
                new DirectorySearcher(new DirectoryEntry(parentOU.Path, 
                ADLContext.contextUsername, ADLContext.contextPw))
            {
                Filter = "(objectCategory=organizationalUnit)",
                SearchScope = SearchScope.OneLevel
            };
            foreach (SearchResult searchResult in directorySearcher.FindAll())
            {
                myOUs.Add(new ADLOU(searchResult.Path));
            }
            return myOUs.OrderBy(o=>o.DirectoryEntry.Name);
        }
        #endregion
        #region USER queries
        public IEnumerable<ADLUser> GetAllUsers()
        {
            // deze methode zoekt alle users in het basispad
            // en alle onderliggende paden
            List<ADLUser> users = new List<ADLUser>();
            DirectorySearcher directorySearcher = new DirectorySearcher
                (new DirectoryEntry
                (ADLContext.contextLDAPRoot, ADLContext.contextUsername, ADLContext.contextPw))
            {
                Filter = "(objectCategory=person)",
                SearchScope = SearchScope.Subtree
            };
            foreach (SearchResult searchResult in directorySearcher.FindAll())
            {
                users.Add(new ADLUser(searchResult.Properties["sAMAccountName"][0].ToString()));
            }
            return users.OrderBy(u=>u.SamAccountName);
        }
        public IEnumerable<ADLUser> GetAllUsers(ADLOU adlOU)
        {
            // deze methode zoekt alle users in het meegeleverde OU
            List<ADLUser> users = new List<ADLUser>();
            DirectorySearcher directorySearcher = new DirectorySearcher
                (new DirectoryEntry(adlOU.Path,
                ADLContext.contextUsername, ADLContext.contextPw))
            {
                Filter = "(objectCategory=person)",
                SearchScope = SearchScope.OneLevel
            };
            foreach (SearchResult searchResult in directorySearcher.FindAll())
            {
                users.Add(new ADLUser(searchResult.Properties["sAMAccountName"][0].ToString()));
            }
            return users.OrderBy(u=>u.SamAccountName);
        }

        // onderstaande methode uit dienst genomen wegens VEEL TE TRAAG
        public IEnumerable<ADLGroup> unused_GetUserGroupMemberShip(ADLUser adlUser)
        {
            // deze methode retourneert alle groepen
            // waartoe de meegeleverde gebruiker behoort
            List<ADLGroup> adlGroups = new List<ADLGroup>();
            // onderstaande try catch werd toegevoegd omdat net nieuw toegevoegde gebruikers hier een error op gaven
            try
            {
                PrincipalContext principalContext = new PrincipalContext(ContextType.Domain,
                    ADLContext.contextAddress, ADLContext.contextRootDomain,
                    ADLContext.contextUsername, ADLContext.contextPw);
                foreach (GroupPrincipal groupPrincipal in adlUser.UserPrincipal.GetGroups(principalContext))
                {
                    adlGroups.Add(new ADLGroup(groupPrincipal.SamAccountName));
                }
            }
            catch (Exception fout)
            {
                string bericht = fout.Message;
            }
            return adlGroups.OrderBy(g => g.SamAccountName);
        }
        public IEnumerable<ADLGroup> GetUserGroupMemberShip(ADLUser adlUser)
        {
            List<ADLGroup> adlGroups = new List<ADLGroup>();

            DirectoryEntry de = adlUser.UserPrincipal.GetUnderlyingObject() as DirectoryEntry;
            if (de.Properties.Contains("memberof"))
            {
                foreach (var dn in de.Properties["memberof"])
                {
                    // bv inhoud dn = //CN=SuperGroup,OU=GroepenVanIlse,OU=ou.Ilse.Bonne,OU=StudentsBase,DC=AIT,DC=GRADPROG
                    string groupname = dn.ToString().Split(',')[0];
                    groupname = Regex.Replace(groupname,"CN=","", RegexOptions.IgnoreCase);
                    adlGroups.Add(new ADLGroup(groupname));
                }
            }
            return adlGroups.OrderBy(g => g.SamAccountName);
        }
        #endregion
        #region GROUP queries
        public IEnumerable<ADLGroup> GetAllGroups()
        {
            // deze methode zoekt alle groepen in het basispad
            // en alle onderliggende paden            
            List<ADLGroup> adlGroups = new List<ADLGroup>();
            DirectorySearcher directorySearcher = new DirectorySearcher(
                new DirectoryEntry(ADLContext.contextLDAPRoot,
                ADLContext.contextUsername, ADLContext.contextPw))
            {
                Filter = "(objectCategory=group)",
                SearchScope = SearchScope.Subtree
            };
            foreach (SearchResult searchResult in directorySearcher.FindAll())
            {
                adlGroups.Add(new ADLGroup(searchResult.Properties["sAMAccountName"][0].ToString()));
            }
            return adlGroups.OrderBy(g=>g.SamAccountName);
        }
        public IEnumerable<ADLGroup> GetAllGroups(ADLOU adlOU)
        {
            // deze methode zoekt alle groepen in het meegeleverde pad
            // en alle onderliggende paden  
            List<ADLGroup> adlGroups = new List<ADLGroup>();
            DirectorySearcher directorySearcher = new DirectorySearcher(
                new DirectoryEntry(adlOU.Path, ADLContext.contextUsername,
                ADLContext.contextPw))
            {
                Filter = "(objectCategory=group)",
                SearchScope = SearchScope.Subtree
            };
            foreach (SearchResult searchResult in directorySearcher.FindAll())
            {
                adlGroups.Add(new ADLGroup(searchResult.Properties["sAMAccountName"][0].ToString()));
            }
            return adlGroups.OrderBy(g => g.SamAccountName);
        }
        public IEnumerable<ADLGroup> GetGroupGroupMemberShip(ADLGroup adlGroup)
        {
            // deze methode retourneert alle groepen
            // waar de opgegeven groep lid van is
            List<ADLGroup> adlGroups = GetAllGroups(new ADLOU(ADLContext.contextLDAPRoot)).ToList();
            List<ADLGroup> groupMemberships = new List<ADLGroup>();
            foreach (ADLGroup adlSearchGroup in adlGroups)
            {
                if (adlSearchGroup.GroupPrincipal.IsMemberOf(adlGroup.GroupPrincipal))
                {
                    groupMemberships.Add(adlSearchGroup);
                }
            }
            return groupMemberships.OrderBy(g => g.SamAccountName);
        }
        public IEnumerable<ADLUser> GetUsersInGroup(ADLGroup adlGroup)
        {
            // deze methode retourneert alle users
            // die lid zijn van de opgegeven groep
            List<ADLUser> adlUsers = new List<ADLUser>();
            foreach (Principal principal in adlGroup.GroupPrincipal.GetMembers(false))
            {
                if (principal is UserPrincipal)
                {
                    adlUsers.Add(new ADLUser(principal.SamAccountName));
                }
            }
            return adlUsers.OrderBy(u => u.SamAccountName);

        }
        public IEnumerable<ADLGroup> GetGroupsInGroup(ADLGroup adlGroup)
        {
            // deze methode retourneert alle groepen
            // die lid zijn van de opgegeven groep
            List<ADLGroup> aDLGroups = new List<ADLGroup>();
            foreach (Principal principal in adlGroup.GroupPrincipal.GetMembers())
            {
                if (principal is GroupPrincipal)
                {
                    aDLGroups.Add(new ADLGroup(principal.SamAccountName));
                }
            }
            return aDLGroups.OrderBy(g => g.SamAccountName);
        }

        public int NumberOfUsersInGroup(ADLGroup adlGroup)
        {
            List<Principal> allMembers = adlGroup.GroupPrincipal.GetMembers(false).ToList();
            int count = 0;
            foreach (Principal Principal in allMembers)
                if(Principal is UserPrincipal)
                    count++;
            return count;
        }
        public int NumberOfGroupsInGroup(ADLGroup adlGroup)
        {
            List<Principal> allMembers = adlGroup.GroupPrincipal.GetMembers(false).ToList();
            int count = 0;
            foreach (Principal Principal in allMembers)
                if (Principal is GroupPrincipal)
                    count++;
            return count;
        }
        #endregion
    }
}
