using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class ABLoader
{
    // �洢AB��
    static Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();
    // ������Ϣ
    static AssetBundleManifest abManifest = null;
    // ����
    static AssetBundle abMain = null;


    // ab����·��
    static string abPath = Application.streamingAssetsPath + "/";

    // ��AB����
    static string abMainName
    {
        get
        {
            return "Windows";
        }
    }

    static MonoBehaviour monoBehaviour = new MonoBehaviour();

    /// <summary>
    /// ��ʼ��������������������Ϣ
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
    /// ͬ�� ����AB�� �� abDic�� : ��������������AB��
    /// </summary>
    static void LoadAb(string abName)
    {
        Init();
        if (abDic.ContainsKey(abName)) return;
        // ��������
        string[] abNameArr = abManifest.GetAllDependencies(abName);
        foreach(var oneAbName in abNameArr)
        {
            if (abDic.ContainsKey(oneAbName)) continue;
            abDic.Add(oneAbName, AssetBundle.LoadFromFile(abPath + oneAbName));
        }

        // ����AB��
        abDic.Add(abName, AssetBundle.LoadFromFile(abPath + abName));
    }



    /// <summary>
    /// �첽 ����AB�� �� abDic�� : ��������������AB��
    /// </summary>
    static void AsyncLoadAb(string abName, System.Action action)
    {
        if (abDic.ContainsKey(abName)) return;
        monoBehaviour.StartCoroutine(_AsyncLoadAb(abName, action));
    }

    static IEnumerator _AsyncLoadAb(string abName, System.Action action)
    {
        Init();
        // ��������
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
        // ����AB��
        aR = AssetBundle.LoadFromFileAsync(abPath + abName);
        abDic.Add(abName, aR.assetBundle);
        action?.Invoke();
    }



    /// <summary>
    /// ͬ��������Դ
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <returns></returns>
    public static Object LoadRes(string abName, string resName)
    {
        LoadAb(abName); // ����AB��
        if (!abDic.ContainsKey(abName)) return null;

        return abDic[abName].LoadAsset(resName);
    }

    /// <summary>
    /// ͬ��������Դ
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <returns></returns>
    public static Object LoadRes(string abName, string resName, System.Type type)
    {
        LoadAb(abName); // ����AB��
        if (!abDic.ContainsKey(abName)) return null;

        return abDic[abName].LoadAsset(resName, type);
    }


    /// <summary>
    /// �첽������Դ
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <returns></returns>
    public static void AsyncLoadRes(string abName, string resName, System.Action<Object> action)
    {
        // ����AB��
        AsyncLoadAb(abName, () =>
        {
            // ������Դ
            if (!abDic.ContainsKey(abName)) action(null);
            else action(abDic[abName].LoadAsset(resName));

        });
    }


    /// <summary>
    /// �첽������Դ
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <returns></returns>
    public static void AsyncLoadRes(string abName, string resName, System.Type type, System.Action<Object> action)
    {
        // ����AB��
        AsyncLoadAb(abName, () =>
        {
            // ������Դ
            if (!abDic.ContainsKey(abName)) action(null);
            else action(abDic[abName].LoadAsset(resName, type));
        });
    }

    /// <summary>
    /// ɾ������AB��
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
    /// ɾ������AB��
    /// </summary>
    static void Clear()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        abDic.Clear();
    }

}
