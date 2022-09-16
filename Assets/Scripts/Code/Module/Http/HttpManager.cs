using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace TaoTie
{

    public class HttpManager
    {
        const int DEFAULT_TIMEOUT = 10; // 默认超时时间
        public static AcceptAllCertificate certificateHandler = new AcceptAllCertificate();
        public static HttpManager Instance { get; private set; } = new HttpManager();

        public UnityWebRequest HttpGet(string url, Dictionary<string, string> headers = null,
            Dictionary<string, string> param = null, int timeout = DEFAULT_TIMEOUT)
        {
            string strParam = ConvertParamToStr(param);
            if (!string.IsNullOrEmpty(strParam))
                url += "?" + strParam;
            var request = UnityWebRequest.Get(url);
            request.certificateHandler = certificateHandler;
            if (timeout > 0)
            {
                request.timeout = timeout;
            }

            if (headers != null)
                foreach (var item in headers)
                {
                    request.SetRequestHeader(item.Key, item.Value);
                }

            request.SendWebRequest();
            return request;
        }

        public UnityWebRequest HttpPost(string url, Dictionary<string, string> headers = null,
            Dictionary<string, string> param = null, int timeout = DEFAULT_TIMEOUT)
        {
            byte[] postBytes = System.Text.Encoding.Default.GetBytes(JsonHelper.ToJson(param));
            var request = new UnityWebRequest(url, "POST");
            request.certificateHandler = certificateHandler;
            request.uploadHandler = (UploadHandler) new UploadHandlerRaw(postBytes);
            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            if (headers != null)
                foreach (var item in headers)
                {
                    request.SetRequestHeader(item.Key, item.Value);
                }

            if (timeout > 0)
            {
                request.timeout = timeout;
            }

            request.SendWebRequest();
            return request;
        }

        public UnityWebRequest HttpPostUrl(string url, Dictionary<string, string> headers = null,
            Dictionary<string, string> param = null, int timeout = DEFAULT_TIMEOUT)
        {
            string strParam = ConvertParamToStr(param);
            var request = new UnityWebRequest(url + "?" + strParam, "POST");
            request.certificateHandler = certificateHandler;
            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            if (headers != null)
                foreach (var item in headers)
                {
                    request.SetRequestHeader(item.Key, item.Value);
                }

            if (timeout > 0)
            {
                request.timeout = timeout;
            }

            request.SendWebRequest();
            return request;
        }

        public UnityWebRequest HttpPutUrl(string url, Dictionary<string, string> headers = null,
            Dictionary<string, string> param = null, int timeout = DEFAULT_TIMEOUT)
        {
            string strParam = ConvertParamToStr(param);
            var request = new UnityWebRequest(url + "?" + strParam, "PUT");
            request.certificateHandler = certificateHandler;
            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            if (headers != null)
                foreach (var item in headers)
                {
                    request.SetRequestHeader(item.Key, item.Value);
                }

            if (timeout > 0)
            {
                request.timeout = timeout;
            }

            request.SendWebRequest();
            return request;
        }

        public UnityWebRequest HttpGetImageOnline(string url, bool local, Dictionary<string, string> headers = null,
            int timeout = DEFAULT_TIMEOUT)
        {
            //本地是否存在图片
            if (local)
            {
                url = LocalImage(url);
            }

            var request = UnityWebRequestTexture.GetTexture(url);
            request.certificateHandler = certificateHandler;
            if (timeout > 0)
            {
                request.timeout = timeout;
            }

            if (headers != null)
                foreach (var item in headers)
                {
                    request.SetRequestHeader(item.Key, item.Value);
                }

            request.SendWebRequest();
            return request;
        }

        public async ETTask<string> HttpGetResult(string url, Dictionary<string, string> headers = null,
            Dictionary<string, string> param = null, int timeout = DEFAULT_TIMEOUT)
        {
            var op = HttpGet(url, headers, param, timeout);
            while (!op.isDone)
            {
                await TimerManager.Instance.WaitAsync(1);
            }

            if (op.result == UnityWebRequest.Result.Success)
                return op.downloadHandler.text.Replace("\"", "");
            else
            {
                Log.Info(string.Format("url {0} get fail. msg : {1}",url, op.error));
                return null;
            }

        }

        public async ETTask<T> HttpGetResult<T>(string url, Dictionary<string, string> headers = null,
            Dictionary<string, string> param = null, int timeout = DEFAULT_TIMEOUT) where T : class
        {
            var op = HttpGet(url, headers, param, timeout);
            while (!op.isDone)
            {
                await TimerManager.Instance.WaitAsync(1);
            }

            if (op.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    return JsonHelper.FromJson<T>(op.downloadHandler.text);
                }
                catch
                {
                    Log.Error("json.encode error:\n" + op.downloadHandler.text);
                    return null;
                }
            }
            else
            {
                Log.Info(string.Format("url {0} get fail. msg : {1}",url, op.error));
                return null;
            }
        }

        public string LocalImage(string url)
        {
            byte[] input = Encoding.Default.GetBytes(url.Trim());
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(input);
            string md5_url_string = BitConverter.ToString(output).Replace("-", "");
            string path = Application.persistentDataPath + "/downloadimage/";
            CheckDirAndCreateWhenNeeded(path);
            string savePath = Application.persistentDataPath + "/downloadimage/" + md5_url_string + ".png";
            //Debug.LogError("=======savePath:" + savePath);
            return savePath;
        }
        public static void CheckDirAndCreateWhenNeeded(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }
        private string ConvertParamToStr(Dictionary<string, string> param)
        {
            if (param == null) return "";
            StringBuilder builder = new StringBuilder();
            int flag = 0;
            foreach (var item in param)
            {
                if (flag == 0)
                {
                    builder.Append(item.Key + "=" + item.Value);
                    flag = 1;
                }
                else
                {
                    builder.Append("&" + item.Key + "=" + item.Value);
                }
            }

            return builder.ToString();
        }
    }
}