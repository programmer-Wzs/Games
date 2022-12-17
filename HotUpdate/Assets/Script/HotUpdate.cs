using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// 读取远程服务器的资源对比文件（abList.txt）
// 对比 本地资源对比文件(StreamingAssets保存默认资源文件，persistentDataPath保持最新)
//    将本地没有的文件进行下载（先查找persistentDataPath路径中文件，若无则去StreamingAssets寻找，若无则下载）
//    将md5不一致的 进行更新
//    将无效资源进行删除，服务器不存在，而persistentDataPath存在的文件进行删除
// 最后更新本地资源对比文件

// 由于 更新只存在 开始游戏时，我们使用时直接进行对象创建，不使用单例模式
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

        // 更新AB包
        public void UpdateLocalAB(Action<int, int> upDataAction = null)
        {
            UpdateServerAbListDic(); // 服务器资源对比文件
            UpdateLocalAbListDic(); // 本地资源对比文件
            //    将本地没有的文件进行下载（先查找persistentDataPath路径中文件，若无则去StreamingAssets寻找，若无则下载）
            //    将md5不一致的 进行更新
            //    将无效资源进行删除，服务器不存在，而persistentDataPath存在的文件进行删除

            // 更新数据统计
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

            // 异步更新
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
                // 更新AB包
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
            // 去除无效资源
            DeleteUnValidFile(persistentFileNameList);
            // 更新资源对比文件
            UpdateAbList();
        }

        // 将数据整理，放入字典中
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

        // 删除无效资源
        private void DeleteUnValidFile(List<string> unValidFileList)
        {
            foreach (string filename in unValidFileList)
                File.Delete(Application.persistentDataPath + "/" + filename);
        }

        // 更新persistentDataPath中的资源对比文件
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
            // 读取persistentDataPath下的abList.txt
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
            // 读取StreamingAssets下的abList.txt
            if (File.Exists(Application.streamingAssetsPath + "/" + Data.abListName))
            {
                readText = File.ReadAllText(Application.streamingAssetsPath + "/" + Data.abListName);
                HandleDataToDic(readText, ref strLocalAbListDic);
            }
        }

    }


}
