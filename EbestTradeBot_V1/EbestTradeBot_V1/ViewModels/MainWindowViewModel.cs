using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using EbestTradeBot.Core;
using EbestTradeBot.Core.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace EbestTradeBot_V1.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region 멤버변수
        XingApiService _xingApiService = Manager.Instance.XingApi;
        OpenApiService _openApiService = Manager.Instance.OpenApi;
        #endregion

        public event EventHandler Started;
        public event EventHandler Stopped;

        private async void OnStarted()
        {
            CancellationTokenSource = new CancellationTokenSource();
            
            // OpenApi 토큰 발급
            _openApiService.GetToken();

            if (_openApiService.BoardFunc == null)
                _openApiService.BoardFunc = (str) =>
                {
                    AddBoard(str);
                };
            if (_xingApiService.BoardFunc == null)
                _xingApiService.BoardFunc = (str) =>
                {
                    AddBoard(str);
                };

            Manager.Instance.MyAccount = _openApiService.GetStockBalance();

            for (int i = 0; i < Manager.Instance.MyAccount.Count; i++)
            {
                // 매수가 지정
                _openApiService.SetBuyPrice(Manager.Instance.MyAccount[i]);
                AddBoard($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}] " +
                         $"[잔고조회] " +
                         $"[종목코드:{Manager.Instance.MyAccount[i].Shcode}] " + 
                         $"[종목명:{Manager.Instance.MyAccount[i].Hname}] " + 
                         $"[보유량:{Manager.Instance.MyAccount[i].보유량}] " + 
                         $"[매수가:{Manager.Instance.MyAccount[i].매수가}] " + 
                         $"[손절가:{Manager.Instance.MyAccount[i].손절가}] " + 
                         $"[익절가:{Manager.Instance.MyAccount[i].익절가}]");

                await Task.Delay(AppSettings.Instance.ReplySecond * 1000);
            }

            // 구매 모듈 실행
            _xingApiService.StartFindStockToBuyAsync(CancellationTokenSource);

            // 판매 모듈 실행
            _openApiService.StartAccountToSellAsync(CancellationTokenSource);

            Started?.Invoke(this, null);
        }

        private void OnStopped()
        {
            // OpenApi 토큰 폐기
            Manager.Instance.MyAccount.Clear();
            _openApiService.RevokeToken();

            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();

            Stopped?.Invoke(this, null);
        }


        public DelegateCommand RunCommand { get; set; }

        public CancellationTokenSource CancellationTokenSource;

        private bool _isRun;
        public bool IsRun
        {
            get => _isRun;
            set => SetProperty(ref _isRun, value);
        }

        private string _board;

        public string Board
        {
            get
            {
                return _board;
            }
            set
            {
                SetProperty(ref _board, value);
            }
        }

        public MainWindowViewModel()
        {
            CancellationTokenSource = new CancellationTokenSource();
            RunCommand = new DelegateCommand(() => ExecuteRunCommand());

            SetRun(false, true);
        }

        private void SetRun(bool isRun, bool isFirst)
        {
            Board = "";
            IsRun = isRun;

            if (IsRun)          // 종료요청
            {
                OnStarted();
            }
            else if(!isFirst)   // 실행 요청
            {
                OnStopped();
            }
        }

        private void ExecuteRunCommand()
        {
            SetRun(!IsRun, false);
        }

        private void AddBoard(string board)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Board += $"{board}\r\n";
            });
        }
    }
}
