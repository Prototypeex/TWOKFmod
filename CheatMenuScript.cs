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
        public static ConfigEntry<int> configInt;
        ConfigEntry<float> configFloat;
        ConfigEntry<float> configFloat1;
        ConfigEntry<string> configString;
        public static ConfigEntry<bool> preventItemReduction;

        private Rect windowRect = new Rect(50, 50, 500, 300);
        private string teststring;
        private bool testBool = false;
        private string[] testToolbarNames = new string[] { "属性", "其他" };
        private int testToolbarIndex = 0;
        private float testFloat;
        private Vector2 scrollViewPos;
        private bool windowShow = false;

        void Start()
        {
            hotkey = Config.Bind<KeyCode>("config","hotkey",KeyCode.Y,"增加1000钱");//默认的快捷键是Y
            hotkey1 = Config.Bind<KeyCode>("config","hotkey1",KeyCode.T,"增加500经验");//默认的快捷键是T
            configInt = Config.Bind<int>("config","skillexpamount",20,"武功额外熟练度,0-100");//默认额外+1
            preventItemReduction = Config.Bind<bool>("config","isItemReductionInField",true,"true为不减，false为默认减少");
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
            if(windowShow)//约束mod菜单的渲染
            {
                windowRect = GUILayout.Window(1, windowRect, WindowFunc, "菜单");
            }
        }

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
            testFloat = GUILayout.HorizontalSlider(testFloat, 0, 10);
            //GUILayout.Label(testToolbarIndex.ToString());
            GUILayout.Label("Hello World");
            if(GUILayout.Button("增加"))
            {
                Debug.Log("点击按钮");
            }

            scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);
            GUILayout.Label("输入文本");
            teststring = GUILayout.TextField(teststring);
            testBool = GUILayout.Toggle(testBool, "某开关");
            GUILayout.Label("输入文本");
            GUILayout.Label("输入文本");
            GUILayout.BeginHorizontal();
            GUILayout.Label("输入文本");
            GUILayout.Label("输入文本");
            GUILayout.Label("输入文本");
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUI.DragWindow();

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
                if (preventItemReduction.Value)
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
            int extraexp = CheatMenu.configInt.Value;
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
