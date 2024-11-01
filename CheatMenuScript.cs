using System;
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
using UnityEngine.UI;
using System.Globalization;

namespace CheatMenu
{
    [BepInPlugin("me.Prototypehu.plugin.CheatMenu","Cheatmenu","0.0.1")]
    public class CheatMenu:BaseUnityPlugin
    {
        ConfigEntry<KeyCode> hotkey;//这个定义+下面的Config.Bind就可以在启动后生成配置文件，进而在配置中手动修改快捷键
        ConfigEntry<KeyCode> hotkey1;
        //ConfigEntry<float> configFloat;//修改数值
        //ConfigEntry<float> configFloat1;
        //ConfigEntry<string> configString;
        public static bool preventItemReduction;
        public static bool modifyLevelLimit;
        public static bool oneHitWugongMax;
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
        private string[] testToolbarNames = new string[] { "属性", "物品","特征","其他" };
        private int testToolbarIndex = 0;
        private float testFloat;
        private List<gang_b07Table.Row> searchResults = new List<gang_b07Table.Row>();
        private List<gang_b06Table.Row> searchResults1 = new List<gang_b06Table.Row>();
        private Vector2 scrollViewPos;
        private bool windowShow = false;
        private static bool _hasProcessedUseItemID = false;

        void Start()
        {
            hotkey = Config.Bind<KeyCode>("config","hotkey",KeyCode.Tab,"CheatMenu菜单显示");//默认的快捷键是Tab
            hotkey1 = Config.Bind<KeyCode>("config","hotkey1",KeyCode.T,"测试修改功能的快捷键，无需理会");
            Logger.LogInfo("CheatMenu初始化中...");
            Harmony.CreateAndPatchAll(typeof(CheatMenu));//Harmony注入，用于即时性修改游戏内方法调用逻辑
            Harmony.CreateAndPatchAll(typeof(MapHighLight));
            Harmony.CreateAndPatchAll(typeof(DisplayTraitChains));
            Harmony.CreateAndPatchAll(typeof(StealPatch));
            Harmony.CreateAndPatchAll(typeof(SpecialEnemyPatch));
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
                CharaData charaData = SharedData.Instance(false).CurrentCharaData;
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
        /// GUI显示经验：不能嵌套GUILayout，必须在第一层显示。
        /// 通过交互来显示的元素，则先定义存储变量的数据结构，在GUI中获取，然后在第一层GUI中获取这些数据并显示(具体来说，显示的代码一直在刷新，获取数据的操作交给玩家来手动)
        /// 其原理是GUI是不断刷新的，所以不能嵌套。但是数据是可以固定的，GUI获取的是当前的数据，而数据更新后GUI跟着更新
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
            if(SharedData.Instance(false).CurrentCharaData==null)
            {
                SharedData.Instance(false).CurrentCharaData = SharedData.Instance(false).GetCharaData(SharedData.Instance(false).playerid);
            }
            CharaData charaData = SharedData.Instance(false).CurrentCharaData;
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
                    GUIMultiPropertyShow("锻造", "Forge");
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

                    preventItemReduction = GUILayout.Toggle(preventItemReduction,"在场景/战斗中使用物品，则物品不减少");
                    oneHitWugongMax = GUILayout.Toggle(oneHitWugongMax,"出招一次，武功升级到上限（受武功上限和定力约束）");
                    modifyLevelLimit= GUILayout.Toggle(modifyLevelLimit,"修改自创武功等级上限为30，星级上限为8(不设为10怕超出上限报错)");
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
                    GUILayout.Label("武功列表,方便快速遗忘武功");
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(400), GUILayout.Height(400));
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
            CharaData charaData = SharedData.Instance(false).CurrentCharaData;
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
            CharaData charaData = SharedData.Instance(false).CurrentCharaData;
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
        /// <summary>
        /// 在和平场景中使用物品不减少
        /// </summary>
        /// <param name="__instance"></param>
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
        /// <summary>
        /// 设置自创武功等级上限为30
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix, HarmonyPatch(typeof(CreateWGController), "Start")]
        public static void ModifyWGLimit(CreateWGController __instance)
        {
            Traverse traverse = Traverse.Create(__instance);
            traverse.Field("levelLimit").SetValue(modifyLevelLimit ? 30 : 9);
            traverse.Field("starsLimit").SetValue(modifyLevelLimit ? 8 : 6);
        }

