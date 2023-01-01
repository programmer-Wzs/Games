using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using XLua;

[LuaCallCSharp]
public enum ItemKind
{
    WEAPON,
    ARMOR,
    CONSUME
}

[LuaCallCSharp]
public struct ItemInfo
{
    public string name;
    public string num;
    public ItemKind kind;
    public string text;
    public string abName;
    public string spriteAtlasName;
    public string spriteName;
}


[LuaCallCSharp]
public class BoxViewCtrl : MonoBehaviour
{
    
    ScrollRect sr;
    RectTransform contentRt;
    RectTransform rTransform;
    Scrollbar scrollbarVertical;

    int leftPadding = 25;
    int topPadding = 20;
    int interval = 20;
    int columnNum = 10;
    int rowNum = 8;
    int itemWidth;
    int itemHigth;
    int maxCount=255;
    //int x;
    //int y;

    int nowIndex;
    int startIndex;
    int endIndex;

    Dictionary<ItemKind, List<int> > boxDic = null; // 存储背包中不同类别物品的id
    Dictionary<int, ItemInfo> itemInfoDic = null; // 存储所有物品信息：物品id->物品信息

    ItemKind nowItemKind = ItemKind.WEAPON;

    List<GameObject> itemObjectList;
    List<GameObject> itemShowObjectList;
    GameObject itemObject;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    public void Init(Dictionary<int, ItemInfo> dic, Dictionary<ItemKind, List<int>> boxdic, ItemKind itemKind)
    {
        itemInfoDic = dic;
        boxDic = boxdic;
        nowItemKind = itemKind;
        maxCount = boxdic[itemKind].Count-1;

        Init();
        ClearShow();
        InitShow();

    }

    public void SetBoxDic(Dictionary<ItemKind, List<int>>  dic)
    {
        boxDic = dic;
    }

    public void SetNowItemKind(ItemKind itemKind)
    {
        if (nowItemKind == itemKind) return;
        nowItemKind = itemKind;
        maxCount = boxDic[itemKind].Count - 1;

        int storeNum = rowNum * columnNum;
        contentRt.sizeDelta = new Vector2(0, topPadding + (itemHigth + interval) * (maxCount / columnNum + 1));
        startIndex = 0;
        endIndex = maxCount > storeNum ? storeNum - 1 : maxCount - 1;
        nowIndex = -1;

        ClearShow();
        InitShow();
    }

    void ClearShow()
    {
        while(itemShowObjectList.Count > 0)
        {
            AddGameObjectToPool(itemShowObjectList[0]);
            itemShowObjectList.RemoveAt(0);
        }
    }

    void Init()
    {
        itemShowObjectList = new List<GameObject>();
        int storeNum = rowNum * columnNum;
        sr = transform.GetComponent<ScrollRect>();
        sr.onValueChanged.AddListener(onValueChanged);
        rTransform = GetComponent<RectTransform>();
        scrollbarVertical = transform.Find("Scrollbar Vertical").GetComponent<Scrollbar>();

        itemObject = transform.Find("ItemObject").gameObject as GameObject;
        itemWidth = ((int)itemObject.GetComponent<RectTransform>().sizeDelta.x);
        itemHigth = ((int)itemObject.GetComponent<RectTransform>().sizeDelta.y);

        contentRt = transform.Find("Viewport").Find("Content").GetComponent<RectTransform>();
        contentRt.sizeDelta = new Vector2(0, topPadding + (itemHigth + interval) * (maxCount / columnNum + 1));
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);

        startIndex = 0;
        endIndex = maxCount > storeNum ? storeNum - 1 : maxCount - 1;
        nowIndex = -1;

