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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ADLocal.Core.Entities;
using ADLocal.Core.Services;

namespace ADLocal.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ADLQueryServices adlQueryServices = new ADLQueryServices();
        ADLCrudServices adlCrudServices = new ADLCrudServices();
        bool isNew;
        bool onRoot;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BuildTreeView();
            DefaultView();
        }
        private void TVOU_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (TVOU.SelectedItem == null) return;
            TreeViewItem itm = (TreeViewItem)TVOU.SelectedItem;
            ADLOU adlOU = (ADLOU)itm.Tag;
            lblOUADPath.Content = adlOU.Path;
            lblOUParentNode.Content = adlOU.DirectoryEntry.Parent.Path;
            txtOUName.Text = adlOU.DirectoryEntry.Name.Replace("OU=", "");
            ShowUsers(adlOU);
            ShowGroups(adlOU);

            lblUserInfo.Content = "";
            lblGroupInfo.Content = "";
        }
        
        //** EVENT HANDLERS OU BUTTONS
        private void BtnNewOU_Click(object sender, RoutedEventArgs e)
        {
            if (TVOU.SelectedItem != null)
            {
                isNew = true;
                onRoot = false;
                EditView();
                lblOUParentNode.Content = lblOUADPath.Content;
                lblOUADPath.Content = string.Empty;
                txtOUName.Text = string.Empty;
            }
            else
            {
                MessageBox.Show("Kies eerst een parent OU !", "Fout");
            }
        }
        private void BtnNewRootOU_Click(object sender, RoutedEventArgs e)
        {
            isNew = true;
            onRoot = true;
            EditView();
            lblOUParentNode.Content = ADLContext.contextLDAPRoot;
            lblOUADPath.Content = string.Empty;
            txtOUName.Text = string.Empty;
        }
        private void BtnEditOU_Click(object sender, RoutedEventArgs e)
        {
            if (TVOU.SelectedItem != null)
            {
                isNew = false;
                EditView();
            }
            else
            {
                MessageBox.Show("Selecteer eerst een OU !", "Fout");
            }
        }
        private void BtnDeleteOU_Click(object sender, RoutedEventArgs e)
        {
            if (TVOU.SelectedItem != null)
            {
                if(MessageBox.Show("Ben je zeker dat deze OU en alle onderliggende objecten mogen verwijderd worden?", "OU verwijderen", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    TreeViewItem itm = (TreeViewItem)TVOU.SelectedItem;
                    ADLOU adlOU = (ADLOU)itm.Tag;
                    if(!adlCrudServices.DeleteOU(adlOU, true))
                    {
                        MessageBox.Show("Het verwijderen is mislukt", "Fout");
                        return;
                    }
                    BuildTreeView();
                    DefaultView();
                    ClearControls();
                }
            }
            else
            {
                MessageBox.Show("Selecteer eerst een OU !", "Fout");
            }
        }
        private void BtnSaveOU_Click(object sender, RoutedEventArgs e)
        {
            string ouName = txtOUName.Text;
            ADLOU adlOU = null;
            if (isNew)
            {
                if (onRoot)
                    adlOU = adlCrudServices.CreateOU(ouName);
                else
                    adlOU = adlCrudServices.CreateOU(new ADLOU(lblOUParentNode.Content.ToString()), ouName);
                if(adlOU is null)
                {
                    MessageBox.Show("De OU kon niet aangemaakt worden", "Error");
                    return;
                }
            }
            else
            {
                string path = ((TreeViewItem)TVOU.SelectedItem).Tag.ToString();
                adlOU = new ADLOU(path);
                adlOU = adlCrudServices.RenameOU(adlOU, ouName);
                if (adlOU is null)
                {
                    MessageBox.Show("De OU kon niet gewijzigd worden", "Error");
                    return;
                }
            }

            BuildTreeView();
            SelectNode(adlOU);
            DefaultView();
            ClearControls();
            //TVOU.SelectedValue = ouName;
            TVOU_SelectedItemChanged(null, null);

        }
        private void BtnCancelOU_Click(object sender, RoutedEventArgs e)
        {
            DefaultView();
            ClearControls();
            TVOU_SelectedItemChanged(null, null);
        }
        
        //** EVENT HANDLERS LISTBOXES
        private void LstUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lblUserInfo.Content = "";
            if(lstUsers.SelectedItem != null)
            {
                ADLUser adlUser = (ADLUser)lstUsers.SelectedItem;
                lblUserInfo.Content = adlUser.DisplayInfo();

            }
        }
        private void LstGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lblGroupInfo.Content = "";
            if(lstGroups.SelectedItem != null)
            {
                ADLGroup adlGroup = (ADLGroup)lstGroups.SelectedItem;
                lblGroupInfo.Content = adlGroup.DisplayInfo();
            }
        }

        //** EVENT HANDLERS USER BUTTONS
        private void BtnDetailsUser_Click(object sender, RoutedEventArgs e)
        {
            if (TVOU.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een OU in de boomstructuur links.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (lstUsers.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een USER in de bovenstaande lijst.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            WinUser winUser = new WinUser();
            ADLUser adlUser = (ADLUser)lstUsers.SelectedItem;
            winUser.adlActiveUser = adlUser;
            winUser.isNew = null;
            TreeViewItem itm = (TreeViewItem)TVOU.SelectedItem;
            winUser.adlActiveOU = (ADLOU)(itm.Tag);
            winUser.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winUser.ShowDialog();
        }
        private void BtnNewUser_Click(object sender, RoutedEventArgs e)
        {
            if (TVOU.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een OU in de boomstructuur links.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            WinUser winUser = new WinUser();
            winUser.isNew = true;

            TreeViewItem itm = (TreeViewItem)TVOU.SelectedItem;
            winUser.adlActiveOU = (ADLOU)(itm.Tag);

            winUser.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winUser.ShowDialog();
            if (winUser.isRefreshRequired)
            {
                TVOU_SelectedItemChanged(null, null);
                try
                {
                    int indeks = 0;
                    foreach (ADLUser adlUser in lstUsers.Items)
                    {
                        if (((ADLUser)adlUser).SamAccountName == winUser.adlActiveUser.SamAccountName)
                        {
                            lstUsers.SelectedIndex = indeks;
                            break;
                        }
                        indeks++;
                    }
                }
                catch
                {

                }
            }
        }
        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (TVOU.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een OU in de boomstructuur links.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (lstUsers.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een USER in de bovenstaande lijst.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            WinUser winUser = new WinUser();
            winUser.isNew = false;
            winUser.adlActiveUser = (ADLUser)lstUsers.SelectedItem;
            TreeViewItem itm = (TreeViewItem)TVOU.SelectedItem;
            winUser.adlActiveOU = (ADLOU)(itm.Tag);

            winUser.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winUser.ShowDialog();
            if (winUser.isRefreshRequired)
            {
                TVOU_SelectedItemChanged(null, null);
                try
                {
                    int indeks = 0;
                    foreach (ADLUser adlUser in lstUsers.Items)
                    {
                        if (adlUser.SamAccountName == winUser.adlActiveUser.SamAccountName)
                        {
                            lstUsers.SelectedIndex = indeks;
                            break;
                        }
                        indeks++;
                    }
                }
                catch
                {
                }
            }
        }
        private void BtnGroupUser_Click(object sender, RoutedEventArgs e)
        {
            if (TVOU.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een OU in de boomstructuur links.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (lstUsers.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een USER in de bovenstaande lijst.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            WinUserGroupMembership winUserGroupMembership = new WinUserGroupMembership();
            winUserGroupMembership.adlActiveUser = (ADLUser)lstUsers.SelectedItem;
            winUserGroupMembership.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winUserGroupMembership.ShowDialog();
            if (winUserGroupMembership.isRefreshRequired)
            {
                TVOU_SelectedItemChanged(null, null);
                try
                {
                    int indeks = 0;
                    foreach (ADLUser adlUser in lstUsers.Items)
                    {
                        if (adlUser.SamAccountName == winUserGroupMembership.adlActiveUser.SamAccountName)
                        {
                            lstUsers.SelectedIndex = indeks;
                            break;
                        }
                        indeks++;
                    }
                }
                catch
                {
                }
            }
        }
        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Mag deze gebruiker verwijderd worden?", "Wissen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (lstUsers.SelectedItem == null)
                    return;
                ADLUser victim = (ADLUser)lstUsers.SelectedItem;
                if (adlCrudServices.DeleteUser(victim))
                    TVOU_SelectedItemChanged(null, null);
                else
                    MessageBox.Show("User kon niet verwijderd worden", "Error");
            }
        }

        //** EVENT HANDLERS GROUP BUTTONS
        private void BtnNewGroup_Click(object sender, RoutedEventArgs e)
        {
            if (TVOU.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een OU in de boomstructuur links.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            WinGroup winGroup = new WinGroup();
            winGroup.isNew = true;
            TreeViewItem itm = (TreeViewItem)TVOU.SelectedItem;
            winGroup.activeAdlOU = (ADLOU)(itm.Tag);
            winGroup.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winGroup.ShowDialog();
            if (winGroup.isRefreshRequired)
            {
                TVOU_SelectedItemChanged(null, null);
                try
                {
                    int indeks = 0;
                    foreach (ADLGroup aDLGroup in lstGroups.Items)
                    {
                        if (((ADLGroup)aDLGroup).SamAccountName == winGroup.activeAdlGroup.SamAccountName)
                        {
                            lstGroups.SelectedIndex = indeks;
                            break;
                        }
                        indeks++;
                    }
                }
                catch
                {

                }
            }
        }
        private void BtnEditGroup_Click(object sender, RoutedEventArgs e)
        {
            if (TVOU.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een OU in de boomstructuur links.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (lstGroups.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een GROUP in de bovenstaande lijst.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            WinGroup winGroup = new WinGroup();
            winGroup.isNew = false;
            winGroup.activeAdlGroup = (ADLGroup)lstGroups.SelectedItem;
            TreeViewItem itm = (TreeViewItem)TVOU.SelectedItem;
            winGroup.activeAdlOU = (ADLOU)(itm.Tag);

            winGroup.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winGroup.ShowDialog();
            if (winGroup.isRefreshRequired)
            {
                TVOU_SelectedItemChanged(null, null);
                try
                {
                    int indeks = 0;
                    foreach (ADLGroup adlGroup in lstGroups.Items)
                    {
                        if (adlGroup.SamAccountName == winGroup.activeAdlGroup.SamAccountName)
                        {
                            lstGroups.SelectedIndex = indeks;
                            break;
                        }
                        indeks++;
                    }
                }
                catch
                {
                }
            }
        }
        private void BtnDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Mag deze groep verwijderd worden?", "Wissen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (lstGroups.SelectedItem == null)
                    return;
                ADLGroup victim = (ADLGroup)lstGroups.SelectedItem;
                if (adlCrudServices.DeleteGroup(victim))
                    TVOU_SelectedItemChanged(null, null);
                else
                    MessageBox.Show("Groep kon niet verwijderd worden", "Error");
            }
        }
        private void BtnGroupGroup_Click(object sender, RoutedEventArgs e)
        {
            if (TVOU.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een OU in de boomstructuur links.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (lstGroups.SelectedItem == null)
            {
                MessageBox.Show("Selecteer eerst een GROUP in de bovenstaande lijst.", "Fout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            WinGroupGroupMembership winGroupGroupMembership = new WinGroupGroupMembership();
            winGroupGroupMembership.activeAdlGroup = (ADLGroup)lstGroups.SelectedItem;
            winGroupGroupMembership.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winGroupGroupMembership.ShowDialog();
            if (winGroupGroupMembership.isRefreshRequired)
            {
                TVOU_SelectedItemChanged(null, null);
                try
                {
                    int indeks = 0;
                    foreach (ADLGroup adlGroup in lstGroups.Items)
                    {
                        if (adlGroup.SamAccountName == winGroupGroupMembership.activeAdlGroup.SamAccountName)
                        {
                            lstGroups.SelectedIndex = indeks;
                            break;
                        }
                        indeks++;
                    }
                }
                catch
                {
                }
            }
        }

        //** PRIVATE METHODS
        private void BuildTreeView()
        {
            IEnumerable<ADLOU> adlOUs = adlQueryServices.GetBaseOUs();
            TVOU.Items.Clear();
            foreach (ADLOU adlOU in adlOUs)
            {
                TreeViewItem treeViewItem = new TreeViewItem();
                treeViewItem.Tag = adlOU;
                treeViewItem.Header = adlOU.DirectoryEntry.Name;
                BuildTreeViewRecursive(treeViewItem, adlOU);
                TVOU.Items.Add(treeViewItem);
            }
        }
        private void BuildTreeViewRecursive(TreeViewItem parentItem, ADLOU parentOU)
        {
            IEnumerable<ADLOU> myOUs = adlQueryServices.GetChildOUs(parentOU);
            foreach (ADLOU adlOU in myOUs)
            {
                TreeViewItem treeViewItem = new TreeViewItem();
                treeViewItem.Tag = adlOU;
                treeViewItem.Header = adlOU.DirectoryEntry.Name;
                BuildTreeViewRecursive(treeViewItem, adlOU);
                parentItem.Items.Add(treeViewItem);
            }

        }
        private void DefaultView()
        {
            btnSaveOU.Visibility = Visibility.Hidden;
            btnCancelOU.Visibility = Visibility.Hidden;
            grpOUOverview.IsEnabled = true;
            grpOUDetails.IsEnabled = false;
            grpUsers.IsEnabled = true;
            grpGroups.IsEnabled = true;
        }
        private void EditView()
        {
            btnSaveOU.Visibility = Visibility.Visible;
            btnCancelOU.Visibility = Visibility.Visible;
            grpOUOverview.IsEnabled = false;
            grpOUDetails.IsEnabled = true;
            grpUsers.IsEnabled = false;
            grpGroups.IsEnabled = false;
        }
        private void ClearControls()
        {
            lblOUADPath.Content = string.Empty;
            lblOUParentNode.Content = string.Empty;
            txtOUName.Text = string.Empty;
            lstUsers.ItemsSource = null;
            lstUsers.Items.Refresh();
            lstGroups.ItemsSource = null;
            lstGroups.Items.Refresh();
        }
        private void SelectNode(ADLOU? adlOU)
        {
            if (adlOU is null) return;

            foreach(TreeViewItem treeViewItem in TVOU.Items)
            {
                if(treeViewItem.Tag.ToString() == adlOU.Path )
                {
                    treeViewItem.IsSelected = true;
                    return;
                }
                else
                {
                    SelectNodeRecursive(treeViewItem, adlOU);
                }
            }
        }
        private void SelectNodeRecursive(TreeViewItem parentItem, ADLOU adlOU)
        {
            foreach (TreeViewItem treeViewItem in parentItem.Items)
            {
                if (treeViewItem.Tag.ToString() == adlOU.Path)
                {
                    treeViewItem.IsSelected = true;
                    return;
                }
                else
                {
                    SelectNodeRecursive(treeViewItem, adlOU);
                }
            }
            return;
        }
        private void ShowUsers(ADLOU adlOU)
        {
            lstUsers.ItemsSource = adlQueryServices.GetAllUsers(adlOU);
            lstUsers.Items.Refresh();
        }
        private void ShowGroups(ADLOU adlOU)
        {
            lstGroups.ItemsSource = adlQueryServices.GetAllGroups(adlOU);
            lstGroups.Items.Refresh();
        }

    }
}
