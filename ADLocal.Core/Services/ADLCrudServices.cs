using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text;
using System.Text.RegularExpressions;
using ADLocal.Core.Entities;

namespace ADLocal.Core.Services
{
    public class ADLCrudServices
    {
        #region OU operations
        public ADLOU CreateOU(string ouName)
        {
            try
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(ADLContext.contextLDAPRoot, ADLContext.contextUsername, ADLContext.contextPw);
                DirectoryEntry newOU = directoryEntry.Children.Add("OU=" + ouName, "OrganizationalUnit");
                newOU.CommitChanges();
                return new ADLOU(newOU.Path);
            }
            catch (Exception error)
            {
                return null;
            }
        }
        public ADLOU CreateOU(ADLOU parentOU, string ouName)
        {
            try
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(parentOU.Path, ADLContext.contextUsername, ADLContext.contextPw);
                DirectoryEntry newOU = directoryEntry.Children.Add("OU=" + ouName, "OrganizationalUnit");
                newOU.CommitChanges();
                return new ADLOU(newOU.Path);
            }
            catch (Exception error)
            {
                return null;
            }
        }
        public ADLOU RenameOU(ADLOU adlOU, string newName)
        {
            if (newName.Substring(0, 3).ToUpper() != "OU=")
                newName = "OU=" + newName;
            try
            {
                DirectoryEntry directoryEntry =
                    new DirectoryEntry(adlOU.Path,
                        ADLContext.contextUsername,
                        ADLContext.contextPw);
                directoryEntry.Rename(newName);
                directoryEntry.CommitChanges();
                return new ADLOU(directoryEntry.Path);
            }
            catch (Exception error)
            {
                return null;
            }



        }
        public bool DeleteOU(ADLOU adlOU, bool deleteWhenNotEmpty = false)
        {
            try
            {
                DirectoryEntry directoryEntry =
                    new DirectoryEntry(adlOU.Path,
                        ADLContext.contextUsername,
                        ADLContext.contextPw);


                if (deleteWhenNotEmpty)
                {
                    directoryEntry.DeleteTree();
                    directoryEntry.CommitChanges();
                }
                else
                {
                    DirectoryEntry parent = directoryEntry.Parent;
                    parent.Children.Remove(directoryEntry);
                    parent.CommitChanges();
                }


                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
        #region MISC operations
        public void MovePrincipal(ADLUser user, ADLOU destinationOU)
        {
            // Deze methode verplaats de meegeleverd user
            // naar de meegeleverde OU
            DirectoryEntry currentDirectoryEntry = user.DirectoryEntry;
            DirectoryEntry destinationDirectory = destinationOU.DirectoryEntry;
            currentDirectoryEntry.MoveTo(destinationDirectory);
        }
        public void MovePrincipal(ADLGroup group, ADLOU destinationOU)
        {
            // Deze methode verplaats de meegeleverd group
            // naar de meegeleverde OU
            DirectoryEntry currentDirectoryEntry = group.DirectoryEntry;
            DirectoryEntry destinationDirectory = destinationOU.DirectoryEntry;
            currentDirectoryEntry.MoveTo(destinationDirectory);
        }
        #endregion
        #region USER operations
        public ADLUser CreateUser(ADLOU targetOU, string firstname, string lastname, string loginName, string password, bool isEnabled, DateTime? accountExpirationDate)
        {
            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, 
                ADLContext.contextAddress,
                ADLContext.contextRootDomain,
                ADLContext.contextUsername,
                ADLContext.contextPw);
            UserPrincipal userPrincipal = new UserPrincipal(principalContext);
            userPrincipal.GivenName = firstname;
            userPrincipal.Surname = lastname;
            userPrincipal.DisplayName = firstname + " " + lastname;
            userPrincipal.SamAccountName = loginName;
            userPrincipal.UserPrincipalName = loginName + ADLContext.contextEmail;
            userPrincipal.SetPassword(password);
            userPrincipal.Enabled = isEnabled;
            userPrincipal.AccountExpirationDate = accountExpirationDate;
            try
            {
                userPrincipal.Save();
            }
            catch (Exception error)
            {
                return null;
            }
            ADLUser adlUser = new ADLUser(userPrincipal.SamAccountName);
            try
            {
                MovePrincipal(adlUser, targetOU);
            }
            catch (Exception error)
            {

            }
            return new ADLUser(userPrincipal.SamAccountName);
        }
        public bool UpdateUser(ADLUser adlUser, ADLOU adlTargetOU, string firstname, string lastname, string loginName, string password, bool isEnabled, DateTime? accountExpirationDate)
        {
            adlUser.UserPrincipal.GivenName = firstname;
            adlUser.UserPrincipal.Surname = lastname;
            adlUser.UserPrincipal.DisplayName = firstname + " " + lastname;
            adlUser.UserPrincipal.SamAccountName = loginName;
            adlUser.UserPrincipal.UserPrincipalName = loginName + ADLContext.contextEmail;

            if (password.Trim() != "")
                adlUser.UserPrincipal.SetPassword(password);
            adlUser.UserPrincipal.Enabled = isEnabled;
            adlUser.UserPrincipal.AccountExpirationDate = accountExpirationDate;
            try
            {
                adlUser.UserPrincipal.Save();
                adlUser.SamAccountName = loginName;

                // de Name-property dient ook nog aangepast te worden
                // maar is read-only
                // Name pas je aan via een direcotryEntry
                DirectoryEntry entry = (DirectoryEntry)adlUser.UserPrincipal.GetUnderlyingObject();
                entry.Rename("CN=" + loginName);
                entry.CommitChanges();
                //DirectoryEntry directoryEntry = new DirectoryEntry("LDAP://" + adlUser.UserPrincipal.DistinguishedName, ADLContext.contextUsername, ADLContext.contextPw);
                //string x = directoryEntry.Name;
                //directoryEntry.Rename("CN=" + loginName);

            }
            catch (Exception fout)
            {
                return false;
            }
            if (adlTargetOU.Path != adlUser.DirectoryEntry.Path)
            {
                MovePrincipal(adlUser, adlTargetOU);
            }
            return true;
        }
        public bool DeleteUser(ADLUser adlUser)
        {
            try
            {
                adlUser.UserPrincipal.Delete();
                adlUser = null;
                return true;
            }
            catch
            {
                return false;
            }

        }

        //// onderstaande methode uit dienst genomen wegens VEEL TE TRAAG
        //public bool unused_AddUserToGroup(ADLGroup adlGroup, ADLUser adlUser)
        //{
        //    try
        //    {
        //        adlGroup.GroupPrincipal.Members.Add(adlUser.UserPrincipal);
        //        adlGroup.GroupPrincipal.Save();
        //        return true;
        //    }
        //    catch (Exception fout)
        //    {
        //        return false;
        //    }
        //}
        public bool AddUserToGroup(ADLGroup adlGroup, ADLUser adlUser)
        {
            try
            {
                string userSid = string.Format($"<SID={ToSidString(adlUser.DirectoryEntry)}>");
                adlGroup.DirectoryEntry.Properties["member"].Add(userSid);
                adlGroup.DirectoryEntry.CommitChanges();
                return true;
            }
            catch (Exception fout)
            {
                return false;
            }
        }

        //// onderstaande methode uit dienst genomen wegens VEEL TE TRAAG
        //public bool unused_RemoveUserFromGroup(ADLGroup adlGroup, ADLUser adlUser)
        //{
        //    try
        //    {
        //        adlGroup.GroupPrincipal.Members.Remove(adlUser.UserPrincipal);
        //        adlGroup.GroupPrincipal.Save();
        //        return true;
        //    }
        //    catch (Exception fout)
        //    {
        //        return false;
        //    }
        //}
        public bool RemoveUserFromGroup(ADLGroup adlGroup, ADLUser adlUser)
        {
            try
            {
                string userSid = string.Format($"<SID={ToSidString(adlUser.DirectoryEntry)}>");
                adlGroup.DirectoryEntry.Properties["member"].Remove(userSid);
                adlGroup.DirectoryEntry.CommitChanges();
                return true;
            }
            catch (Exception fout)
            {
                return false;
            }
        }
        #endregion
        #region GROUP operations
        public ADLGroup CreateGroup(ADLOU targetOU, string groupName)
        {
            // deze methode maakt een nieuwe groep aan (met de naam groupName)
            // in de megeleverde OUm

            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain,
                ADLContext.contextAddress,
                ADLContext.contextRootDomain,
                ADLContext.contextUsername,
                ADLContext.contextPw);
            GroupPrincipal groupPrincipal = new GroupPrincipal(principalContext);
            groupPrincipal.Name = groupName;
            groupPrincipal.SamAccountName = groupName;
            try
            {
                groupPrincipal.Save();
                ADLGroup adlGroup = new ADLGroup(groupPrincipal.SamAccountName);
                MovePrincipal(adlGroup, targetOU);
                return adlGroup;
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }
        public bool DeleteGroup(ADLGroup adlGroup)
        {
            // deze methode verwijdert de meegeleverde groep uit AD
            try
            {
                adlGroup.GroupPrincipal.Delete();
                adlGroup = null;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public ADLGroup UpdateGroup(ADLGroup adlGroup, ADLOU targetOU, string newGroupName)
        {
            // deze methode hernoemt en verplaatst de groep (adlGroup) 
            // die meegeleverd werd
            // * nieuwe OU via argument targetOU
            // * nieuwe groepsnaam via argument groupName
            ADLGroup retourGroup = null;
            try
            {
                // dit kan wel ???
                adlGroup.GroupPrincipal.SamAccountName = newGroupName;
                adlGroup.GroupPrincipal.Save();

                // Name prop is readonly bij een bestaande groep, dus onderstaande werkt niet ?????
                // group.GroupPrincipal.Name = groupName;
                //
                // Wat dan wel werkt : 
                // ===========================================================

                DirectoryEntry entry = (DirectoryEntry)adlGroup.GroupPrincipal.GetUnderlyingObject();
                entry.Rename("CN=" + newGroupName);
                entry.CommitChanges();

                // ===========================================================

                retourGroup = new ADLGroup(newGroupName);
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
            if (targetOU.Path != retourGroup.DirectoryEntry.Path)
            {
                MovePrincipal(retourGroup, targetOU);
            }
            return retourGroup;
        }
        
        //// onderstaande methode uit dienst genomen wegens VEEL TE TRAAG
        //public bool unused_AddGroupToGroup(ADLGroup childGroup, ADLGroup parentGroup)
        //{
        //    // deze methode voegt de eerste groep (childGroupPrincipal) toe
        //    // als lid van de tweede groep (parentGroupPrincipal)
        //    try
        //    {
        //        parentGroup.GroupPrincipal.Members.Add(childGroup.GroupPrincipal);
        //        parentGroup.GroupPrincipal.Save();
        //        return true;
        //    }
        //    catch (Exception fout)
        //    {
        //        return false;
        //    }
        //}
        public bool AddGroupToGroup(ADLGroup childGroup, ADLGroup parentGroup)
        {
            try
            {
                string userSid = string.Format($"<SID={ToSidString(childGroup.DirectoryEntry)}>");
                parentGroup.DirectoryEntry.Properties["member"].Add(userSid);
                parentGroup.DirectoryEntry.CommitChanges();
                return true;
            }
            catch (Exception fout)
            {
                return false;
            }
        }
        
        //// onderstaande methode uit dienst genomen wegens VEEL TE TRAAG
        //public bool unused_RemoveGroupFromGroup(ADLGroup childGroup, ADLGroup parentGroup)
        //{
        //    try
        //    {
        //        parentGroup.GroupPrincipal.Members.Remove(childGroup.GroupPrincipal);
        //        parentGroup.GroupPrincipal.Save();
        //        return true;
        //    }
        //    catch (Exception fout)
        //    {
        //        return false;
        //    }
        //}
        public bool RemoveGroupFromGroup(ADLGroup childGroup, ADLGroup parentGroup)
        {
            try
            {
                string groupSid = string.Format($"<SID={ToSidString(childGroup.DirectoryEntry)}>");
                parentGroup.DirectoryEntry.Properties["member"].Remove(groupSid);
                parentGroup.DirectoryEntry.CommitChanges();
                return true;
            }
            catch (Exception fout)
            {
                return false;
            }
        }
        #endregion

        private string ToSidString(DirectoryEntry entry)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in (byte[])entry.Properties["objectSid"].Value)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

    }
}
