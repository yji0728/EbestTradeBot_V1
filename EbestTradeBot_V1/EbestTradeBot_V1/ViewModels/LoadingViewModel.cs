using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using System.Windows;
using EbestTradeBot.Core;
using EbestTradeBot.Core.Services;
using Prism.Mvvm;

namespace EbestTradeBot_V1.ViewModels
{
    public class LoadingViewModel : BindableBase
    {
        #region Variables
        AppSettings _appSettings = AppSettings.Instance;

        private OpenApiService _openApi;
        private XingApiService _xingApi;
        #endregion

        #region event
        public event EventHandler InitCompleted;

        private void OnInitCompleted()
        {
            InitCompleted?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region DelegateCommand

        public DelegateCommand InitCommand { get; private set; }

        private void ExecuteInitCommand()
        {
            Init();
            OnInitCompleted();
        }

        private void Init()
        {
            // STEP 0 :: appsettings.json 파일 읽기 -> 없으면 바로 종료
            Status1 = "appsettings.json 파일을 읽는 중입니다.";

            // STEP 1 :: appkey, secretkey가 존재할 경우 openAPI 연결
            if (!string.IsNullOrEmpty(_appSettings.AppKey) && !string.IsNullOrEmpty(_appSettings.SecretKey))
            {
                Status1 = "OpenApi 초기세팅 중입니다.";
                _openApi = Manager.Instance.OpenApi;
            }

            // STEP 2 :: ID, PW, CertPW가 존재할 경우 xingAPI연결
            if (!string.IsNullOrEmpty(_appSettings.Id) && !string.IsNullOrEmpty(_appSettings.Password) &&
                !string.IsNullOrEmpty(_appSettings.CertificationPassword))
            {
                Status1 = "XingApi 서버와 연결 중입니다.";
                _xingApi = Manager.Instance.XingApi;
                _xingApi.Connect(AppSettings.Instance.IsTestTrade);
            }
        }

        #endregion

        #region Properties
        private string _status2;

        public string Status2
        {
            get
            {
                return _status2;
            }
            set
            {
                SetProperty(ref _status2, value);
            }
        }

        private string _status1;

        public string Status1
        {
            get
            {
                return _status1;
            }
            set
            {
                SetProperty(ref _status1, value);
            }
        }

        private string _title;

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                SetProperty(ref _title, value);
            }
        }
        #endregion

        public LoadingViewModel()
        {
            Title = "로딩";

            InitCommand = new DelegateCommand(ExecuteInitCommand);
        }
    }
}
