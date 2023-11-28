using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Converters;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using Formatting = Newtonsoft.Json.Formatting;
using EbestTradeBot.Core.Models;
using Microsoft.Win32;

namespace EbestTradeBot.Core
{
    public class AppSettings
    {
        private static readonly Lazy<AppSettings> _instance = new Lazy<AppSettings>(() => new AppSettings());
        public static AppSettings Instance => _instance.Value;

        public string Id { get; set; }                      // 이베스트 투자증권 ID
        public string Password { get; set; }                // 이베스트 투자증권 PW
        public string CertificationPassword { get; set; }   // 공인인증서 PW
        public string AppKey { get; set; }                  // 앱키
        public string SecretKey { get; set; }               // 시크릿키
        public string AccountNumber { get; set; }           // 계좌번호
        public string AccountPassword { get; set; }         // 계좌 비밀번호
        public string AcfFilePath { get; set; }             // .acf 파일 경로
        public int TradePrice { get; set; }                 // 거래 금액
        public bool IsTestTrade { get; set; }               // 모의투자 여부
        public int DayCount { get; set; }
        public Theme Theme { get; set; }
        public int ReplySecond { get; set; }
        public int CooldownDay { get; set; }

        public AppSettings()
        {
            try
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();

                GetAppSettingForConfig(configuration);
            }
            catch (Exception e)
            {
                InitAppSettings();
            }

            // Theme
            if (IsWindowsThemeDarkMode()) // 다크모드
            {
                Theme = Theme.Dark;
            }
            else
            {
                Theme = Theme.Light;
            }

            SaveToJson(this);
        }

        public void Set(AppSettings appSettings)
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
            DayCount = appSettings.DayCount;
            IsTestTrade = appSettings.IsTestTrade;
            ReplySecond = appSettings.ReplySecond;
            CooldownDay = appSettings.CooldownDay;

            SaveToJson(this);
        }

        private bool IsWindowsThemeDarkMode()
        {
            const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string RegistryValueName = "AppsUseLightTheme";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                if (key != null)
                {
                    object value = key.GetValue(RegistryValueName);
                    if (value is int intValue)
                    {
                        return intValue == 0;
                    }
                }
            }

            // 기본적으로 Light 모드로 처리
            return false;
        }

        private void InitAppSettings()
        {
            Id = "";
            Password = "";
            CertificationPassword = "";
            AppKey = "";
            SecretKey = "";
            AccountNumber = "";
            AccountPassword = "";
            AcfFilePath = "";
            DayCount = 0;
            TradePrice = 0;
            IsTestTrade = true;
            ReplySecond = 0;
            CooldownDay = 0;
        }

        private void GetAppSettingForConfig(IConfiguration configuration)
        {
            string id = configuration["Id"];
            if (!string.IsNullOrEmpty(id)) Id = id;
            else Id = string.Empty;

            string password = configuration["Password"];
            if (!string.IsNullOrEmpty(password)) Password = password;
            else Password = string.Empty;

            string certificationPassword = configuration["CertificationPassword"];
            if (!string.IsNullOrEmpty(certificationPassword)) CertificationPassword = certificationPassword;
            else CertificationPassword = string.Empty;

            string appKey = configuration["AppKey"];
            if (!string.IsNullOrEmpty(appKey)) AppKey = appKey;
            else AppKey = string.Empty;

            string secretKey = configuration["SecretKey"];
            if (!string.IsNullOrEmpty(secretKey)) SecretKey = secretKey;
            else SecretKey = string.Empty;

            string accountNumber = configuration["AccountNumber"];
            if (!string.IsNullOrEmpty(accountNumber)) AccountNumber = accountNumber;
            else AccountNumber = string.Empty;
            
            string accountPassword = configuration["AccountPassword"];
            if (!string.IsNullOrEmpty(accountPassword)) AccountPassword = accountPassword;
            else AccountPassword = string.Empty;
            
            string acfFilePath = configuration["AcfFilePath"];
            if (!string.IsNullOrEmpty(acfFilePath)) AcfFilePath = acfFilePath;
            else AcfFilePath = string.Empty;
            
            int tradePrice;
            if (int.TryParse(configuration["TradePrice"], out tradePrice)) TradePrice = tradePrice;
            else TradePrice = 0;

            bool isTestTrade;
            if (bool.TryParse(configuration["IsTestTrade"], out isTestTrade)) IsTestTrade = isTestTrade;
            else IsTestTrade = false;

            int dayCount;
            if (int.TryParse(configuration["DayCount"], out dayCount)) DayCount = dayCount;
            else DayCount = 0;

            int replySecond;
            if (int.TryParse(configuration["ReplySecond"], out replySecond)) ReplySecond = replySecond;
            else ReplySecond = 0;

            int cooldownDay;
            if (int.TryParse(configuration["CooldownDay"], out cooldownDay)) CooldownDay = cooldownDay;
            else CooldownDay = 0;
        }

        private void SaveToJson(AppSettings appSettings)
        {
            if (!File.Exists("appsettings.json"))
            {
                using (File.Create("appsettings.json")) ;
            }

            // 변경된 설정 값을 appsettings.json 파일에 저장
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                Formatting = Formatting.Indented
            };
            string updatedJson = Newtonsoft.Json.JsonConvert.SerializeObject(appSettings, settings);

            File.WriteAllText("appsettings.json", updatedJson);
        }
    }
}
