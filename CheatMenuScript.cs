﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Unity.Mono;
using UnityEngine;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;

namespace CheatMenu
{
    [BepInPlugin("me.Prototypehu.plugin.CheatMenu","Cheatmenu","0.0.1")]
    public class CheatMenu:BaseUnityPlugin
    {
        ConfigEntry<KeyCode> hotkey;//这个定义+下面的Config.Bind就可以在启动后生成配置文件，进而在配置中手动修改快捷键
        ConfigEntry<KeyCode> hotkey1;
        ConfigEntry<float> configFloat;
        ConfigEntry<float> configFloat1;
        ConfigEntry<string> configString;
        public static bool preventItemReduction;
        public static int extraExpValue=0;
        public static List<gang_b07Table.Row> b07 { get; private set; }//物品表
        public static List<gang_b06Table.Row> b06 { get; private set; }//特征表
        public static List<string> itemNames { get; private set; }
        public static List<string> traitNames { get; private set; }
        private Rect windowRect = new Rect(50, 50, 800, 480);
        private string inputString = "0";
        private string searchText = "";
        private static string input = "0";
        private Vector2 scrollPosition = Vector2.zero;
        private string teststring;
        private bool testBool = false;
        private string[] testToolbarNames = new string[] { "属性", "物品","特征","其他" };
        private int testToolbarIndex = 0;
        private float testFloat;
        private List<gang_b07Table.Row> searchResults = new List<gang_b07Table.Row>();
        private List<gang_b06Table.Row> searchResults1 = new List<gang_b06Table.Row>();
        private Vector2 scrollViewPos;
        private bool windowShow = false;

        void Start()
        {
            hotkey = Config.Bind<KeyCode>("config","hotkey",KeyCode.Tab,"CheatMenu菜单显示");//默认的快捷键是Tab
            hotkey1 = Config.Bind<KeyCode>("config","hotkey1",KeyCode.T,"测试修改功能的快捷键，无需理会");
            Logger.LogInfo("CheatMenu初始化中...");
            Harmony.CreateAndPatchAll(typeof(CheatMenu));
            Logger.LogInfo("CheatMenu初始化成功");

        }

        void Update()
        {
            if(Input.GetKeyDown(hotkey.Value))
            {
                windowShow = !windowShow;//通过快捷键控制修改菜单的显示与否
            }
            if (Input.GetKeyDown(hotkey1.Value))
            {
                SharedData.Instance(false).m_Money += 1000;//改钱，是队伍数据         

                //获取玩家的数据
                CharaData charaData = SharedData.Instance(false).GetCharaData(SharedData.Instance(false).playerid);
                charaData.m_Exp += 500;
                charaData.m_Talent += 1;
                charaData.Indexs_Name["MOR"].alterValue++;//这个只建议修改道德，然后用天赋点来加别的点
                float Hp = charaData.GetFieldValueByName("HP");//界面显示的数据，是经过计算的，不要动，仅用于显示
                List<gang_b07Table.Row> b07 = CommonResourcesData.b07.GetRowList();
                foreach(var kongfu in charaData.m_KongFuList)
                {
                    Debug.Log(kongfu.kf.Name);
                    Debug.Log(kongfu.kf.ID);
                }
            }
        }

