using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

[LuaCallCSharp]
public class BoxComponent : MonoBehaviour
{
    GameObject boxView = null;
    GameObject leftView = null;
    GameObject rightView = null;
    GameObject moneyView = null;    
    GameObject deleteButtonObject = null;
    GameObject useButtonObject = null;

    Button backButton = null;
    Text kindText = null;
    Text moneyText = null;
    BoxViewCtrl boxViewCtrl = null;
    Text rightNameText = null;
    Text rightAtuText = null;
    Text rightOtherText = null;

    private void Awake()
    {
        boxView = transform.Find("BoxView").gameObject;
        boxViewCtrl = boxView.GetComponent<BoxViewCtrl>();
        leftView = transform.Find("LeftView").gameObject;
        rightView = transform.Find("RightView").gameObject;
        moneyView = transform.Find("MoneyView").gameObject;
        moneyText = moneyView.transform.Find("Text").gameObject.GetComponent<Text>();
        backButton = transform.Find("BackButton").gameObject.GetComponent<Button>();
        deleteButtonObject = transform.Find("DeleteButton").gameObject;
        useButtonObject = transform.Find("UseButton").gameObject;
        kindText = transform.Find("KindText").gameObject.GetComponent<Text>();
        rightNameText = rightView.transform.Find("NameText").GetComponent<Text>();
        rightAtuText = rightView.transform.Find("AtuText").GetComponent<Text>();
        rightOtherText = rightView.transform.Find("OtherText").GetComponent<Text>();
    }
}
