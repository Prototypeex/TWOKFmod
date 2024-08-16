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
                SharedData.Instance(false).m_Money += 1000;          
            }
            if (Input.GetKeyDown(hotkey1.Value))
            {
                //获取玩家的数据
                CharaData charaData = SharedData.Instance(false).GetCharaData(SharedData.Instance(false).playerid);
                charaData.m_Exp += 500;
                charaData.m_Talent += 1;
                //charaData.Indexs_Name["MOR"].alterValue++;//这个只建议修改道德，然后用天赋点来加别的点


            }
        }
        [HarmonyPrefix,HarmonyPatch(typeof(PackageController), "UseSelectedItemOnField")]
        public static void PackageController_UseSelectedItemOnField_Patch(PackageController __instance)
        {
            FieldInfo selectedItemField = typeof(PackageController).GetField("SelectedPackageItem", BindingFlags.NonPublic | BindingFlags.Instance);
            var selectedItem = (PackageItemData)selectedItemField.GetValue(__instance);
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
