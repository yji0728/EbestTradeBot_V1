using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using EbestTradeBot.Core.Models;
using EbestTradeBot.Core.Services;

namespace EbestTradeBot.Core
{
    public class Manager
    {
        #region Instance
        private static readonly Lazy<Manager> _instance = new Lazy<Manager>(() => new Manager());
        public static Manager Instance => _instance.Value;
        #endregion

        #region Services
        private OpenApiService _openApiService;

        public OpenApiService OpenApi
        {
            get
            {
                return _openApiService;
            }
            private set
            {
                _openApiService = value;
            }
        }

        private XingApiService _xingApiService;

        public XingApiService XingApi
        {
            get
            {
                return _xingApiService;
            }
            private set
            {
                _xingApiService = value;
            }
        }
        #endregion

        #region Properties
        private List<Stock> _myAccount;

        public List<Stock> MyAccount
        {
            get => _myAccount;
            set => _myAccount = value;
        }

        private List<TradedStock> _banStock;

        public List<TradedStock> BanStock
        {
            get => _banStock;
            set => _banStock = value;
        }

        private CancellationTokenSource _cancellationTokenSource;

        public CancellationTokenSource CancellationTokenSource
        {
            get
            {
                return _cancellationTokenSource;
            }
            private set
            {
                _cancellationTokenSource = value;
            }
        }

        private AppSettings _appSetting;

        public AppSettings AppSetting
        {
            get
            {
                return _appSetting;
            }
            private set
            {
                _appSetting = value;
            }
        }
        #endregion

        private Manager()
        {
            XingApi = new XingApiService();
            OpenApi = new OpenApiService(XingApi);
            MyAccount = new();

            CancellationTokenSource = new CancellationTokenSource();
        }
    }
}
