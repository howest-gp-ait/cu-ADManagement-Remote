using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ADLocal.Core.Entities;
using ADLocal.Core.Services;

namespace ADLocal.Wpf
{
    /// <summary>
    /// Interaction logic for WinUser.xaml
    /// </summary>
    public partial class WinUser : Window
    {
        public bool? isNew;
        public ADLUser adlActiveUser;
        public ADLOU adlActiveOU;
        public bool isRefreshRequired = false;
        private ADLQueryServices adlQueryServices = new ADLQueryServices();
        private ADLCrudServices adlCrudServices = new ADLCrudServices();


        public WinUser()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbOUs.ItemsSource = adlQueryServices.GetAllOUs();

            for (int r = 0; r < cmbOUs.Items.Count; r++)
            {
                if (((ADLOU)cmbOUs.Items[r]).Path == adlActiveOU.Path)
                {
                    cmbOUs.SelectedIndex = r;
                    break;
                }
            }
            if (isNew == null)
            {
                FillControls();
                grpDetails.IsEnabled = false;
                btnSave.Visibility = Visibility.Hidden;
                btnCancel.Content = "Sluiten";
            }
            else if (isNew == true)
            {
                ClearControls();
                txtPassword.Text = Guid.NewGuid().ToString().ToUpper().Substring(0, 10);
            }
            else
            {
                FillControls();
            }
        }
        private void TxtFirstName_TextChanged(object sender, TextChangedEventArgs e)
        {
            BuildUsername();
        }

        private void TxtLastname_TextChanged(object sender, TextChangedEventArgs e)
        {
            BuildUsername();
        }

        private void RdbExpiresNever_Checked(object sender, RoutedEventArgs e)
        {
            dtpExpirationDate.Visibility = Visibility.Hidden;
        }

        private void RdbExpiresAt_Checked(object sender, RoutedEventArgs e)
        {
            dtpExpirationDate.Visibility = Visibility.Visible;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ADLOU targetOU = (ADLOU)cmbOUs.SelectedItem;
            string firstname = txtFirstName.Text.Trim();
            string lastname = txtLastname.Text.Trim();
            string loginname = txtUserName.Text.Trim();
            if (loginname == "")
            {
                MessageBox.Show("Gebruikersnaam kan niet leeg zijn !", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string password = txtPassword.Text.Trim();
            bool isEnabled = false;
            if (chkEnabled.IsChecked == true)
                isEnabled = true;
            DateTime? accountExpirationDate = null;
            if (rdbExpiresAt.IsChecked == true)
            {
                accountExpirationDate = dtpExpirationDate.SelectedDate;
            }
            if (isNew == true)
            {
                try
                {
                    adlActiveUser = adlCrudServices.CreateUser(targetOU, firstname, lastname, loginname, password, isEnabled, accountExpirationDate);
                }
                catch (Exception error)
                {
                    MessageBox.Show("Nieuwe gebruiker werden niet aangemaakt !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                isRefreshRequired = true;
                System.Threading.Thread.Sleep(1000); // even AD de tijd geven vooraleer we verder doen anders problemen met plaatsen in groepen;
            }
            else
            {
                if (adlCrudServices.UpdateUser(adlActiveUser, targetOU, firstname, lastname, loginname, password, isEnabled, accountExpirationDate))
                {
                    isRefreshRequired = true;
                }
                else
                {
                    MessageBox.Show("Wijzigingen werden niet weggeschreven !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            isRefreshRequired = false;
            this.Close();
        }
        private void ClearControls()
        {
            txtFirstName.Text = "";
            txtLastname.Text = "";
            txtPassword.Text = "";
            txtUserName.Text = "";
            rdbExpiresNever.IsChecked = true;
            rdbExpiresAt.IsChecked = false;
            dtpExpirationDate.Visibility = Visibility.Hidden;
            dtpExpirationDate.SelectedDate = DateTime.Now.AddDays(365);
            chkEnabled.IsChecked = true;
        }
        private void FillControls()
        {
            txtFirstName.Text = adlActiveUser.UserPrincipal.GivenName;
            txtLastname.Text = adlActiveUser.UserPrincipal.Surname;
            txtPassword.Text = "";
            txtUserName.Text = adlActiveUser.UserPrincipal.SamAccountName;
            if (adlActiveUser.UserPrincipal.Enabled == true)
                chkEnabled.IsChecked = true;
            else
                chkEnabled.IsChecked = false;

            if (adlActiveUser.UserPrincipal.AccountExpirationDate == null)
            {
                rdbExpiresNever.IsChecked = true;
                rdbExpiresAt.IsChecked = false;
                dtpExpirationDate.Visibility = Visibility.Hidden;
            }
            else
            {
                rdbExpiresNever.IsChecked = false;
                rdbExpiresAt.IsChecked = true;
                dtpExpirationDate.Visibility = Visibility.Visible;
                dtpExpirationDate.SelectedDate = (DateTime)adlActiveUser.UserPrincipal.AccountExpirationDate;
            }

        }
        private void BuildUsername()
        {
            string fn = txtFirstName.Text.Trim().Replace(" ", ".");
            string ln = txtLastname.Text.Trim().Replace(" ", ".");
            txtUserName.Text = fn + "." + ln;
        }

    }
}
