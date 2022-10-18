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
    /// Interaction logic for WinUserGroupMembership.xaml
    /// </summary>
    public partial class WinUserGroupMembership : Window
    {
        public WinUserGroupMembership()
        {
            InitializeComponent();
        }

        public ADLUser adlActiveUser;
        public bool isRefreshRequired = false;
        private List<ADLGroup> adlAvailableGroups;
        private List<ADLGroup> allAdlGroups;
        private List<ADLGroup> adlMemberShipGroups;
        private ADLQueryServices adlQueryServices = new ADLQueryServices();
        private ADLCrudServices adlCrudServices = new ADLCrudServices();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            allAdlGroups = adlQueryServices.GetAllGroups().ToList();
            PopulateMemberShipGroups();
            PopulateAvailableGroups();
            DisplayMemberShipGroups();
            DisplayAvailableGroups();
            txtUserName.Text = adlActiveUser.SamAccountName;
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // eerst ActiveUser uit alle groepen halen en vervolgens in alle groepen plaatsen die in lstMemberOff zitten

            foreach (ADLGroup adlGroup in adlMemberShipGroups)
            {
                adlCrudServices.RemoveUserFromGroup(adlGroup, adlActiveUser);
            }

            foreach (var item in lstMemberOff.Items)
            {
                adlCrudServices.AddUserToGroup((ADLGroup)item, adlActiveUser);
            }
            isRefreshRequired = true;
            this.Close();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            isRefreshRequired = false;
            this.Close();
        }
        private void BtnAddToGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstAvailable.SelectedIndex == -1) return;
            lstMemberOff.Items.Add(lstAvailable.SelectedItem);
            lstAvailable.Items.Remove(lstAvailable.SelectedItem);
            // user wordt nog niet echt member gemaakt : dat gebeurt pas bij het opslaan
        }

        private void BtnRemoveFromGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lstMemberOff.SelectedIndex == -1) return;
            lstAvailable.Items.Add(lstMemberOff.SelectedItem);
            lstMemberOff.Items.Remove(lstMemberOff.SelectedItem);
            // user wordt nog niet echt uit de groep gehaald : dat gebeurt pas bij het opslaan
        }

        private void PopulateMemberShipGroups()
        {
            adlMemberShipGroups = adlQueryServices.GetUserGroupMemberShip(adlActiveUser).ToList();
        }
        private void PopulateAvailableGroups()
        {
            adlAvailableGroups = new List<ADLGroup>();
            foreach (ADLGroup adlGroup in allAdlGroups)
            {
                if (!adlMemberShipGroups.Exists(g => g.SamAccountName.ToUpper() == adlGroup.SamAccountName.ToUpper()))
                {
                    adlAvailableGroups.Add(adlGroup);
                }
            }
        }
        private void DisplayMemberShipGroups()
        {
            lstMemberOff.Items.Clear();
            foreach (ADLGroup adlGroup in adlMemberShipGroups)
            {
                lstMemberOff.Items.Add(adlGroup);
            }
        }
        private void DisplayAvailableGroups()
        {
            lstAvailable.Items.Clear();
            foreach (ADLGroup adlGroup in adlAvailableGroups)
            {
                lstAvailable.Items.Add(adlGroup);
            }
        }
    }
}
