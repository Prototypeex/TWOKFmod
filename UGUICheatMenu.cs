//using System.Collections.Generic;
//using System.Linq;
//using BepInEx;
//using BepInEx.Unity.Mono;
//using UnityEngine;
//using BepInEx.Configuration;
//using HarmonyLib;
//using System;
//using System.Reflection;
//using UnityEngine.UI;
//using TMPro;


//namespace CheatMenu
//{
//    [BepInPlugin("me.Prototypehu.plugin.UGUICheatMenu", "UGUICheatMenu", "0.0.1")]
//    public class UGUICheatMenu : BaseUnityPlugin
//    {
//        private GameObject uiCanvasPrefab;
//        private GameObject uiCanvas;
//        private Button closeButton;
//        private CharaData charaData;
//        private AssetBundle ab;
//        private string lastid;
//        public static List<gang_b07Table.Row> itemlist;//物品表
//        public static List<gang_b06Table.Row> traitlist;//特征表
//        private List<string> itemnames = new List<string>();
//        private List<string> traitnames = new List<string>();
//        ConfigEntry<KeyCode> hotkey;
//        // 创建一个结构体来存储 Toggle、Panel 和 HoverImage 的引用
//        private struct TogglePanel
//        {
//            public Toggle toggle;
//            public GameObject panel;
//            public GameObject hoverImage;

//            public TogglePanel(Toggle toggle, GameObject panel, GameObject hoverImage)
//            {
//                this.toggle = toggle;
//                this.panel = panel;
//                this.hoverImage = hoverImage;
//            }
//        }
//        private List<TogglePanel> togglePanels = new List<TogglePanel>();
//        void Start()
//        {
//            hotkey = Config.Bind("Hotkeys", "CheatMenu", KeyCode.F1);
//        }
//        void Update()
//        {
//            if (Input.GetKeyDown(hotkey.Value))
//            {
//                if (uiCanvas != null)
//                {
//                    string curId = SharedData.Instance(false).CurrentCharaData.m_Id;
//                    //若id变化，则刷新属性栏
//                    if(curId!=lastid)
//                    {
//                        lastid=curId;
//                        var panel = uiCanvas.transform.Find("Element/AttributePanel").gameObject;
//                        InitAttributePanel(panel,ab);
//                    }
//                    uiCanvas.SetActive(!uiCanvas.activeSelf);
//                }
//                else
//                {
//                    charaData= SharedData.Instance(false).CurrentCharaData;
//                    lastid = charaData.m_Id;
//                    if (charaData == null)
//                    {
//                        return;
//                    }
//                    else
//                    {
//                        // 如果未实例化，则加载并显示
//                        LoadAB();
//                    }
//                }
//            }
//        }

//        public void LoadAB()
//        {
//            // 取得当前程序集
//            Assembly asm = Assembly.GetExecutingAssembly();
//            using (var stream = asm.GetManifestResourceStream("CheatMenu.twokfui"))
//            {
//                if (stream == null)
//                {
//                    Debug.LogError("未找到资源流 CheatMenu.twokfui");
//                    return;
//                }

//                ab = AssetBundle.LoadFromStream(stream);
//                if (ab != null)
//                {
//                    Debug.Log("AssetBundle 加载成功");
//                    foreach (var assetName in ab.GetAllAssetNames())
//                    {
//                        Debug.Log("Asset in bundle: " + assetName);
//                    }
//                    DisplayCanvas(ab);
//                }
//                else
//                {
//                    Debug.LogError("未找到AssetBundle");
//                }
//            }
//        }
//        private void OnDestroy()
//        {
//            if (ab != null)
//            {
//                ab.Unload(true);
//                ab = null;
//            }
//        }
//        public void DisplayCanvas(AssetBundle ab)
//        {
//            uiCanvasPrefab = ab.LoadAsset<GameObject>("assets/prefabs/canvasprefab.prefab");
//            if (uiCanvasPrefab != null)
//            {
//                uiCanvas = Instantiate(uiCanvasPrefab);
//                uiCanvas.transform.localPosition = Vector3.zero;
//                uiCanvas.transform.localScale = Vector3.one;
//                uiCanvas.SetActive(true);
//                uiCanvas.GetComponent<Canvas>().sortingOrder = 9999;
//                InitializeToggles(ab);
//            }
//            else
//            {
//                Debug.LogError("未找到CanvasPrefab");
//            }
//        }

