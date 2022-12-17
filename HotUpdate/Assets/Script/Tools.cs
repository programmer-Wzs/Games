using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace Wu
{
    public class Tools
    {
        // 获取目的网页源码
        public static string GetWebCode(string url)
        {
            try
            {
                string strHTML = "";
                WebClient myWebClient = new WebClient();
                Stream myStream = myWebClient.OpenRead(url);
                StreamReader sr = new StreamReader(myStream, System.Text.Encoding.GetEncoding("utf-8"));
                strHTML = sr.ReadToEnd();
                myStream.Close();
                return strHTML;
            }
            catch
            {
                Debug.LogError("GetWebCode Error");
                return "error";
            }
        }

        // 从远程服务器获取文件，放到本地
        public static bool GetFileByServer(string serviceUrl, string localUrl)
        {
            try
            {
                WebClient client = new WebClient();
                byte[] buffer = client.DownloadData(serviceUrl);
                using (FileStream fileStream = new FileStream(localUrl, FileMode.CreateNew))
                {
                    fileStream.Write(buffer, 0, buffer.Length);
                    fileStream.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("GetFileByServer Error" + ex.Message);
                return false;
            }
        }

    }
}

