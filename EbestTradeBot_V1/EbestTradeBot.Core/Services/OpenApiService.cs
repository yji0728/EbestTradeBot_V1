﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EbestTradeBot.Core.EventArgs;
using EbestTradeBot.Core.Helpers;
using EbestTradeBot.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XA_DATASETLib;
using XA_SESSIONLib;
using static System.Runtime.InteropServices.JavaScript.JSType;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace EbestTradeBot.Core.Services
{
    public class OpenApiService
    {
        public Action<string> BoardFunc = null;

        public static bool IsBuyRun = false;
        public static bool IsSellRun = false;
        public static bool IsMarketEnd = true;

        #region ReadOnly
        private string _tokenPath = "/oauth2/token";
        private string _revokePath = "/oauth2/revoke";
        private string _accnoPath = "/stock/accno";
        private string _marketdataPath = "/stock/market-data";
        private string _orderPath = "/stock/order";
        #endregion

        #region Variables

        private XingApiService _xingApiService;

        static object t1305Key = new object();
        static object t1101Key = new object();
        static object CSPAT00601Key = new object();
        #endregion
        
        private string _domain = "https://openapi.ebestsec.co.kr:8080";
        
        private string _token;

        public string Token
        {
            get { return $"Bearer {_token}"; }
            set { _token = value; }
        }
        
        public OpenApiService(XingApiService xingApiService)
        {
            _xingApiService = xingApiService;
            _xingApiService.GetStockCompleted += OnGetStockCompleted;
        }

        private async void OnGetStockCompleted(object? sender, System.EventArgs e)
        {
            try
            {
                List<Stock> stocks = ((StockEventArgs)e).Stocks;

                for (int i = 0; i < stocks.Count; i++)
                {
                    // 매수가 지정
                    if (Manager.Instance.BanStock.Any(x => x.Shcode.Equals(stocks[i].Shcode)) || 
                        Manager.Instance.MyAccount.Any(x => x.Shcode.Equals(stocks[i].Shcode))) continue;

                    SetBuyPrice(stocks[i]);
                    await Task.Delay(AppSettings.Instance.ReplySecond * 1000);
                    if (stocks[i].매수가_1차 == -1 || stocks[i].손절가 == -1 || stocks[i].익절가 == -1 || stocks[i].매수가_2차 == -1) continue;
                    int stockPrice = (await GetCurrentQuote(stocks[i].Shcode)).Price;
                    BoardFunc($"[검색] " +
                              $"[종목코드:{stocks[i].Shcode}] " +
                              $"[종목명:{stocks[i].Hname}] " +
                              $"[1차 매수가:{stocks[i].매수가_1차}] " +
                              $"[2차 매수가:{stocks[i].매수가_2차}] " +
                              $"[손절가:{stocks[i].손절가}] " +
                              $"[익절가:{stocks[i].익절가}]");
                    if (
                        stocks[i].매수가_1차 >= stockPrice && 
                        Manager.Instance.MyAccount.Count < 3 &&
                        !Manager.Instance.MyAccount.Any(x => x.Shcode.Equals(stocks[i].Shcode))
                        )
                    {
                        BuyStock(stocks[i], stockPrice);
                    }
                }
            }
            catch (Exception ex)
            {
                BoardFunc($"[ERROR] " +
                          $"[{ex.Message}] " +
                          $"[{ex.StackTrace}]");
            }
            finally
            {
                IsBuyRun = false;
            }
            
        }
        
        #region private Methods

        private List<Data_t1305> Gett1305Data(JObject jObj)
        {
            List<Data_t1305> ret = new();

            JArray t1305OutBlock1 = (JArray)jObj["t1305OutBlock1"];

            if (t1305OutBlock1 == null) return null;

            bool isFirst = true;
            foreach (var t1305 in t1305OutBlock1)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }
                Data_t1305 t = new Data_t1305
                {
                    Diff = double.Parse(t1305["diff"].ToString()),
                    Open = (int)t1305["open"],
                    Close = (int)t1305["close"],
                    Date = DateTime.ParseExact(t1305["date"].ToString(), "yyyyMMdd", null)
                };
                ret.Add(t);
            }

            return ret.OrderBy(x => x.Date).ToList();
        }

        private void BuyStock(Stock stock, int price)
        {
            lock (CSPAT00601Key)
            {
                int tradePrice = AppSettings.Instance.TradePrice;
                string url = $"{_domain}{_orderPath}";
                string postData;
                Dictionary<string, string> headers = new();

                JObject jObj = null;

                headers["content-type"] = "application/json; charset=utf-8";
                headers["authorization"] = $"{Token}";
                headers["tr_cd"] = "CSPAT00601";
                headers["tr_cont"] = "X";
                headers["tr_cont_key"] = "";
                headers["mac_address"] = "";

                string IsuNo = $"A{stock.Shcode}";  // 종목번호
                int OrdQty = tradePrice / price;    // 주문수량
                double OrdPrc = 0;                  // 주문가
                string BnsTpCode = "2";             // 매매구분
                string OrdprcPtnCode = "03";        // 호가유형코드
                string MgntrnCode = "000";          // 신용거래코드
                string LoanDt = "";                 // 대출일
                string OrdCndiTpCode = "0";         // 주문조건구분

                postData = JsonSerializer.Serialize(new
                {
                    CSPAT00601InBlock1 = new
                    {
                        IsuNo,
                        OrdQty,
                        OrdPrc,
                        BnsTpCode,
                        OrdprcPtnCode,
                        MgntrnCode,
                        LoanDt,
                        OrdCndiTpCode
                    }
                });

                try
                {
                    jObj = HttpService.PostForJson(url, postData, headers);

                    if (jObj["rsp_cd"].ToString().Equals("00040"))
                    {
                        var existingStock = Manager.Instance.MyAccount.FirstOrDefault(x => x.Shcode.Equals(stock.Shcode));

                        if (existingStock != null)
                        {
                            existingStock.보유량 += OrdQty;
                        }
                        else
                        {
                            Manager.Instance.MyAccount.Add(stock);
                        }

                        if (BoardFunc != null)
                        {
                            BoardFunc($"[구매] " +
                                      $"[종목코드:{stock.Shcode}] " +
                                      $"[종목명:{stock.Hname}] " + 
                                      $"[1차 매수가:{stock.매수가_1차}] " + 
                                      $"[2차 매수가:{stock.매수가_2차}] " + 
                                      $"[손절가:{stock.손절가}] " + 
                                      $"[익절가:{stock.익절가}]");
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"[BuyStock] [{e.StackTrace}] [{e.Message}] [{jObj?.ToString()}]");
                }
            }
        }
        private void SellStock(Stock stock)
        {
            lock (CSPAT00601Key)
            {
                int tradePrice = AppSettings.Instance.TradePrice;
                string url = $"{_domain}{_orderPath}";
                string postData;
                Dictionary<string, string> headers = new();

                JObject jObj = null;

                headers["content-type"] = "application/json; charset=utf-8";
                headers["authorization"] = $"{Token}";
                headers["tr_cd"] = "CSPAT00601";
                headers["tr_cont"] = "X";
                headers["tr_cont_key"] = "";
                headers["mac_address"] = "";

                string IsuNo = $"A{stock.Shcode}";  // 종목번호
                int OrdQty = stock.보유량;           // 주문수량
                double OrdPrc = 0;                  // 주문가
                string BnsTpCode = "1";             // 매매구분
                string OrdprcPtnCode = "03";        // 호가유형코드
                string MgntrnCode = "000";          // 신용거래코드
                string LoanDt = "";                 // 대출일
                string OrdCndiTpCode = "0";         // 주문조건구분

                postData = JsonSerializer.Serialize(new
                {
                    CSPAT00601InBlock1 = new
                    {
                        IsuNo,
                        OrdQty,
                        OrdPrc,
                        BnsTpCode,
                        OrdprcPtnCode,
                        MgntrnCode,
                        LoanDt,
                        OrdCndiTpCode
                    }
                });

                try
                {
                    jObj = HttpService.PostForJson(url, postData, headers);

                    if (jObj["rsp_cd"].ToString().Equals("00039"))
                    {
                        Manager.Instance.MyAccount.Remove(stock);
                        if (BoardFunc != null)
                        {
                            BoardFunc($"[판매] " + 
                                      $"[종목코드:{stock.Shcode}] " + 
                                      $"[종목명:{stock.Hname}]");
                        }
                        Helpers.CsvHelper.WriteCsv("TradedStock.csv", new TradedStock{Shcode = stock.Shcode, TradeDate = DateTime.Now});
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"[SellStock] [{e.StackTrace}] [{e.Message}] [{jObj?.ToString()}]");
                }
            }
        }

        private async Task<(int Price, int Uplmtprice, int Dnlmtprice)> GetCurrentQuote(string shcode) // 
        {
            JObject jObj = null;
            try
            {
                lock (t1101Key)
                {
                    string url = $"{_domain}{_marketdataPath}";
                    string postData;
                    Dictionary<string, string> headers = new();

                    

                    headers["content-type"] = "application/json; charset=utf-8";
                    headers["authorization"] = $"{Token}";
                    headers["tr_cd"] = "t1101";
                    headers["tr_cont"] = "X";
                    headers["tr_cont_key"] = "";
                    headers["mac_address"] = "";

                    postData = JsonSerializer.Serialize(new
                    {
                        t1101InBlock = new
                        {
                            shcode
                        }
                    });

                    try
                    {
                        jObj = HttpService.PostForJson(url, postData, headers);

                        return ((int)jObj["t1101OutBlock"]["price"], (int)jObj["t1101OutBlock"]["uplmtprice"], (int)jObj["t1101OutBlock"]["dnlmtprice"]);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"[GetCurrentQuote] [{e.StackTrace}] [{e.Message}] [{jObj?.ToString()}]");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"[SetBuyPrice] [{e.StackTrace}] [{e.Message}] [{jObj?.ToString()}]");
            }
            finally
            {
                await Task.Delay(1000);
            }
            
        }
        #endregion

        #region public Methods
        public void SetBuyPrice(Stock stock)
        {
            lock (t1305Key)
            {
                //List<int> prices = GetPrice(stock, _dayCnt);
                string url = $"{_domain}{_marketdataPath}";
                string postData;
                Dictionary<string, string> headers = new();

                JObject jObj = null;

                headers["content-type"] = "application/json; charset=utf-8";
                headers["authorization"] = $"{Token}";
                headers["tr_cd"] = "t1305";
                headers["tr_cont"] = "X";
                headers["tr_cont_key"] = "";
                headers["mac_address"] = "";

                string shcode = stock.Shcode;
                int dwmcode = 1;
                string date = "";
                int idx = 0;
                int cnt = AppSettings.Instance.DayCount + 1;

                postData = JsonSerializer.Serialize(new
                {
                    t1305InBlock = new
                    {
                        shcode,
                        dwmcode,
                        date,
                        idx,
                        cnt
                    }
                });

                try
                {
                    jObj = HttpService.PostForJson(url, postData, headers);

                    List<Data_t1305> t1305s = Gett1305Data(jObj);

                    CalcHelper.CalcPrice(ref stock, t1305s);
                }
                catch (Exception e)
                {
                    throw new Exception($"[SetBuyPrice] [{e.StackTrace}] [{e.Message}] [{jObj?.ToString()}]");
                }
            }
        }
        public void RevokeToken()
        {
            if (string.IsNullOrEmpty(_token)) return;

            string appKey = AppSettings.Instance.AppKey;
            string secretKey = AppSettings.Instance.SecretKey;

            Dictionary<string, string> headers = new();
            string url = _domain + _revokePath;
            string postData = string.Empty;

            headers["content-type"] = "application/x-www-form-urlencoded";

            string appkey;
            string appsecretkey;
            string token_type_hint;
            string token;

            postData += $"appkey={appKey}&";
            postData += $"appsecretkey={secretKey}&";
            postData += $"token_type_hint=access_token&";
            postData += $"token={_token}";

            try
            {
                JObject jObj = HttpService.PostForJson(url, postData, headers);

                if (jObj["code"].ToString().Equals("200"))
                {
                    Token = "";
                }
                else
                {
                    throw new Exception($"RevokeTokenError [{jObj.ToString()}]");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"[Error] [RevokeToken] [{e.StackTrace}] [{e.Message}] [{e.InnerException?.StackTrace}] [{e.InnerException?.Message}]");
            }
        }

        public void GetToken()
        {
            string appKey = AppSettings.Instance.AppKey;
            string secretKey = AppSettings.Instance.SecretKey;

            // API 엔드포인트 및 요청 데이터 설정
            Dictionary<string, string> headers = new Dictionary<string, string>();

            string url = _domain + _tokenPath;
            string postData = string.Empty;

            string grantType = "client_credentials";
            string scope = "oob";

            try
            {
                // 요청 데이터 구성
                postData += $"appkey={appKey}&";
                postData += $"appsecretkey={secretKey}&";
                postData += $"grant_type={grantType}&";
                postData += $"scope={scope}";

                // header
                headers["content-type"] = "application/x-www-form-urlencoded";

                JObject jObj = HttpService.PostForJson(url, postData, headers);

                _token = jObj["access_token"].ToString();
            }
            catch (Exception e)
            {
                throw new Exception($"[Error] [GetToken] [{e.StackTrace}] [{e.Message}] [{e.InnerException?.StackTrace}] [{e.InnerException?.Message}]");
            }
        }

        public List<Stock> GetStockBalance()
        {
            try
            {
                List<Stock> stocks = new List<Stock>();

                string url = string.Empty;
                string postData = string.Empty;
                Dictionary<string, string> headers = new Dictionary<string, string>();

                url = $"{_domain}{_accnoPath}";

                var jsonRequest = new
                {
                    t0424InBlock = new
                    {
                        prcgb = "",
                        chegb = "",
                        dangb = "",
                        charge = "",
                        cts_expcode = ""
                    }
                };

                postData = JsonConvert.SerializeObject(jsonRequest, Formatting.Indented);

                headers["content-type"] = "application/json; charset=utf-8";
                headers["authorization"] = $"{Token}";
                headers["tr_cd"] = "t0424";
                headers["tr_cont"] = "X";
                headers["tr_cont_key"] = "";
                headers["mac_address"] = "";

                JObject jObj = HttpService.PostForJson(url, postData, headers);
                foreach (JToken jToken in (JArray)jObj["t0424OutBlock1"])
                {
                    Stock stock = new Stock
                    {
                        Shcode = jToken["expcode"].ToString(),
                        Hname = jToken["hname"].ToString(),
                        손절가 = -1,
                        익절가 = -1,
                        매수가_1차 = -1,
                        매수가_2차 = -1,
                        보유량 = (int)jToken["janqty"]
                    };

                    stocks.Add(stock);
                }

                return stocks;
            }
            catch (Exception e)
            {
                throw new Exception($"[Error] [GetToken] [{e.StackTrace}] [{e.Message}] [{e.InnerException?.StackTrace}] [{e.InnerException?.Message}]");
            }

        }
        
        public async Task StartAccountToSellAsync(CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (!IsSellRun)
                    {
                        IsSellRun = true;
                        
                        List<Stock> stocks = new();
                        stocks.AddRange(Manager.Instance.MyAccount);

                        // 장끝나는 시간에 전부매도
                        
                        if (!TimeHelper.IsMarketOpen())
                        {
                            if (!IsMarketEnd)
                            {
                                /*
                                foreach (var stock in stocks)
                                {
                                    SellStock(stock);
                                }
                                */
                                BoardFunc($"[장이 마감되어 판매 모듈을 종료합니다]");
                            }

                            IsMarketEnd = true;
                            continue;
                        }
                        

                        // 계산후 매도
                        foreach (var stock in stocks)
                        {
                            var stockPrice = (await GetCurrentQuote(stock.Shcode));
                            int price = stockPrice.Price;
                            int up = stockPrice.Uplmtprice;
                            int down = stockPrice.Dnlmtprice;

                            if (price > stock.익절가 || price < stock.손절가 || price == up || price == down)
                            {
                                SellStock(stock);
                                Manager.Instance.BanStock.Add(new TradedStock { Shcode = stock.Shcode, TradeDate = DateTime.Now });
                            }
                            await Task.Delay(AppSettings.Instance.ReplySecond * 500);
                        }

                        if (IsMarketEnd)
                        {
                            BoardFunc($"[장이 시작되어 판매 모듈을 시작합니다]");
                            IsMarketEnd = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    BoardFunc($"[ERROR] " +
                              $"[{e.Message}] " +
                              $"[{e.StackTrace}]");
                }
                finally
                {
                    await Task.Delay(AppSettings.Instance.ReplySecond * 1000);
                    IsSellRun = false;
                }
            }
        }
        #endregion

        public async Task StartAccountToSecondBuyAsync(CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var stocks = Manager.Instance.MyAccount;
                    var buyPrice = AppSettings.Instance.TradePrice;

                    foreach (var stock in stocks)
                    {
                        if ((buyPrice * 1.1) - (stock.매수가_1차 * stock.보유량) < 0) continue;

                        var price = (await GetCurrentQuote(stock.Shcode)).Price;
                        if (price <= stock.매수가_2차) BuyStock(stock, price);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
                finally
                {
                    await Task.Delay(AppSettings.Instance.ReplySecond * 1000);
                }
            }
        }
    }
}
