using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbestTradeBot.Core.EventArgs;
using EbestTradeBot.Core.Helpers;
using EbestTradeBot.Core.Models;
using XA_DATASETLib;
using XA_SESSIONLib;

namespace EbestTradeBot.Core.Services
{
    public class XingApiService
    {
        public Action<string> BoardFunc = null;

        private bool _isLogin = false;
        private static bool IsMarketEnd = true;
        private bool _currentIsTestTrade;

        #region events
        public event EventHandler LoginCompleted;
        private void OnLoginCompleted(string code, string message)
        {
            switch (code)
            {
                case "0000":
                    _isLogin = true;
                    break;
                default:
                    _isLogin = false;
                    break;
            }
            LoginCompleted?.Invoke(this, new LoginEventArgs(code, message));
        }

        public event EventHandler GetStockCompleted;

        private void OnGetStockCompleted(List<Stock> stocks)
        {
            GetStockCompleted?.Invoke(this, new StockEventArgs(stocks));
        }
        #endregion

        private XASession _xaSession;
        private XAQuery _xaQuery_t1857;
        private AppSettings _appSettings = AppSettings.Instance;

        public XingApiService()
        {
            _xaSession = new XASession();

            ((_IXASessionEvents_Event)_xaSession).Disconnect += XaSessionOnDisconnect;
            ((_IXASessionEvents_Event)_xaSession).Login += OnLogin;
            ((_IXASessionEvents_Event)_xaSession).Logout += OnLogout;

            _xaQuery_t1857 = new XAQuery();
            ((_IXAQueryEvents_Event)_xaQuery_t1857).ReceiveData += _xaQuery_t1857_OnReceiveData;
            _xaQuery_t1857.ResFileName = @"Res\t1857.res";
        }
        #region public Methods

        public void Login(string id, string pw, string certPw, bool isTestTrade)
        {
            if (!_xaSession.IsConnected() || _currentIsTestTrade != isTestTrade)
            {
                Connect(isTestTrade);
            }

            if (!_xaSession.Login(id, pw, certPw, 0, true))
            {
                OnLoginCompleted("0", "로그인 실패");
            }
        }

        public void Connect(bool isTestTrade)
        {
            _currentIsTestTrade = isTestTrade;
            if (isTestTrade)
            {
                _xaSession.ConnectServer("demo.ebestsec.co.kr", 20001);

            }
            else
            {
                _xaSession.ConnectServer("hts.ebestsec.co.kr", 20001);
            }
        }
        #endregion

        #region private Methods

        private void Request_t1857()
        {
            if (!_xaSession.IsConnected())
                Login(AppSettings.Instance.Id, AppSettings.Instance.Password, AppSettings.Instance.CertificationPassword, AppSettings.Instance.IsTestTrade);

            if (_isLogin)
            {
                OpenApiService.IsBuyRun = true;
                string acfFile = AppSettings.Instance.AcfFilePath;

                _xaQuery_t1857.SetFieldData("t1857InBlock", "sRealFlag", 0, "0");
                _xaQuery_t1857.SetFieldData("t1857InBlock", "sSearchFlag", 0, "F");
                _xaQuery_t1857.SetFieldData("t1857InBlock", "query_index", 0, acfFile);

                int nSuccess = _xaQuery_t1857.RequestService("t1857", "");
                if (nSuccess < 0)
                {
                    throw new Exception($"[{nSuccess}] [{_xaSession.GetErrorMessage(nSuccess)}] [t1857 Request]");
                }
            }
            else
            {
                OpenApiService.IsBuyRun = false;
            }
        }

        private void _xaQuery_t1857_OnReceiveData(string sztrcode)
        {
            List<Stock> stocks = new();

            int count = -1;
            if (!int.TryParse(_xaQuery_t1857.GetFieldData("t1857OutBlock", "result_count", 0), out count))
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                string shcode = _xaQuery_t1857.GetFieldData("t1857OutBlock1", "shcode", i);
                string hname = _xaQuery_t1857.GetFieldData("t1857OutBlock1", "hname", i);

                Stock stock = new ();
                stock.Shcode = shcode;
                stock.Hname = hname;

                stocks.Add(stock);
            }

            //_buyList.Add(new StockInfo(){Hname = "인포바인", Shcode = "115310"});
            if (stocks.Count > 0)
            {
                OnGetStockCompleted(stocks);
            }
        }
        #endregion

        #region Event Handler
        private void OnLogout()
        {
            _xaSession.DisconnectServer();
        }

        private void OnLogin(string szcode, string szmsg)
        {
            OnLoginCompleted(szcode, szmsg);
        }

        private void XaSessionOnDisconnect()
        {
            _isLogin = false;
        }
        #endregion

        public async Task StartFindStockToFirstBuyAsync(CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (!TimeHelper.IsMarketOpen())
                    {
                        if (!IsMarketEnd)
                        {
                            BoardFunc($"[장이 마감되어 구매 모듈을 종료합니다]");
                        }

                        IsMarketEnd = true;
                        continue;
                    }

                    if (!OpenApiService.IsBuyRun)
                    {
                        Request_t1857();
                    }

                    if (IsMarketEnd)
                    {
                        BoardFunc($"[장이 시작되어 구매 모듈을 시작합니다]");
                        IsMarketEnd = false;

                        DateTime eDate = DateTime.Now;
                        DateTime sDate = DateTime.Now.Date.AddDays(_appSettings.CooldownDay * -1);
                        Manager.Instance.BanStock = Helpers.CsvHelper.ReadTradedStockCsv($"TradedStock.csv").Where(x => x.TradeDate >= sDate && x.TradeDate <= eDate)
                            .ToList();
                    }
                }
                catch (Exception e)
                {
                    BoardFunc($"[ERROR] " +
                              $"[{e.Message}] " +
                              $"[{e.StackTrace}]");

                    OpenApiService.IsBuyRun = false;
                }
                finally
                {
                    await Task.Delay(4000);
                }
            }
        }
    }
}
