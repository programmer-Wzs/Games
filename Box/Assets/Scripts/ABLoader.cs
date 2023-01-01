using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class ABLoader
{
    // 存储AB包
    static Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();
    // 依赖信息
    static AssetBundleManifest abManifest = null;
    // 主包
    static AssetBundle abMain = null;


    // ab包首路径
    static string abPath = Application.streamingAssetsPath + "/";

    // 主AB包名
    static string abMainName
    {
        get
        {
            return "Windows";
        }
    }

    static MonoBehaviour monoBehaviour = new MonoBehaviour();

    /// <summary>
    /// 初始化：加载主包和依赖信息
    /// </summary>
    static void Init()
    {
        if(null == abMain)
        {
            abMain = AssetBundle.LoadFromFile(abPath + abMainName);
            abManifest = abMain.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        }
    }
    /// <summary>
    /// 同步 加载AB包 到 abDic中 : 加载依赖，加载AB包
    /// </summary>
    static void LoadAb(string abName)
    {
        Init();
        if (abDic.ContainsKey(abName)) return;
        // 加载依赖
        string[] abNameArr = abManifest.GetAllDependencies(abName);
        foreach(var oneAbName in abNameArr)
        {
            if (abDic.ContainsKey(oneAbName)) continue;
            abDic.Add(oneAbName, AssetBundle.LoadFromFile(abPath + oneAbName));
        }

        // 加载AB包
        abDic.Add(abName, AssetBundle.LoadFromFile(abPath + abName));
    }



    /// <summary>
    /// 异步 加载AB包 到 abDic中 : 加载依赖，加载AB包
    /// </summary>
    static void AsyncLoadAb(string abName, System.Action action)
    {
        if (abDic.ContainsKey(abName)) return;
        monoBehaviour.StartCoroutine(_AsyncLoadAb(abName, action));
    }

    static IEnumerator _AsyncLoadAb(string abName, System.Action action)
    {
        Init();
        // 加载依赖
        AssetBundleCreateRequest aR;
        string[] abNameArr = abManifest.GetAllDependencies(abName);
        foreach (var oneAbName in abNameArr)
        {
            if (abDic.ContainsKey(oneAbName)) continue;
            abDic.Add(oneAbName, AssetBundle.LoadFromFile(abPath + oneAbName));
            aR = AssetBundle.LoadFromFileAsync(abPath + oneAbName);
            yield return aR;
            abDic.Add(oneAbName, aR.assetBundle);
        }
        // 加载AB包
        aR = AssetBundle.LoadFromFileAsync(abPath + abName);
        abDic.Add(abName, aR.assetBundle);
        action?.Invoke();
    }



    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <returns></returns>
    public static Object LoadRes(string abName, string resName)
    {
        LoadAb(abName); // 加载AB包
        if (!abDic.ContainsKey(abName)) return null;

        return abDic[abName].LoadAsset(resName);
    }

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <returns></returns>
    public static Object LoadRes(string abName, string resName, System.Type type)
    {
        LoadAb(abName); // 加载AB包
        if (!abDic.ContainsKey(abName)) return null;

        return abDic[abName].LoadAsset(resName, type);
    }


    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <returns></returns>
    public static void AsyncLoadRes(string abName, string resName, System.Action<Object> action)
    {
        // 加载AB包
        AsyncLoadAb(abName, () =>
        {
            // 加载资源
            if (!abDic.ContainsKey(abName)) action(null);
            else action(abDic[abName].LoadAsset(resName));

        });
    }


    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <returns></returns>
    public static void AsyncLoadRes(string abName, string resName, System.Type type, System.Action<Object> action)
    {
        // 加载AB包
        AsyncLoadAb(abName, () =>
        {
            // 加载资源
            if (!abDic.ContainsKey(abName)) action(null);
            else action(abDic[abName].LoadAsset(resName, type));
        });
    }

    /// <summary>
    /// 删除单个AB包
    /// </summary>
    /// <param name="abName"></param>
    static void UnLoadOneAB(string abName)
    {
        if (abDic.ContainsKey(abName))
        {
            abDic[abName].Unload(false);
            abDic.Remove(abName);
        }
    }

    /// <summary>
    /// 删除所有AB包
    /// </summary>
    static void Clear()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        abDic.Clear();
    }

}