        private void OnGUI()
        {
            // 修改全局样式
            GUI.skin.label.fontSize = 20;
            GUI.skin.button.fontSize = 20;
            GUI.skin.textField.fontSize = 20;
            GUI.skin.toggle.fontSize = 20;

            // 修改窗口标题的字体大小
            GUI.skin.window.fontSize = 20;

            if (windowShow)
            {
                windowRect = GUILayout.Window(1, windowRect, WindowFunc, "CheatMenu");
            }
        }
        /// <summary>
        /// 菜单内的显示内容，所有的修改内容均在这里展示
        /// </summary>
        /// <param name="id"></param>
        public void WindowFunc(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X",GUILayout.Width(20)))//这里结合上面的FlexbleSpace是给按钮布局
            {
                windowShow = false;
            }
            GUILayout.EndHorizontal();

            testToolbarIndex = GUILayout.Toolbar(testToolbarIndex, testToolbarNames);
            CharaData charaData = SharedData.Instance(false).GetCharaData(SharedData.Instance(false).playerid);
            switch (testToolbarIndex)
            {
                case 0:
                    GUILayout.BeginHorizontal();
                    GUIPropertyShow("臂力", "STR");
                    GUIPropertyShow("定力", "WIL");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUIPropertyShow("敏捷", "AGI");
                    GUIPropertyShow("悟性", "LER");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUIPropertyShow("根骨", "BON");
                    GUIPropertyShow("道德", "MOR");
                    GUILayout.EndHorizontal();
                    GUILayout.Label("上面增加的五维属性不会对应增加基础属性如血内攻防等");
                    GUILayout.Label("若要对应基础属性增加，请使用天赋点加点");
                    GUILayout.BeginHorizontal();
                    GUIMultiPropertyShow("攻击力", "ATK");
                    GUIMultiPropertyShow("防御力", "DEF");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUIMultiPropertyShow("血量", "HP");
                    GUIMultiPropertyShow("内力", "MP");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUIMultiPropertyShow("轻功", "SP");
                    GUIMultiPropertyShow("暴击率", "Crit");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUIMultiPropertyShow("暴击伤害", "Crit1");
                    GUIMultiPropertyShow("连击率", "Combo");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUIMultiPropertyShow("剑法", "Sword");
                    GUIMultiPropertyShow("刀法", "Knife");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUIMultiPropertyShow("棍杖", "Stick");
                    GUIMultiPropertyShow("拳掌", "Hand");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUIMultiPropertyShow("指力", "Finger");
                    GUIMultiPropertyShow("特殊", "Special");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUIMultiPropertyShow("奇门", "YinYang");
                    GUIMultiPropertyShow("音律", "Melody");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("这里获取锻造的字段失败导致无法修改锻造");
                    //GUIMultiPropertyShow("锻造", "Making");
                    GUIMultiPropertyShow("酒艺", "Wineart");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUIMultiPropertyShow("暗器", "Darts");
                    GUIMultiPropertyShow("盗术", "Steal");
                    GUILayout.EndHorizontal();
                    break;
                case 1:
                    if(GUILayout.Button("先点击这个来初始化物品列表，然后才能搜索"))
                    {
                        Logger.LogInfo("物品列表初始化中...");
                        //实例化物品列表并给出所有物品
                        b07 = CommonResourcesData.b07.GetRowList();
                        itemNames = new List<string>();

                        foreach (var item in b07)
                        {
                            itemNames.Add(item.Name);
                        }
                        Logger.LogInfo("物品列表初始化成功");
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("物品名:");

                    searchText = GUILayout.TextField(searchText);
                    if (GUILayout.Button("搜索"))
                    {
                        SearchItems(searchText);
                    }
                    GUILayout.EndHorizontal();

                    // 显示搜索结果
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(300), GUILayout.Height(400));
                    foreach (var item in searchResults)
                    {
                        if (GUILayout.Button(item.Name))
                        {
                            SharedData.Instance(false).PackageAdd(item.ID, 1);
                            Debug.Log($"添加 {item.Name} 到背包中");
                        }
                    }
                    GUILayout.EndScrollView();
                    break;
                case 2:

                    if (GUILayout.Button("点击这个来初始化特征列表，然后才能搜索"))
                    {
                        Logger.LogInfo("特征列表初始化中...");
                        b06 = CommonResourcesData.b06.GetRowList();
                        traitNames = new List<string>();
                        foreach(var item in b06)
                        {
                            traitNames.Add(item.name);
                        }
                        Logger.LogInfo("特征列表初始化完成");
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("特征名:");
                    searchText = GUILayout.TextField(searchText);
                    if (GUILayout.Button("搜索"))
                    {
                        SearchTraits(searchText);
                    }
                    GUILayout.EndHorizontal();

                    //显示搜索结果
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(300), GUILayout.Height(400));
                    foreach(var item in searchResults1)
                    {
                        if(GUILayout.Button(item.name))
                        {
                            charaData.AddTraits(item.id);
                            Debug.Log($"添加 {item.name} 特征");
                        }
                    }
                    GUILayout.EndScrollView();
                    break;
                case 3:

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("修改天赋点：");
                    GUILayout.Space(10);

                    if (GUILayout.Button("+1"))
                    {
                        charaData.m_Talent++;
                    }
                    GUILayout.Space(10);

                    if (GUILayout.Button("-1"))
                    {
                        charaData.m_Talent--;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("修改金钱：");
                    inputString = GUILayout.TextField(inputString);

                    GUILayout.Space(10);
                    if (GUILayout.Button("+"))
                    {
                        int inputValue;
                        if (int.TryParse(inputString, out inputValue))
                        {
                            SharedData.Instance(false).m_Money += inputValue;
                        }
                        else
                        {
                            Debug.LogWarning("输入无效，请输入一个有效的数字。");
                        }
                    }
                    if (GUILayout.Button("-"))
                    {
                        int inputValue;
                        if (int.TryParse(inputString, out inputValue))
                        {
                            SharedData.Instance(false).m_Money -= inputValue;
                        }
                        else
                        {
                            Debug.LogWarning("输入无效，请输入一个有效的数字。");
                        }
                    }
                    GUILayout.Space(10);

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("修改经验：");
                    inputString = GUILayout.TextField(inputString);

                    GUILayout.Space(10);
                    if (GUILayout.Button("+"))
                    {
                        int inputValue;
                        if (int.TryParse(inputString, out inputValue))
                        {
                            charaData.m_Exp += inputValue;
                        }
                        else
                        {
                            Debug.LogWarning("输入无效，请输入一个有效的数字。");
                        }
                    }
                    if (GUILayout.Button("-"))
                    {
                        int inputValue;
                        if (int.TryParse(inputString, out inputValue))
                        {
                            charaData.m_Exp -= inputValue;
                        }
                        else
                        {
                            Debug.LogWarning("输入无效，请输入一个有效的数字。");
                        }
                    }
                    GUILayout.Space(10);

                    GUILayout.EndHorizontal();

                    preventItemReduction = GUILayout.Toggle(preventItemReduction,"在场景中打开背包使用物品，则物品不减少");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("战斗中出招获得额外武功熟练度点数：");
                    input = GUILayout.TextField(input);
                    if(GUILayout.Button("应用"))
                    {
                        int inputValue;
                        if(int.TryParse(input,out inputValue))
                        {
                            extraExpValue = inputValue;
                        }
                        else
                        {
                            GUILayout.Label("输入无效，请输入一个有效的数字。");
                            input = "0";
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Label("武功列表,遗忘武功无法消除增加的属性");
                    GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(400), GUILayout.Height(400));
                    var kongfuListCopy = charaData.m_KongFuList.ToList(); // 创建集合的副本
                    foreach (var kongfu in kongfuListCopy)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(kongfu.kf.Name);
                        if (GUILayout.Button("遗忘"))
                        {
                            charaData.KongFuListRemove(kongfu); // 现在可以安全地删除元素
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndScrollView();
                    break;
                default:
                    break;
            }
            GUI.DragWindow();

            //testFloat = GUILayout.HorizontalSlider(testFloat, 0, 10);//水平滑动条
            //GUILayout.Label(testToolbarIndex.ToString());//显示ToolBar索引
            //GUILayout.Label("Hello World");
            //if(GUILayout.Button("增加"))
            //{
            //    Debug.Log("点击按钮");
            //}

            //scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);
            //GUILayout.Label("输入文本");
            //teststring = GUILayout.TextField(teststring);
            //testBool = GUILayout.Toggle(testBool, "某开关");
            //GUILayout.Label("输入文本");
            //GUILayout.Label("输入文本");
            //GUILayout.BeginHorizontal();
            //GUILayout.Label("输入文本");
            //GUILayout.Label("输入文本");
            //GUILayout.Label("输入文本");
            //GUILayout.EndHorizontal();

            //GUILayout.EndScrollView();

        }

        /// <summary>
        /// 查找物品
        /// </summary>
        /// <param name="searchText">物品名称</param>
        private void SearchItems(string searchText)
        {
            // 清空之前的搜索结果
            searchResults.Clear();

            if (b07 == null || b07.Count == 0)
            {
                GUILayout.Label("物品列表未初始化或为空");
                return;
            }

            foreach (var item in b07)
            {
                if (item.Name.Contains(searchText))
                {
                    searchResults.Add(item);
                }
            }

            if (searchResults.Count == 0)
            {
                GUILayout.Label("没有匹配到任何物品");
            }
        }
        /// <summary>
        /// 搜索特征
        /// </summary>
        /// <param name="searchText"></param>
        private void SearchTraits(string searchText)
        {
            // 清空之前的搜索结果
            searchResults1.Clear();

            if (b06 == null || b06.Count == 0)
            {
                GUILayout.Label("物品列表未初始化或为空");
                return;
            }

            foreach (var item in b06)
            {
                if (item.name.Contains(searchText))
                {
                    searchResults1.Add(item);
                }
            }

            if (searchResults1.Count == 0)
            {
                GUILayout.Label("没有匹配到任何物品");
            }
        }

        /// <summary>
        /// 单个六维值显示
        /// </summary>
        /// <param name="name">属性值描述</param>
        /// <param name="field">属性值字段</param>
        public void GUIPropertyShow(string name,string field)
        {
            GUILayout.Label(name);
            CharaData charaData = SharedData.Instance(false).GetCharaData(SharedData.Instance(false).playerid);
            GUILayout.Space(10);
            GUILayout.Label(charaData.GetFieldValueByName(field).ToString());
            GUILayout.Space(10);
            if (GUILayout.Button("+1"))
            {
                charaData.Indexs_Name[field].alterValue++;
            }
            GUILayout.Space(10);
            if (GUILayout.Button("-1"))
            {
                charaData.Indexs_Name[field].alterValue--;
            }
            GUILayout.Space(20);
        }
        public void GUIMultiPropertyShow(string name, string field)
        {
            GUILayout.Label(name);
            CharaData charaData = SharedData.Instance(false).GetCharaData(SharedData.Instance(false).playerid);
            GUILayout.Space(10);
            GUILayout.Label(charaData.GetFieldValueByName(field).ToString());
            GUILayout.Space(10);

            inputString = GUILayout.TextField(inputString);

            GUILayout.Space(10);
            if (GUILayout.Button("+"))
            {
                float inputValue;
                if (float.TryParse(inputString, out inputValue))
                {
                    charaData.Indexs_Name[field].alterValue += inputValue;
                }
                else
                {
                    Debug.LogWarning("输入无效，请输入一个有效的数字。");
                }
            }

            GUILayout.Space(10);
            if (GUILayout.Button("-"))
            {
                float inputValue;
                if (float.TryParse(inputString, out inputValue))
                {
                    charaData.Indexs_Name[field].alterValue -= inputValue;
                }
                else
                {
                    Debug.LogWarning("输入无效，请输入一个有效的数字。");
                }
            }
        }
        [HarmonyPrefix,HarmonyPatch(typeof(PackageController), "UseSelectedItemOnField")]
        //[HarmonyPatch(new Type[] { typeof(int).MakeByRefType(), typeof(string).MakeByRefType() })]这种方法针对字段有ref关键字的情况，即无法直接用Traverse来获取字段/方法
        //[HarmonyPatch(new ArgumentType[] { ArgumentType.Ref(typeof(int)), ArgumentType.Out(typeof(string)) })]专门匹配函数参数有ref关键字的情况，关键是在ref、out等修饰符位置放Ref、Out
        public static void PackageController_UseSelectedItemOnField_Patch(PackageController __instance)
        {
            //这两行是.NET的反射特性，用来获取私有字段
            //FieldInfo selectedItemField = typeof(PackageController).GetField("SelectedPackageItem", BindingFlags.NonPublic | BindingFlags.Instance);
            //var selectedItem = (PackageItemData)selectedItemField.GetValue(__instance);

            //Traverse.Create(obj).Field("fieldName").GetValue<T>();fieldName就是要访问的私有字段名称
            //Traverse.Create(obj).Method("methodName", args).GetValue<T>();调用私有方法
            var selectedItem = Traverse.Create(__instance).Field("SelectedPackageItem").GetValue<PackageItemData>();//使用Harmony自带的Traverse获取私有字段

            if (selectedItem != null)
            {
                // 获取 b07row
                gang_b07Table.Row b07row = selectedItem.b07Row;

                // 检查配置项，如果为 true，则在减少物品之前增加物品数量
                if (preventItemReduction)
                {
                    SharedData.Instance(false).PackageAdd(b07row.ID, 1);
                }
            }
            else
            {
                Debug.LogError("SelectedPackageItem字段无法获取");
            }
        }
        [HarmonyPostfix, HarmonyPatch(typeof(BattleObject), "PlayEffectAll")]
        public static void BattleObject_PlayEffectAll_Patch(BattleObject __instance)
        {
            int extraexp = CheatMenu.extraExpValue;
            if("player".Equals(__instance.race))
            {
                Debug.Log("成功增加额外经验");
                __instance.m_SkillRow.proficiency += extraexp;
            }
            else
            {
                //Debug.LogError("增加经验失败，方法条件错误或攻击者非友方");
            }
        }
    }
}