        /// <summary>
        /// 出招增加额外经验
        /// </summary>
        /// <param name="__instance"></param>
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
        /// <summary>
        /// 出招武功满级，注入并用oneHitWugongMax控制是否替换原武功升级检查的方法
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPrefix, HarmonyPatch(typeof(BattleObject), "SkillLevelUpCheckProcess")]
        public static bool BattleObject_SkillLevelUpCheckProcess_Patch(BattleObject __instance)
        {
            // 初始检查
            UnityEngine.Debug.Log("SkillLevelUpCheckProcess_Patch called");

            if (oneHitWugongMax)
            {
                UnityEngine.Debug.Log("oneHitWugongMax is enabled");

                // 初始条件检查
                bool flag = (__instance.charadata.m_Table != "b01" && !SharedData.Instance(false).FollowList.Contains(__instance.charadata.m_Id)) || __instance.charadata.originRace != "player";
                UnityEngine.Debug.Log($"Initial flag check: {flag}");

                if (!flag)
                {
                    // 确保 __instance.m_SkillRow 和 __instance.m_SkillRow.kf 不为空
                    if (__instance.m_SkillRow == null)
                    {
                        UnityEngine.Debug.Log("m_SkillRow is null");
                        return true;
                    }
                    if (__instance.m_SkillRow.kf == null)
                    {
                        UnityEngine.Debug.Log("m_SkillRow.kf is null");
                        return true;
                    }

                    int currentlv = __instance.m_SkillRow.lv;
                    string maxlv = __instance.m_SkillRow.kf.LV;
                    float wil = __instance.charadata.GetFieldValueByName("WIL");

                    bool flag2 = currentlv < int.Parse(maxlv) && (float)currentlv < wil;

                    if (flag2)
                    {
                        __instance.m_SkillRow.newlv = float.Parse(maxlv) > wil ? (int)wil : int.Parse(maxlv);
                        __instance.m_SkillRow.CheckAppendTraits(__instance.charadata);
                        bool flag3 = !SharedData.Instance(false).skillLevelupObjList.Contains(__instance);

                        if (flag3)
                        {
                            SharedData.Instance(false).skillLevelupObjList.Add(__instance);
                        }

                        __instance.charadata.m_LevelUpSkillId = __instance.m_SkillRow.kf.ID;
                        __instance.m_BattleController.ShowLevelUpSkill();
                        __instance.SetBattleObjState(BattleObjectState.SkillLevelUpNeet);
                    }
                    else
                    {
                        bool flag4 = __instance.m_FinalAddExp > 0;

                        if (flag4)
                        {
                            bool flag5 = !"".Equals(__instance.charadata.m_Training_Id);

                            if (flag5)
                            {
                                KongFuData training_kf = __instance.charadata.GetKongFuByID(__instance.charadata.m_Training_Id);
                                float learn = __instance.charadata.GetFieldValueByName("LER");
                                bool flag6 = training_kf != null;

                                if (flag6)
                                {
                                    training_kf.newlv = training_kf.lv;
                                    bool flag7 = training_kf.lv < int.Parse(training_kf.kf.LV) && training_kf.CheckLevelUp((float)__instance.m_FinalAddExp, __instance.charadata.GetFieldValueByName("WIL"), learn, __instance.charadata);

                                    if (flag7)
                                    {
                                        bool flag8 = !SharedData.Instance(false).skillLevelupObjList.Contains(__instance);

                                        if (flag8)
                                        {
                                            SharedData.Instance(false).skillLevelupObjList.Add(__instance);
                                        }

                                        __instance.charadata.m_LevelUpSkillId = __instance.charadata.m_Training_Id;
                                        __instance.m_BattleController.ShowLevelUpSkill();
                                        __instance.SetBattleObjState(BattleObjectState.SkillLevelUp);
                                    }
                                    else
                                    {
                                        SharedData.Instance(false).m_BattleController.LevelUpCheckProcess();
                                    }
                                }
                                else
                                {
                                    SharedData.Instance(false).m_BattleController.LevelUpCheckProcess();
                                }
                            }
                            else
                            {
                                SharedData.Instance(false).m_BattleController.LevelUpCheckProcess();
                            }
                        }
                    }
                }
                return false; // Block original method
            }
            else
            {
                return true; // Allow original method
            }
        }
        /// <summary>
        /// 战斗中使用物品不减少
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPrefix,HarmonyPatch(typeof(BattleObject), "State_Complete")]
        public static void BattleObject_State_Complete_Patch(BattleObject __instance)
        {

            if (__instance == null)
            {
                Debug.LogError("BattleObject instance is null in the patch.");
                return;
            }

            bool flag7 = "ITEM".Equals(__instance.m_AttackType?[0]);
            if (flag7)
            {
                string useItemID = SharedData.Instance(false)?.useItemID;
                if (!string.IsNullOrEmpty(useItemID)&&!_hasProcessedUseItemID)
                {
                    if(preventItemReduction)
                    {
                        SharedData.Instance(false).PackageAdd(useItemID, 1);
                        _hasProcessedUseItemID = true;
                    }
                }
                else if(string.IsNullOrEmpty(useItemID)&&_hasProcessedUseItemID)
                {
                    _hasProcessedUseItemID = false;
                }
            }
        }
    }
    public static class MapHighLight
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapController), "Start")]
        public static void MapController_Start_Postfix(MapController __instance)
        {
            foreach (MapController.Event @event in __instance.GetFieldValue<Dictionary<string,MapController.Event>>("events").Values)
            {
                if (!(@event.obj == null))
                {
                    gang_e01Table.Row evdata = @event.evdata;
                    string[] array;
                    if (evdata == null)
                    {
                        array = null;
                    }
                    else
                    {
                        string action = evdata.action;
                        array = ((action != null) ? action.Split(new char[]
                        {
                            '|'
                        }) : null);
                    }
                    string[] array2 = array;
                    if (array2 != null && array2[0] == "GET" && @event.evdata.display == "Prefabs/Field/Dummy")
                    {
                        @event.evdata.display = "Prefabs/Effect/GroundLight";
                    }
                }
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(gang_e01Table), "Load")]
        public static void gang_e01TableLoadPatch(gang_e01Table __instance, TextAsset csv)
        {
            List<gang_e01Table.Row> fieldValue = __instance.GetFieldValue<List<gang_e01Table.Row>>("rowList");
            for (int i = 0; i < fieldValue.Count; i++)
            {
                string[] array = fieldValue[i].action.Split(new char[]
                {
            '|'
                });
                if (fieldValue[i].trigger == "CLICK" && array[0] == "GET" && fieldValue[i].display == "Prefabs/Field/Dummy")
                {
                    fieldValue[i].display = "Prefabs/Effect/GroundLight";
                }
            }
        }
    }

    public static class DisplayTraitChains
    {
        private static readonly Dictionary<string, List<gang_b06ChainTable.Row>> _chainsCache = new Dictionary<string, List<gang_b06ChainTable.Row>>();
        private static readonly Dictionary<string, List<string>> _hintStringsCache = new Dictionary<string, List<string>>();
        public static List<string> ConvertChainsToHintStrings(string traitId, List<gang_b06ChainTable.Row> chains)
        {
            if (DisplayTraitChains._hintStringsCache.ContainsKey(traitId))
            {
                return DisplayTraitChains._hintStringsCache[traitId];
            }
            List<string> list = new List<string>();
            foreach (gang_b06ChainTable.Row row in chains)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 1; i < 10; i++)
                {
                    string text = row.nineGridDict[i.ToString()];
                    if (!text.Equals("0") && !text.Split(new char[]
                    {
                '|'
                    }).Contains(traitId))
                    {
                        string conditionNameHint = DisplayTraitChains.GetConditionNameHint(text);
                        stringBuilder.Append(DisplayTraitChains.GetChineseNumeral(i) + ": " + conditionNameHint + " + ");
                    }
                }
                if (stringBuilder.Length > 0)
                {
                    string item = "【" + row.name + "】：" + stringBuilder.ToString().TrimEnd(new char[]
                    {
                ' ',
                '+'
                    });
                    list.Add(item);
                }
            }
            DisplayTraitChains._hintStringsCache[traitId] = list;
            return list;
        }
        public static List<gang_b06ChainTable.Row> FindChainsContainingTrait(string traitId)
        {
            if (DisplayTraitChains._chainsCache.ContainsKey(traitId))
            {
                return DisplayTraitChains._chainsCache[traitId];
            }
            List<gang_b06ChainTable.Row> list = new List<gang_b06ChainTable.Row>();
            foreach (gang_b06ChainTable.Row row in CommonResourcesData.b06Chain.GetRowList())
            {
                for (int i = 1; i < 10; i++)
                {
                    if (row.nineGridDict[i.ToString()].Split(new char[]
                    {
                '|'
                    }).Contains(traitId))
                    {
                        list.Add(row);
                        break;
                    }
                }
            }
            DisplayTraitChains._chainsCache[traitId] = list;
            return list;
        }
        private static string GetChineseNumeral(int number)
        {
            switch (number)
            {
                case 1:
                    return "一";
                case 2:
                    return "二";
                case 3:
                    return "三";
                case 4:
                    return "四";
                case 5:
                    return "五";
                case 6:
                    return "六";
                case 7:
                    return "七";
                case 8:
                    return "八";
                case 9:
                    return "九";
                default:
                    return number.ToString();
            }
        }
        private static string GetConditionNameHint(string condition)
        {
            if (condition == "Any")
            {
                return "任意特性";
            }
            if (condition == "NAN")
            {
                return "无特性";
            }
            List<string> values = (from id in condition.Split(new char[]
            {
        '|'
            })
                                   select CommonResourcesData.b06.Find_id(id).name_Trans).ToList<string>();
            return string.Join(" | ", values);
        }
        public static List<string> GetMissingTraitHintsForTraitId(string traitId)
        {
            if (DisplayTraitChains._hintStringsCache.ContainsKey(traitId))
            {
                return DisplayTraitChains._hintStringsCache[traitId];
            }
            List<gang_b06ChainTable.Row> chains = DisplayTraitChains.FindChainsContainingTrait(traitId);
            List<string> list = DisplayTraitChains.ConvertChainsToHintStrings(traitId, chains);
            DisplayTraitChains._hintStringsCache[traitId] = list;
            return list;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TraitPackageController), "RefreshTraitInfo")]
        public static void TraitPackageController_RefreshTraitInfo_Postfix(TraitPackageController __instance, GameObject traitIcon)
        {
            Transform fieldValue = __instance.GetFieldValue<Transform>("TraitInfo");
            Text text;
            if (fieldValue == null)
            {
                text = null;
            }
            else
            {
                Transform transform = fieldValue.Find("TraitInfo/Info");
                text = ((transform != null) ? transform.GetComponent<Text>() : null);
            }
            Text text2 = text;
            TraitIconController componentInParent = traitIcon.GetComponentInParent<TraitIconController>();
            string text3;
            if (componentInParent == null)
            {
                text3 = null;
            }
            else
            {
                TraitItemData traitItemData = componentInParent.traitItemData;
                if (traitItemData == null)
                {
                    text3 = null;
                }
                else
                {
                    gang_b06Table.Row b06Row = traitItemData.b06Row;
                    text3 = ((b06Row != null) ? b06Row.id : null);
                }
            }
            string text4 = text3;
            if (text2 == null || text4 == null)
            {
                return;
            }
            List<string> missingTraitHintsForTraitId = DisplayTraitChains.GetMissingTraitHintsForTraitId(text4);
            Text text5 = text2;
            text5.text = text5.text + "\n<size=16>" + string.Join("\n", missingTraitHintsForTraitId) + "</size>";
        }
    }

    public static class StealPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SkillTraitEquipManager), "RunSkill")]
        public static void SkillTraitEquipManager_RunSkill_Prefix(BattleObject _attacker, BattleObject _defender)
        {
            if (_attacker.race != "player" || _defender.race == "player")
            {
                return;
            }
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < _defender.m_StealableItemsName.Count; i++)
            {
                if (_defender.m_StealableItemsNum[i] != 0)
                {
                    string item = _defender.m_StealableItemsName[i].Item;
                    int num = _defender.m_StealableItemsNum[i];
                    SharedData.Instance(false).PackageAdd(item, num);
                    if (dictionary.ContainsKey(item))
                    {
                        Dictionary<string, int> dictionary2 = dictionary;
                        string key = item;
                        dictionary2[key] += num;
                    }
                    else
                    {
                        dictionary.Add(item, num);
                    }
                    _defender.m_StealableItemsNum[i] = 0;
                }
            }
            string str = _attacker.charadata.Indexs_Name["Name"].stringValue + " " + CommonFunc.I18nGetLocalizedValue("I18N_StalSuccess");
            List<string> list = new List<string>();
            string text = string.Empty;
            foreach (KeyValuePair<string, int> keyValuePair in dictionary)
            {
                gang_b07Table.Row row = CommonResourcesData.b07.Find_ID(keyValuePair.Key);
                if (row != null)
                {
                    list.Add(string.Format("<color=#f0e352>{0}</color> * {1}", row.Name_Trans, keyValuePair.Value));
                    text = row.BookIcon;
                }
            }
            if (list.Count > 0)
            {
                string text2 = str + " " + string.Join(", ", list) + "！";
                _attacker.m_MenuController.OpenInteruptInfo(_attacker, text2, text);
                _attacker.InteruptIn(BattleObjectState.ActionOver);
            }
        }
    }
    public static class SpecialEnemyPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(gang_b04RandomFightInfos), "Load")]
        public static void gang_b04RandomFightInfos_Load_Postfix(gang_b04RandomFightInfos __instance, TextAsset csv)
        {
            foreach (gang_b04RandomFightInfos.Row row in __instance.GetFieldValue<List<gang_b04RandomFightInfos.Row>>("rowList"))
            {
                if (!(row.Unique == "0"))
                {
                    row.RequireStep = "0";
                    row.BattleRate = (row.BattleRate.ToFloat() * 10f).ToString();
                }
            }
        }

        public static float ToFloat(this string str)
        {
            float result;
            if (!float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                Debug.LogErrorFormat("Could not convert '{0}' to float.", new object[]
                {
                    str
                });
            }
            return result;
        }
    }

    public static class ReflectionExtensions
    {
        public static T GetFieldValue<T>(this object instance, string fieldname)
        {
            T result;
            try
            {
                FieldInfo fieldInfo = AccessTools.Field(instance.GetType(), fieldname);
                result = (T)((object)((fieldInfo != null) ? fieldInfo.GetValue(instance) : null));
            }
            catch (Exception arg)
            {
                
                result = default(T);
            }
            return result;
        }


    }


}
