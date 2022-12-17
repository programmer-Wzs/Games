using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// ��ȡԶ�̷���������Դ�Ա��ļ���abList.txt��
// �Ա� ������Դ�Ա��ļ�(StreamingAssets����Ĭ����Դ�ļ���persistentDataPath��������)
//    ������û�е��ļ��������أ��Ȳ���persistentDataPath·�����ļ���������ȥStreamingAssetsѰ�ң����������أ�
//    ��md5��һ�µ� ���и���
//    ����Ч��Դ����ɾ���������������ڣ���persistentDataPath���ڵ��ļ�����ɾ��
// �����±�����Դ�Ա��ļ�

// ���� ����ֻ���� ��ʼ��Ϸʱ������ʹ��ʱֱ�ӽ��ж��󴴽�����ʹ�õ���ģʽ
namespace Wu
{
    public class HotUpdate
    {
        Dictionary<string, string> serverAbListDic = new Dictionary<string, string>();
        Dictionary<string, string> perLocalAbListDic = new Dictionary<string, string>();
        Dictionary<string, string> strLocalAbListDic = new Dictionary<string, string>();

        List<string> persistentFileNameList = new List<string>();
        List<string> needUpdateFileList = new List<string>();
        List<string> perValidFileNameList = new List<string>();

        // ����AB��
        public void UpdateLocalAB(Action<int, int> upDataAction = null)
        {
            UpdateServerAbListDic(); // ��������Դ�Ա��ļ�
            UpdateLocalAbListDic(); // ������Դ�Ա��ļ�
            //    ������û�е��ļ��������أ��Ȳ���persistentDataPath·�����ļ���������ȥStreamingAssetsѰ�ң����������أ�
            //    ��md5��һ�µ� ���и���
            //    ����Ч��Դ����ɾ���������������ڣ���persistentDataPath���ڵ��ļ�����ɾ��

            // ��������ͳ��
            foreach(var pair in serverAbListDic)
            {
                if(perLocalAbListDic.ContainsKey(pair.Key))
                {
                    if (!pair.Value.Equals(perLocalAbListDic[pair.Key])) needUpdateFileList.Add(pair.Key);
                    persistentFileNameList.Remove(pair.Key);
                    perValidFileNameList.Add(pair.Key);
                }
                else if(strLocalAbListDic.ContainsKey(pair.Key))
                {
                    if (!pair.Value.Equals(strLocalAbListDic[pair.Key]))
                    {
                        needUpdateFileList.Add(pair.Key);
                        perValidFileNameList.Add(pair.Key);
                    }
                }
                else
                {
                    needUpdateFileList.Add(pair.Key);
                    perValidFileNameList.Add(pair.Key);
                }
            }

            // �첽����
            asyncNeedUpdateAB(upDataAction);
        }

        private async void asyncNeedUpdateAB(Action<int, int> upDataAction)
        {
            int updateFileCount = needUpdateFileList.Count;
            int nowUpdatedCount = 0;
            int reUpdateCount = 0;
            string perPath = Application.persistentDataPath;
            await Task.Run(() =>
            {
                // ����AB��
                foreach(string filename in needUpdateFileList)
                {
                    reUpdateCount = 0;
                    while(reUpdateCount < Data.reDoCountMax)
                    {
                        if (Tools.GetFileByServer(Data.resServerUrl + "/" + filename, perPath + "/" + filename))
                        {
                            ++nowUpdatedCount;
                            upDataAction?.Invoke(nowUpdatedCount, updateFileCount);
                            break;
                        }
                        ++reUpdateCount;
                    }
                }
            });
            // ȥ����Ч��Դ
            DeleteUnValidFile(persistentFileNameList);
            // ������Դ�Ա��ļ�
            UpdateAbList();
        }

        // ���������������ֵ���
        private bool HandleDataToDic(string data, ref Dictionary<string, string> ret)
        {
            if (ret == null) ret = new Dictionary<string, string>();
            if (data.Length <= 0) ret.Clear();

            string[] dataArr = data.Split('-');

            
            foreach(string oneData in dataArr)
            {
                string[] pair = oneData.Split(' ');
                if (pair.Length != 2) { Debug.LogError("HandleDataToDic : Data Format Error"); return false; }
                ret.Add(pair[0], pair[1]);
            }
            return true;
        }

        // ɾ����Ч��Դ
        private void DeleteUnValidFile(List<string> unValidFileList)
        {
            foreach (string filename in unValidFileList)
                File.Delete(Application.persistentDataPath + "/" + filename);
        }

        // ����persistentDataPath�е���Դ�Ա��ļ�
        private bool UpdateAbList()
        {
            StringBuilder text = new StringBuilder();

            foreach(string key in perValidFileNameList)
            {
                text.Append(key);
                text.Append(" ");
                text.Append(serverAbListDic[key]).Append("-");
            }
            try
            {
                File.WriteAllText(Application.persistentDataPath + "/" + Data.abListName, text.ToString().Substring(0, text.Length - 1));
                return true;
            }
            catch
            {
                return false;
            }
            
        }


        private void UpdateServerAbListDic()
        {
            string readText = Tools.GetWebCode(Data.resServerUrl + "/" + Data.abListName);
            HandleDataToDic(readText, ref serverAbListDic);
        }

        private void UpdateLocalAbListDic()
        {
            string readText;
            // ��ȡpersistentDataPath�µ�abList.txt
            if (File.Exists(Application.persistentDataPath +"/"+ Data.abListName))
            {
                readText = File.ReadAllText(Application.persistentDataPath + "/" + Data.abListName);
                HandleDataToDic(readText, ref perLocalAbListDic);
                persistentFileNameList.Clear();
                foreach (var pair in perLocalAbListDic)
                {
                    persistentFileNameList.Add(pair.Key);
                }
            }
            // ��ȡStreamingAssets�µ�abList.txt
            if (File.Exists(Application.streamingAssetsPath + "/" + Data.abListName))
            {
                readText = File.ReadAllText(Application.streamingAssetsPath + "/" + Data.abListName);
                HandleDataToDic(readText, ref strLocalAbListDic);
            }
        }

    }


}
