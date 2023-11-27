using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EbestTradeBot.Core;
using EbestTradeBot.Core.EventArgs;
using EbestTradeBot.Core.Services;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;

namespace EbestTradeBot_V1.ViewModels
{
    public class ConfigurationViewModel : BindableBase
    {
        #region Variable
        private readonly XingApiService _xingApiService = Manager.Instance.XingApi;
        #endregion

        #region DelegateCommand
        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand LoginXingApiCommand { get; private set; }
        public DelegateCommand FileDialogCommand { get; private set; }
        #endregion

        #region Property
        private string _id;
        public string Id
        { 
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        private string _certificationPassword;

        public string CertificationPassword
        {
            get { return _certificationPassword; }
            set { SetProperty(ref _certificationPassword, value); }
        }

        private string _appKey;
        public string AppKey
        {
            get { return _appKey; }
            set { SetProperty(ref _appKey, value); }
        }

        private string _secretKey;
        public string SecretKey
        {
            get { return _secretKey; }
            set { SetProperty(ref _secretKey, value); }
        }

        private string _accountNumber;
        public string AccountNumber
        {
            get { return _accountNumber; }
            set { SetProperty(ref _accountNumber, value); }
        }

        private string _accountPassword;
        public string AccountPassword
        {
            get { return _accountPassword; }
            set { SetProperty(ref _accountPassword, value); }
        }

        private string _acfFilePath;
        public string AcfFilePath
        {
            get { return _acfFilePath; }
            set { SetProperty(ref _acfFilePath, value); }
        }

        private int _tradePrice;
        public int TradePrice
        {
            get { return _tradePrice; }
            set { SetProperty(ref _tradePrice, value); }
        }

        private bool _isTestTrade;
        public bool IsTestTrade
        {
            get { return _isTestTrade; }
            set { SetProperty(ref _isTestTrade, value); }
        }

        private int _dayCount;

        public int DayCount
        {
            get => _dayCount;
            set => SetProperty(ref _dayCount, value);
        }


        private int _replySecond;

        public int ReplySecond
        {
            get => _replySecond;
            set => SetProperty(ref _replySecond, value);
        }
        #endregion

        #region Constructor
        public ConfigurationViewModel(AppSettings appSettings) : this()
        {
            Id = appSettings.Id;
            Password = appSettings.Password;
            CertificationPassword = appSettings.CertificationPassword;
            AppKey = appSettings.AppKey;
            SecretKey = appSettings.SecretKey;
            AccountNumber = appSettings.AccountNumber;
            AccountPassword = appSettings.AccountPassword;
            AcfFilePath = appSettings.AcfFilePath;
            TradePrice = appSettings.TradePrice;
            IsTestTrade = appSettings.IsTestTrade;
            DayCount = appSettings.DayCount;
            ReplySecond = appSettings.ReplySecond;
        }
        private ConfigurationViewModel()
        {
            SaveCommand = new DelegateCommand(() => ExecuteSaveCommand());
            LoginXingApiCommand = new DelegateCommand(ExecuteLoginXingApiCommand);
            FileDialogCommand = new DelegateCommand(ExecuteFileDialogCommand);

            _xingApiService.LoginCompleted -= XingApiLoginCompleted;
            _xingApiService.LoginCompleted += XingApiLoginCompleted;
        }


        public void XingApiLoginCompleted(object? sender, EventArgs e)
        {
            LoginEventArgs args = (LoginEventArgs)e;

            switch (args.Code)
            {
                case "0000":    // 성공
                    MessageBox.Show($"XingAPI 로그인 성공 [{args.Code}] [{args.Message}]");
                    break;
                default:
                    MessageBox.Show($"XingAPI 로그인 실패 [{args.Code}] [{args.Message}]");
                    break;
            }
        }

        #endregion

        #region DelegateCommand Execute

        private void ExecuteFileDialogCommand()
        {
            OpenFileDialog ofd = new();
            ofd.Filter = "ACF files (*.acf)|*.acf|All files (*.*)|*.*";
            if (ofd.ShowDialog() is not null || (bool)ofd.ShowDialog())
            {
                AcfFilePath = ofd.FileName;
            }
        }

        private void ExecuteLoginOpenApiCommand()
        {
            throw new NotImplementedException();
        }

        private void ExecuteLoginXingApiCommand()
        {
            _xingApiService.Login();
        }

        private void ExecuteSaveCommand()
        {
            AppSettings appSettings = new AppSettings();
            appSettings.Id = Id;
            appSettings.Password = Password;
            appSettings.CertificationPassword = CertificationPassword;
            appSettings.AppKey = AppKey;
            appSettings.SecretKey = SecretKey;
            appSettings.AccountNumber = AccountNumber;
            appSettings.AccountPassword = AccountPassword;
            appSettings.AcfFilePath = AcfFilePath;
            appSettings.TradePrice = TradePrice;
            appSettings.IsTestTrade = IsTestTrade;
            appSettings.DayCount = DayCount;
            appSettings.ReplySecond = ReplySecond;

            AppSettings.Instance.Set(appSettings);
        }
        #endregion
    }
}
