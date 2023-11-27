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
using EbestTradeBot.Core;
using EbestTradeBot_V1.ViewModels;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace EbestTradeBot_V1.Views
{
    /// <summary>
    /// ConfigurationWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConfigurationWindow : MetroWindow
    {
        private ConfigurationViewModel _viewModel;
        public ConfigurationWindow()
        {
            InitializeComponent();

            DataContext = new ConfigurationViewModel(AppSettings.Instance);
            _viewModel = (ConfigurationViewModel)DataContext;
            Password.Password = _viewModel.Password;
            CertificationPassword.Password = _viewModel.CertificationPassword;
            AccountPassword.Password = _viewModel.AccountPassword;
        }

        private void Password_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;

            switch (passwordBox.Name)
            {
                case "Password":
                    _viewModel.Password = passwordBox.Password;
                    break;
                case "CertificationPassword":
                    _viewModel.CertificationPassword = passwordBox.Password;
                    break;
                case "AccountPassword":
                    _viewModel.AccountPassword = passwordBox.Password;
                    break;
                    
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveCommand.Execute();
            DialogResult = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            Manager.Instance.XingApi.LoginCompleted -= _viewModel.XingApiLoginCompleted;
            base.OnClosed(e);
        }
    }
}