        InitPool();
        InitShow();
    }

    void onValueChanged(Vector2 v2)
    {
        //Debug.Log(v2);
        //print(contentRt.anchoredPosition.x.ToString() + ", " + contentRt.anchoredPosition.y.ToString());
        int x = leftPadding;
        int y = -topPadding;
        // 上划
        if(Mathf.Abs(contentRt.anchoredPosition.y) > (itemHigth+interval) * (startIndex / columnNum + 1))
        {
            y -= (itemHigth + interval) * (endIndex / columnNum);
            for(int i = 0; i < columnNum && startIndex <= endIndex; ++i)
            {
                if(itemShowObjectList.Count > 0)
                {
                    AddGameObjectToPool(itemShowObjectList[0]);
                    itemShowObjectList.RemoveAt(0);
                    ++startIndex;
                    
                }
                if(endIndex < maxCount-1)
                {
                    ++endIndex;
                    GameObject newGameObject = GetGameObjectByPool();
                    newGameObject.SetActive(true);
                    //UpdateGameObject(newGameObject, boxDic[nowItemKind][endIndex]);
                    newGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
                    itemShowObjectList.Add(newGameObject);
                    x += itemWidth + interval;
                    
                }    
            }
        }

        // 下划
        x = leftPadding;
        y = -topPadding;
        int endRow = endIndex / columnNum;
        int showCount = columnNum * rowNum;
        if (Mathf.Abs(contentRt.anchoredPosition.y) < (itemHigth + interval) * (startIndex / columnNum))
        {
            y -= (itemHigth + interval) * (startIndex / columnNum - 1);
            if(endIndex - startIndex > showCount - 1)
            {
                for (int end = endIndex; end >= (columnNum * endRow) && startIndex <= endIndex; --end)
                {
                    if (itemShowObjectList.Count > 0)
                    {
                        AddGameObjectToPool(itemShowObjectList[itemShowObjectList.Count - 1]);
                        itemShowObjectList.RemoveAt(itemShowObjectList.Count - 1);
                        --endIndex;
                    }
                }
            }

            for (int i = 0; i < columnNum && startIndex > 0; ++i)
            {
                GameObject newGameObject = GetGameObjectByPool();
                newGameObject.SetActive(true);
                //UpdateGameObject(newGameObject, boxDic[nowItemKind][startIndex - 1]);
                newGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
                itemShowObjectList.Insert(0, newGameObject);
                --startIndex;
                x += itemWidth + interval;
            }
        }

        //print("start-end : " + startIndex.ToString() + "-" + endIndex.ToString());

    }

    void UpdateGameObject(GameObject mGameObject, int id)
    {
        /*mGameObject.GetComponent<ItemObject>().id = id;
        mGameObject.GetComponent<Image>().sprite = (ABLoader.LoadRes(itemInfoDic[id].abName, itemInfoDic[id].spriteAtlasName, typeof(SpriteAtlas)) as SpriteAtlas).GetSprite(itemInfoDic[id].spriteName);*/
    }

    void InitPool()
    {
        int storeNum = rowNum * columnNum;
        itemObjectList = new List<GameObject>(storeNum);
        for (int i = 0; i < storeNum; ++i)
        {
            GameObject newGameObject = Instantiate(itemObject, contentRt);
            newGameObject.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            newGameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            newGameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            newGameObject.SetActive(false);
            itemObjectList.Add(newGameObject);
        }
    }

    GameObject GetGameObjectByPool()
    {
        if(itemObjectList.Count == 0)
        {
            GameObject newGameObject = Instantiate(itemObject, contentRt);
            newGameObject.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            newGameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            newGameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            newGameObject.SetActive(false);
            return newGameObject;
        }
        GameObject ret = itemObjectList[0];
        itemObjectList.RemoveAt(0);
        return ret;
    }

    void AddGameObjectToPool(GameObject newGameObject)
    {
        newGameObject.SetActive(false);
        if (itemObjectList == null) itemObjectList = new List<GameObject>() { newGameObject };
        itemObjectList.Add(newGameObject);
    }

    void InitShow()
    {
        scrollbarVertical.value = 1;
        int x = leftPadding;
        int y = -topPadding;
        int nowColumn = 0;
        for(int i = 0; i <= endIndex; ++i)
        {
            GameObject newGameObject = GetGameObjectByPool();
            newGameObject.SetActive(true);
            //UpdateGameObject(newGameObject, boxDic[nowItemKind][i]);
            newGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            itemShowObjectList.Add(newGameObject);
            ++nowColumn;
            if(nowColumn / columnNum == 1)
            {
                nowColumn = 0;
                y -= itemHigth + interval;
                x = leftPadding;
            }
            else
            {
                x += itemWidth + interval;
            }
        }
    }
}
