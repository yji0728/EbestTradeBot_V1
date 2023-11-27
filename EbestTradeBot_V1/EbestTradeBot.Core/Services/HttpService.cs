using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EbestTradeBot.Core.Services
{
    public abstract class HttpService
    {
        private static CookieContainer _cookieContainer;
        static HttpService()
        {
            _cookieContainer = new CookieContainer();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12;
        }

        #region public Method

        public static JObject GetForJson(string url, Dictionary<string, string> dicHeader)
        {
            using (Stream stream = GetForStream(url, dicHeader))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();
                    JObject jsonResponse = JObject.Parse(content);
                    if (jsonResponse == null)
                    {
                        throw new Exception("[Response Error]");
                    }
                    return jsonResponse;
                }
            }
        }

        public static Stream GetForStream(string url, Dictionary<string, string> dicHeader)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = _cookieContainer;

            // 헤더 추가
            if (dicHeader != null)
            {
                SetHeaders(request, dicHeader);
            }

            return StartResponse(request);
        }

        public static JObject PostForJson(string url, string postData, Dictionary<string, string> dicHeader)
        {
            using (Stream stream = PostForStream(url, postData, dicHeader))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();
                    JObject jsonResponse = JObject.Parse(content);
                    if (jsonResponse == null)
                    {
                        throw new Exception("[Response Error]");
                    }
                    return jsonResponse;
                }
            }
        }

        public static Stream PostForStream(string url, string postData, Dictionary<string, string> dicHeader)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.CookieContainer = _cookieContainer;

            // 헤더 추가
            if (dicHeader != null)
            {
                SetHeaders(request, dicHeader);
            }

            using (Stream requestStream = request.GetRequestStream())
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(postData);
                requestStream.Write(dataBytes, 0, dataBytes.Length);
            }

            return StartResponse(request);
        }
        #endregion

        #region private Method
        private static Stream StartResponse(HttpWebRequest request)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    SaveCookies(response.Cookies);

                    return response.GetResponseStream();
                }
                else
                {
                    throw new Exception($"Response Error");
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private static void SetHeaders(HttpWebRequest request, Dictionary<string, string> dicHeader)
        {
            request.Headers.Clear();

            foreach (var header in dicHeader)
            {
                switch (header.Key.ToLower())
                {
                    case "accept":
                        request.Accept = header.Value;
                        break;
                    case "connection":
                        if (header.Value.ToLower().Equals("keep-alive"))
                            request.KeepAlive = true;
                        else
                            request.Connection = header.Value;
                        break;
                    case "content-type":
                        request.ContentType = header.Value;
                        break;
                    case "user-agent":
                        request.UserAgent = header.Value;
                        break;
                    default:
                        request.Headers.Add(header.Key, header.Value);
                        break;
                }
            }
        }

        private static void SaveCookies(CookieCollection cookies)
        {
            foreach (Cookie cookie in cookies)
            {
                _cookieContainer.Add(cookie);
            }
        }
        #endregion
    }
}
