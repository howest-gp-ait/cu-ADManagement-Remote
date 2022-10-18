using ADLocal.Core.Entities;
using ADLocal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ADLocal.Wpf
{
    /// <summary>
    /// Interaction logic for WinGroupGroupMembership.xaml
    /// </summary>
    public partial class WinGroupGroupMembership : Window
    {
        public ADLGroup activeAdlGroup;
        public bool isRefreshRequired = false;

        private List<ADLUser> usersInActiveGroup = new List<ADLUser>();
        private List<ADLUser> usersNotInActiveGroup = new List<ADLUser>();
        private List<ADLUser> allUsers = new List<ADLUser>();
        private List<ADLGroup> groupsInActiveGroup = new List<ADLGroup>();
        private List<ADLGroup> groupsNotInActiveGroup = new List<ADLGroup>();
        private List<ADLGroup> allGroups = new List<ADLGroup>();

        private ADLQueryServices adlQueryServices = new ADLQueryServices();
        private ADLCrudServices adlCrudServices = new ADLCrudServices();

        public WinGroupGroupMembership()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            allGroups = adlQueryServices.GetAllGroups().ToList();
            allUsers = adlQueryServices.GetAllUsers().ToList();
            txtGroupName.Text = activeAdlGroup.SamAccountName;
            usersInActiveGroup = adlQueryServices.GetUsersInGroup(activeAdlGroup).OrderBy(g => g.SamAccountName).ToList();
            groupsInActiveGroup = adlQueryServices.GetGroupsInGroup(activeAdlGroup).OrderBy(g => g.SamAccountName).ToList();
            PopulateUsersNotInGroups();
            PopulateGroupsNotInGroups();
            DisplayPopulations();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            isRefreshRequired = true;

            foreach (ADLUser adlUser in usersInActiveGroup)
            {
                adlCrudServices.RemoveUserFromGroup(activeAdlGroup, adlUser);
            }
            foreach (var item in lstUsersIn.Items)
            {
                adlCrudServices.AddUserToGroup(activeAdlGroup, ((ADLUser)item));
            }
            foreach (ADLGroup adlGroup in groupsInActiveGroup)
            {
                adlCrudServices.RemoveGroupFromGroup(adlGroup, activeAdlGroup);
            }
            foreach (var item in lstGroupsIn.Items)
            {
                adlCrudServices.AddGroupToGroup(((ADLGroup)item), activeAdlGroup);
            }

            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            isRefreshRequired = false;
            this.Close();
        }

        private void BtnAddUserToGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsersOut.SelectedIndex == -1) return;
            lstUsersIn.Items.Add(lstUsersOut.SelectedItem);
            lstUsersOut.Items.Remove(lstUsersOut.SelectedItem);
            // user wordt nog niet echt member gemaakt : dat gebeurt pas bij het opslaan
        }

        private void BtnRemoveUserFromGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsersIn.SelectedIndex == -1) return;
            lstUsersOut.Items.Add(lstUsersIn.SelectedItem);
            lstUsersIn.Items.Remove(lstUsersIn.SelectedItem);
            // user wordt nog niet echt uit de groep gehaald : dat gebeurt pas bij het opslaan
        }

        private void BtnAddGroupToGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstGroupsOut.SelectedIndex == -1) return;
            lstGroupsIn.Items.Add(lstGroupsOut.SelectedItem);
            lstGroupsOut.Items.Remove(lstGroupsOut.SelectedItem);
            // group wordt nog niet echt member gemaakt : dat gebeurt pas bij het opslaan
        }

        private void BtnRemoveGroupFromGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstGroupsIn.SelectedIndex == -1) return;
            lstGroupsOut.Items.Add(lstGroupsIn.SelectedItem);
            lstGroupsIn.Items.Remove(lstGroupsIn.SelectedItem);
            // group wordt nog niet echt uit de groep gehaald : dat gebeurt pas bij het opslaan
        }

        private void PopulateUsersNotInGroups()
        {
            if (activeAdlGroup.GroupPrincipal is null)
            {
                usersNotInActiveGroup = allUsers;
            }
            else
            {
                foreach (ADLUser adlUser in allUsers)
                {
                    if (!adlUser.UserPrincipal.IsMemberOf(activeAdlGroup.GroupPrincipal))
                    {
                        usersNotInActiveGroup.Add(adlUser);
                    }
                }
            }
            usersNotInActiveGroup = usersNotInActiveGroup.OrderBy(g => g.SamAccountName).ToList();
        }
        private void PopulateGroupsNotInGroups()
        {
            if (activeAdlGroup.GroupPrincipal is null)
            {
                groupsNotInActiveGroup = allGroups;
            }
            else
            {
                foreach (ADLGroup adlGroup in allGroups)
                {
                    if (!adlGroup.GroupPrincipal.IsMemberOf(activeAdlGroup.GroupPrincipal))
                    {
                        groupsNotInActiveGroup.Add(adlGroup);
                    }
                }
            }
            groupsNotInActiveGroup = groupsNotInActiveGroup.OrderBy(g => g.SamAccountName).ToList();
        }
        private void DisplayPopulations()
        {
            lstUsersIn.Items.Clear();
            lstUsersOut.Items.Clear();
            lstGroupsIn.Items.Clear();
            lstGroupsOut.Items.Clear();
            foreach (ADLUser adlUser in usersInActiveGroup)
            {
                lstUsersIn.Items.Add(adlUser);
            }
            foreach (ADLUser adlUser in usersNotInActiveGroup)
            {
                lstUsersOut.Items.Add(adlUser);
            }
            foreach (ADLGroup adlGroup in groupsInActiveGroup)
            {
                lstGroupsIn.Items.Add(adlGroup);
            }
            foreach (ADLGroup adlGroup in groupsNotInActiveGroup)
            {
                lstGroupsOut.Items.Add(adlGroup);
            }
        }
    }
}
