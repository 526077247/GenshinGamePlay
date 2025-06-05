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
        private AcceptAllCertificate certificateHandler = new AcceptAllCertificate();
        public static HttpManager Instance { get; } = new HttpManager();
        private string persistentDataPath = Application.persistentDataPath;
        
        
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
            Dictionary<string, object> param = null, int timeout = DEFAULT_TIMEOUT)
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

        private UnityWebRequest HttpGetImageOnlineInner(string url, Dictionary<string, string> headers = null,
            int timeout = DEFAULT_TIMEOUT*2)
        {
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
        
        public async ETTask<Texture2D> HttpGetImageOnline(string url, bool local, Dictionary<string, string> headers = null,
            int timeout = DEFAULT_TIMEOUT,ETCancellationToken cancelToken = null)
        {
            //本地是否存在图片
            if (local)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                url = "file://" + LocalFile(url);
#else
                return null;
#endif
            }
            var op = HttpGetImageOnlineInner(url, headers, timeout);
            while (!op.isDone)
            {
                if (cancelToken.IsCancel())
                {
                    op.Abort();
                    break;
                }
                await TimerManager.Instance.WaitAsync(1,cancelToken);
            }
            if (op.result == UnityWebRequest.Result.Success)//本地已经存在
            {
                var texture = DownloadHandlerTexture.GetContent(op);
                op.Dispose();
                return texture;
            }
            else
            {
                if(!local)
                    Log.Error(string.Format("url {0} get fail. msg : {1}",url, op.error));
                op.Dispose();
                return null;
            }
        }
        
        private UnityWebRequest HttpGetSoundOnlineInner(string url,AudioType type, Dictionary<string, string> headers = null,
            int timeout = DEFAULT_TIMEOUT*2)
        {
            var request = UnityWebRequestMultimedia.GetAudioClip(url, type);
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
        private AudioType GetAudioType(string extension)
        {
            switch (extension)
            {
                case ".mp3":
                case ".mp2":
                    return AudioType.MPEG;
                case ".wav":
                    return AudioType.WAV;
                case ".ogg":
                    return AudioType.OGGVORBIS;
                case ".aiff":
                    return AudioType.AIFF;
                case ".it":
                    return AudioType.IT;
                case ".mod":
                    return AudioType.MOD;
                case ".s3m":
                    return AudioType.S3M;
                case ".xm":
                    return AudioType.XM;
                case ".xma":
                    return AudioType.XMA;
                case ".vag":
                    return AudioType.VAG;
                case ".acc":
                    return AudioType.ACC;
                default:
                    return AudioType.UNKNOWN;
            }
        }
        public async ETTask<AudioClip> HttpGetSoundOnline(string url, bool local, Dictionary<string, string> headers = null,
            int timeout = DEFAULT_TIMEOUT,ETCancellationToken cancelToken = null)
        {
            //本地是否存在图片
            if (local)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                url = "file://" + LocalFile(url, "downloadSound", ".wav");
#else
                return null;
#endif
            }
            string extension = Path.GetExtension(url).ToLower();
            var op = HttpGetSoundOnlineInner(url,GetAudioType(local?".wav": extension), headers, timeout);
            while (!op.isDone)
            {
                if (cancelToken.IsCancel())
                {
                    op.Abort();
                    break;
                }
                await TimerManager.Instance.WaitAsync(1,cancelToken);
            }
            if (op.result == UnityWebRequest.Result.Success)//本地已经存在
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(op);
                clip.name = url;
                op.Dispose();
                return clip;
            }
            else
            {
                if(!local)
                    Log.Error(string.Format("url {0} get fail. msg : {1}",url, op.error));
                op.Dispose();
                return null;
            }
        }

        public async ETTask<string> HttpGetResult(string url, Dictionary<string, string> headers = null,
            Dictionary<string, string> param = null, int timeout = DEFAULT_TIMEOUT,ETCancellationToken cancelToken = null)
        {
            var op = HttpGet(url, headers, param, timeout);
            while (!op.isDone)
            {
                if (cancelToken.IsCancel())
                {
                    op.Abort();
                    break;
                }
                await TimerManager.Instance.WaitAsync(1,cancelToken);
            }

            if (op.result == UnityWebRequest.Result.Success)
            {
                var res = op.downloadHandler.text.Replace("\"", "");
                op.Dispose();
                return res;
            }
            else
            {
                Log.Info(string.Format("url {0} get fail. msg : {1}",url, op.error));
                op.Dispose();
                return null;
            }

        }

        public async ETTask<T> HttpGetResult<T>(string url, Dictionary<string, string> headers = null,
            Dictionary<string, string> param = null, int timeout = DEFAULT_TIMEOUT,ETCancellationToken cancelToken = null) where T : class
        {
            var op = HttpGet(url, headers, param, timeout);
            while (!op.isDone)
            {
                if (cancelToken.IsCancel())
                {
                    op.Abort();
                    break;
                }
                await TimerManager.Instance.WaitAsync(1,cancelToken);
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
                finally
                {
                    op.Dispose();
                }
            }
            else
            {
                Log.Info(string.Format("url {0} get fail. msg : {1}",url, op.error));
                op.Dispose();
                return null;
            }
        }

        public async ETTask<T> HttpPostResult<T>(string url, Dictionary<string, string> headers = null,
            Dictionary<string, object> param = null, int timeout = DEFAULT_TIMEOUT,ETCancellationToken cancelToken = null) where T : class
        {
            var op = HttpPost(url, headers, param, timeout);
            while (!op.isDone)
            {
                if (cancelToken.IsCancel())
                {
                    op.Abort();
                    break;
                }
                await TimerManager.Instance.WaitAsync(1,cancelToken);
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
                finally
                {
                    op.Dispose();
                }
            }
            else
            {
                Log.Info(string.Format("url {0} get fail. msg : {1}",url, op.error));
                op.Dispose();
                return null;
            }
        }
        public string LocalFile(string url,string dir = "downloadimage",string extends = ".png")
        {
            byte[] input = Encoding.Default.GetBytes(url.Trim());
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(input);
            string md5URLString = BitConverter.ToString(output).Replace("-", "");
            string path =  $"{persistentDataPath}/{dir}/";
            CheckDirAndCreateWhenNeeded(path);
            string savePath = persistentDataPath + $"/{dir}/" + md5URLString + $"{extends}";
            //Log.Info("=======savePath:" + savePath);
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