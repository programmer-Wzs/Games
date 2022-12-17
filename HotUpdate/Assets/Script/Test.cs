using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;


// ����
public class Test : MonoBehaviour
{

    void Start()
    {
        /*StartCoroutine(GetAssetBundle());
        Debug.Log(GetWebCode("http://localhost/abList.txt"));
        GetFileByServer("http://localhost/image", "image");*/
        //Debug.Log(Application.streamingAssetsPath + Wu.Data.abListName);

        Wu.HotUpdate hotUpdate = new Wu.HotUpdate();
        hotUpdate.UpdateLocalAB((nowCount, maxCount)=> {
            Debug.Log(nowCount.ToString() + "/" + maxCount);
        });
    }

    IEnumerator GetAssetBundle()
    {
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(@"http://localhost/image");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            
        }
    }


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
            return "�޷���ȡ";
        }
    }

    // ��Զ�̷�������ȡ�ļ����ŵ�����
    private static bool GetFileByServer(string serviceUrl, string localUrl)
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
        catch(Exception ex)
        {
            Debug.LogError("GetFileByServer Error" + ex.Message);
            return false;
        }   
    }
}
