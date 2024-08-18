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

        private Rect windowRect = new Rect(50, 50, 800, 480);
        private string inputString = "0";
        private static string input = "0";
        private string teststring;
        private bool testBool = false;
        private string[] testToolbarNames = new string[] { "属性", "其他" };
        private int testToolbarIndex = 0;
        private float testFloat;
        private Vector2 scrollViewPos;
        private bool windowShow = false;

        void Start()
        {
            hotkey = Config.Bind<KeyCode>("config","hotkey",KeyCode.Tab,"CheatMenu菜单显示");//默认的快捷键是Y
            hotkey1 = Config.Bind<KeyCode>("config","hotkey1",KeyCode.T,"测试修改功能的快捷键，无需理会");
            Debug.Log("Prototypehu:Hello World");
            Logger.LogInfo("BepInEx:Hello World");
            Harmony.CreateAndPatchAll(typeof(CheatMenu));
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
        /// 菜单内的显示内容
        /// </summary>
        /// <param name="id"></param>
        public void WindowFunc(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X",GUILayout.Width(22)))//这里结合上面的FlexbleSpace是给按钮布局
            {
                windowShow = false;
            }
            GUILayout.EndHorizontal();

            testToolbarIndex = GUILayout.Toolbar(testToolbarIndex, testToolbarNames);
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
                    GUIPropertyShow("攻击力", "ATK");
                    GUIPropertyShow("防御力", "DEF");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUIMultiPropertyShow("血量", "HP");
                    GUIMultiPropertyShow("内力", "MP");
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
                    GUIMultiPropertyShow("奇门", "Finger");
                    GUIMultiPropertyShow("音律", "Special");
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
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("修改天赋点：");
                    GUILayout.Space(10);

                    CharaData charaData = SharedData.Instance(false).GetCharaData(SharedData.Instance(false).playerid);
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