//        private void InitializeToggles(AssetBundle ab)
//        {
//            // 使用通用方法添加每个 Toggle 和对应的 Panel
//            AddTogglePanel("AttributeToggle", "AttributePanel",ab);
//            AddTogglePanel("ItemToggle", "ItemPanel",ab);
//            AddTogglePanel("TraitToggle", "TraitPanel", ab);
//            AddTogglePanel("KongFuToggle", "KongFuPanel", ab);
//            AddTogglePanel("OtherToggle", "OtherPanel", ab);

//            // 初始状态：只显示第一个 Panel
//            if (togglePanels.Count > 0)
//            {
//                ShowPanel(togglePanels[0].panel);
//                togglePanels[0].hoverImage.SetActive(true);
//            }
//            closeButton = uiCanvas.transform.Find("Element/Close").GetComponent<Button>();
//            closeButton.onClick.AddListener(() => uiCanvas.SetActive(false));
//        }
//        private void AddTogglePanel(string toggleName, string panelName,AssetBundle ab)
//        {
//            // 根据名称找到 Toggle
//            var toggle = uiCanvas.transform.Find("Element/" + toggleName).GetComponent<Toggle>();
//            // 找到对应的 Panel
//            var panel = uiCanvas.transform.Find("Element/" + panelName).gameObject;
//            // 找到 Toggle 下的 HoverImage
//            var hoverImage = toggle.transform.Find("HoverImage").gameObject;

//            // 绑定 Toggle 事件
//            toggle.onValueChanged.AddListener(isOn => OnToggleChanged(panel, hoverImage, isOn));

//            // 将 Toggle 和 Panel 添加到列表
//            togglePanels.Add(new TogglePanel(toggle, panel, hoverImage));

//            switch(panel.name)
//            {
//                case "AttributePanel":
//                    InitAttributePanel(panel,ab);
//                    break;
//                case "ItemPanel":

//                    break;
//                case "TraitPanel":
//                    break;
//                case "KongFuPanel":
//                    break;
//                case "OtherPanel":
//                    break;
//            }
//        }
//        private void InitAttributePanel(GameObject panel,AssetBundle ab)
//        {
//            DestroyAllChildren(panel.transform.GetChild(0).GetChild(1));
//            DestroyAllChildren(panel.transform.GetChild(1).GetChild(1));
//            DestroyAllChildren(panel.transform.GetChild(2).GetChild(1));
//            Transform mainattri = panel.transform.GetChild(0).GetChild(1);
//            UGUIPropertyInit("臂力", "STR", false, ab, mainattri);
//            UGUIPropertyInit("定力", "WIL", false, ab, mainattri);
//            UGUIPropertyInit("敏捷", "AGI", false, ab, mainattri);
//            UGUIPropertyInit("悟性", "LER", false, ab, mainattri);
//            UGUIPropertyInit("根骨", "BON", false, ab, mainattri);
//            UGUIPropertyInit("道德", "MOR", false, ab, mainattri);
//            Transform disattri = panel.transform.GetChild(1).GetChild(1);
//            UGUIPropertyInit("血量", "HP", true, ab, disattri);
//            UGUIPropertyInit("内力", "MP", true, ab, disattri);
//            UGUIPropertyInit("攻击力", "ATK", true, ab, disattri);
//            UGUIPropertyInit("防御力", "DEF", true, ab, disattri);
//            UGUIPropertyInit("轻功", "SP", true, ab, disattri);
//            UGUIPropertyInit("暴击率", "Crit", true, ab, disattri);
//            UGUIPropertyInit("暴击伤害", "Crit1", true, ab, disattri);
//            UGUIPropertyInit("连击率", "Combo", true, ab, disattri);
//            Transform kongfuattri = panel.transform.GetChild(2).GetChild(1);
//            UGUIPropertyInit("剑法", "Sword", true, ab, kongfuattri);
//            UGUIPropertyInit("刀法", "Knife", true, ab, kongfuattri);
//            UGUIPropertyInit("棍杖", "Stick", true, ab, kongfuattri);
//            UGUIPropertyInit("拳掌", "Hand", true, ab, kongfuattri);
//            UGUIPropertyInit("指力", "Finger", true, ab, kongfuattri);
//            UGUIPropertyInit("特殊", "Special", true, ab, kongfuattri);
//            UGUIPropertyInit("奇门", "YinYang", true, ab, kongfuattri);
//            UGUIPropertyInit("音律", "Melody", true, ab, kongfuattri);
//            UGUIPropertyInit("锻造", "Forge", true, ab, kongfuattri);
//            UGUIPropertyInit("酒艺", "Wineart", true, ab, kongfuattri);
//            UGUIPropertyInit("暗器", "Darts", true, ab, kongfuattri);
//            UGUIPropertyInit("盗术", "Steal", true, ab, kongfuattri);
//        }
//        void DestroyAllChildren(Transform parent)
//        {
//            for (int i = parent.childCount - 1; i >= 0; i--)
//            {
//                GameObject.Destroy(parent.GetChild(i).gameObject);
//            }
//        }
//        private void OnToggleChanged(GameObject panel, GameObject hoverImage, bool isOn)
//        {
//            if (isOn)
//            {
//                ShowPanel(panel); // 显示对应 Panel
//                hoverImage.SetActive(true); // 显示 HoverImage
//            }
//            else
//            {
//                hoverImage.SetActive(false); // 隐藏 HoverImage
//            }
//        }

