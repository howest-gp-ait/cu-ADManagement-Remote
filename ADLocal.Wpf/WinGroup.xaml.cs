using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using ADLocal.Core.Entities;
using ADLocal.Core.Services;

namespace ADLocal.Wpf
{
    /// <summary>
    /// Interaction logic for WinGroup.xaml
    /// </summary>
    public partial class WinGroup : Window
    {
        public bool isNew;
        public ADLGroup activeAdlGroup;
        public ADLOU activeAdlOU;
        public bool isRefreshRequired = false;
        private ADLQueryServices adlQueryServices = new ADLQueryServices();
        private ADLCrudServices adlCrudServices = new ADLCrudServices();
        public WinGroup()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (isNew)
            {
                activeAdlGroup = new ADLGroup();
                txtGroupName.Text = "";
            }
            else
            {
                txtGroupName.Text = activeAdlGroup.SamAccountName;
            }
            cmbOUs.ItemsSource = adlQueryServices.GetAllOUs();
            for (int r = 0; r < cmbOUs.Items.Count; r++)
            {
                if (((ADLOU)cmbOUs.Items[r]).Path == activeAdlOU.Path)
                {
                    cmbOUs.SelectedIndex = r;
                    break;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            isRefreshRequired = false;
            ADLOU targetOU = (ADLOU)cmbOUs.SelectedItem;
            string groupName = txtGroupName.Text.Trim();
            if (groupName == "")
            {
                MessageBox.Show("Groepsnaam kan niet leeg zijn !", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (isNew)
            {
                try
                {
                    activeAdlGroup = adlCrudServices.CreateGroup(targetOU, groupName);
                }
                catch (Exception error)
                {
                    MessageBox.Show("Nieuwe groep werden niet aangemaakt !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                try
                {
                    activeAdlGroup = adlCrudServices.UpdateGroup(activeAdlGroup, targetOU, groupName);
                }
                catch (Exception error)
                {
                    MessageBox.Show("Wijzigingen werden niet weggeschreven !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            isRefreshRequired = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            isRefreshRequired = false;
            this.Close();
        }


    }
}
