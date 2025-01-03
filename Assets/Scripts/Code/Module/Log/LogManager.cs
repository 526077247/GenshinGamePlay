using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace TaoTie
{
    public class LogManager: IManager
    {
        public static LogManager Instance;
        private InterceptorLogger logger;
        public void Init()
        {
            Instance = this;
            Log.ILog = logger = new InterceptorLogger();
            logger.CheckLv = Define.LogLevel;
            Define.LogLevel = 1;
        }

        public void Destroy()
        {
            Define.LogLevel = logger.CheckLv;
            Log.ILog = new UnityLogger();
            logger = null;
            Instance = null;
        }

        /// <summary>
        /// todo: 上传日志到服务器
        /// </summary>
        public void PushLog2Server()
        {
            
        }
    }
}