//        private void ShowPanel(GameObject panelToShow)
//        {
//            foreach (var togglePanel in togglePanels)
//            {
//                togglePanel.panel.SetActive(false); // 隐藏所有 Panel
//            }

//            panelToShow.SetActive(true); // 仅显示指定 Panel
//        }

//        public void UGUIPropertyInit(string name,string field,bool canInput, AssetBundle ab,Transform root)
//        {
//            GameObject prefab;
            
//            if (SharedData.Instance(false).CurrentCharaData == null)
//            {
//                Debug.LogError("CharaData is null");
//                return;
//            }
//            if (canInput)
//            {
//                prefab = ab.LoadAsset<GameObject>("assets/prefabs/displayattriprefab.prefab");
//            }
//            else
//            {
//                prefab = ab.LoadAsset<GameObject>("assets/prefabs/mainattriprefab.prefab");
//            }
//            if( prefab != null)
//            {
//                GameObject attri=Instantiate(prefab,root);
//                //属性名
//                attri.transform.Find("Image").GetComponentInChildren<TextMeshProUGUI>().text=name;
//                //属性值
//                var number=attri.transform.Find("Number").GetComponent<TextMeshProUGUI>();
//                number.text = SharedData.Instance(false).CurrentCharaData.GetFieldValueByName(field).ToString();
//                var add = attri.transform.Find("Add").GetComponent<Button>();
//                var sub = attri.transform.Find("Sub").GetComponent<Button>();
//                TMP_InputField inputfield = canInput?attri.transform.Find("InputField").GetComponent<TMP_InputField>():null;
//                add.onClick.AddListener(() =>
//                {
//                    SharedData.Instance(false).CurrentCharaData.Indexs_Name[field].alterValue += canInput ? int.Parse(inputfield.text) : 1;
//                    UpdateNumber(number, field); // 更新显示的值
//                });

//                // 设置 Sub 按钮点击事件
//                sub.onClick.AddListener(() =>
//                {
//                    SharedData.Instance(false).CurrentCharaData.Indexs_Name[field].alterValue -= canInput ? int.Parse(inputfield.text) : 1;
//                    UpdateNumber(number, field); // 更新显示的值
//                });

//            }
//            else
//            {
//                Debug.LogError("未找到属性Prefab");
//            }
//        }
//        private void UpdateNumber(TextMeshProUGUI number,string field)
//        {
//            number.text= SharedData.Instance(false).CurrentCharaData.GetFieldValueByName(field).ToString();
//        }
//    }   

//}
