using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BreakingModify
{

    public class Settings : UnityModManager.ModSettings
    {

        //人相关
        public bool allYoung = false;
        public int allAge = 14;
        public bool villageYoung = false;
        public int villageAge = 14;
        public bool mianActorKeepLunhui = false;
        public String villageGoodnessStr = "";
        public String villageMoodStr = "";
        public String villageFameStr = "";
        public String villageCharmStr = "";
        public String villageFavorStr = "";
        //战斗相关
        public int allEffectType = 0;
        public List<int> gongfaEffectArr = null;
        //建筑相关
        public bool lockTime = false;
        public bool studyWithNoAgeLimit = false;
        public int increseMaxManPowerType = 1;
        public int maxManPowerNum = 100;
        public int increseBuildingNum = 3;
        public int baseBuildingNum = 10;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
    }


    public static class Main
    {
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        //人相关
        public static int nowActorId = 0;
        public static int setAge = 14;
        public static bool enabled;
        public static Dictionary<String, int> goodnessValue = new Dictionary<string, int>
        {
            {"刚正",75 },{"仁善",250},{"中庸",500},{"叛逆",750},{"唯我",900}
        };
        public static Dictionary<String, int> moodValue = new Dictionary<string, int>
        {
            {"悲极",-10 },{"痛苦",10},{"沮丧",30},{"寻常",50},{"开怀",70},{"欢喜",90},{"乐极",110}
        };
        public static Dictionary<String, int> fameValue = new Dictionary<string, int>
        {
            {"妖魔鬼怪",-170 },{"千夫所指",-120},{"名声败坏",-70},{"默默无闻",0},
            { "立身扬名",70},{"声驰千里",120 },{"誉满天下",170}
        };
        public static int favorBaseNum = 30000;
        public static Dictionary<String, int> favorValue = new Dictionary<string, int>
        {
            {"冷淡", favorBaseNum *10 / 100},{"融洽", favorBaseNum * 35/100},
            {"热枕", favorBaseNum *60 / 100},{"喜爱", favorBaseNum * 85 /100},
            {"亲密", favorBaseNum *140 / 100 },{"不渝", favorBaseNum * 200/100}
        };
        public static Dictionary<String, int> charmValue = new Dictionary<string, int>
        {
            {"非人" ,60},{"可憎",160},{"不扬",260},{"寻常",410},
            {"出众" ,560},{"瑾瑜",660},{"瑶碧",660},{"龙资",760},
            {"风仪",760 },{"绝世",860},{"出尘",860},{"天人",960}
        };
        //战斗相关
        public static string gongfaEffectStr = "";
        public static int effectPerRow = 3;
        public static bool isUpdatingText = false;
        //建筑相关
        public static bool inActorStudy = false;
        public static void setVillagePar(int actorId)
        {
            int goodnessValue = -1;
            if (Main.goodnessValue.TryGetValue(Main.settings.villageGoodnessStr, out goodnessValue))
            {
                DateFile.instance.actorsDate[actorId][16] = goodnessValue.ToString();
            }
            int moodValue = -1;
            if (Main.moodValue.TryGetValue(Main.settings.villageMoodStr, out moodValue))
            {
                DateFile.instance.actorsDate[actorId][4] = moodValue.ToString();
            }

            int charmValue = -1;
            if (Main.charmValue.TryGetValue(Main.settings.villageCharmStr, out charmValue))
            {
                DateFile.instance.actorsDate[actorId][15] = charmValue.ToString();
            }

            int fameValue = -10000;
            if (Main.fameValue.TryGetValue(Main.settings.villageFameStr, out fameValue))
            {
                DateFile.instance.actorsDate[actorId][18] = fameValue.ToString();
            }

            int favorValue = -10000;
            if (Main.favorValue.TryGetValue(Main.settings.villageFavorStr, out favorValue))
            {
                DateFile.instance.actorsDate[actorId][3] = favorValue.ToString();
            }
        }
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;
            if (settings.gongfaEffectArr == null)
            {
                settings.gongfaEffectArr = new List<int> { 266, 319, -1 };
            }
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }
        private static void setVillageModifyGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("太吾村村民立场锁定为：", GUILayout.Width(180));
            settings.villageGoodnessStr = GUILayout.TextField(settings.villageGoodnessStr, GUILayout.Width(60));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("太吾村村民魅力锁定为：", GUILayout.Width(180));
            settings.villageCharmStr = GUILayout.TextField(settings.villageCharmStr, GUILayout.Width(60));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("太吾村村民心情开始为：", GUILayout.Width(180));
            settings.villageMoodStr = GUILayout.TextField(settings.villageMoodStr, GUILayout.Width(60));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("太吾村村民好感开始为：", GUILayout.Width(180));
            settings.villageFavorStr = GUILayout.TextField(settings.villageFavorStr, GUILayout.Width(60));
            GUILayout.EndHorizontal();
            if (DateFile.instance.actorsDate != null && DateFile.instance.actorsDate.Count > 0)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("重设太吾村村民基础数据", GUILayout.Width(250)))
                {
                    foreach (int actorId in DateFile.instance.actorsDate.Keys)
                    {
                        int gangId = int.Parse(DateFile.instance.GetActorDate(actorId, 19, false));
                        if (gangId == 16)
                        {
                            setVillagePar(actorId);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private static void setLunhuiModifyGUI()
        {
            Main.settings.mianActorKeepLunhui = GUILayout.Toggle(Main.settings.mianActorKeepLunhui, "太吾传人必轮回到下一代上。", new GUILayoutOption[0]);
        }
        private static void setKeepYongGUI()
        {
            settings.allYoung = GUILayout.Toggle(settings.allYoung, "所有人外貌年龄可设置为固定值", new GUILayoutOption[0]);
            GUILayout.BeginHorizontal();
            GUILayout.Label("所有人外貌年龄固定值:", GUILayout.Width(200));
            String allAgeStr = settings.allAge.ToString();
            allAgeStr = GUILayout.TextField(allAgeStr, GUILayout.Width(45));
            if (!int.TryParse(allAgeStr, out settings.allAge))
            {
                settings.allAge = 0;
            }
            GUILayout.EndHorizontal();
            settings.villageYoung = GUILayout.Toggle(settings.villageYoung, "太吾村民外貌年龄可设置为固定值", new GUILayoutOption[0]);
            GUILayout.BeginHorizontal();
            GUILayout.Label("太吾村民外貌年龄固定值:", GUILayout.Width(200));
            String villageAgeStr = settings.villageAge.ToString();
            villageAgeStr = GUILayout.TextField(villageAgeStr, GUILayout.Width(45));
            if (!int.TryParse(villageAgeStr, out settings.villageAge))
            {
                settings.villageAge = 0;
            }
            GUILayout.EndHorizontal();

            if (nowActorId != 0)
            {
                Dictionary<int, string> dic = null;
                if (DateFile.instance.actorsDate.TryGetValue(nowActorId, out dic))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("将 <color=#FF0000>{0}</color>外貌年龄锁定为", DateFile.instance.GetActorName(nowActorId, true, false)),
                         GUILayout.Width(210));
                    String setAgeStr = setAge.ToString();
                    setAgeStr = GUILayout.TextField(setAgeStr, GUILayout.Width(45));
                    if (!int.TryParse(setAgeStr, out setAge))
                    {
                        setAge = 0;
                    }
                    GUILayout.Label("岁", GUILayout.Width(20));
                    if (GUILayout.Button("确定", GUILayout.Width(50)))
                    {
                        if (setAge >= 0)
                        {
                            if (DateFile.instance.actorsDate[nowActorId].Keys.Contains<int>(1000010))
                            {
                                DateFile.instance.actorsDate[nowActorId][1000010] = setAge.ToString();
                            }
                            else
                            {
                                DateFile.instance.actorsDate[nowActorId].Add(1000010, setAge.ToString());
                            }
                        }

                    }

                    else if (GUILayout.Button("取消锁定", GUILayout.Width(100)))
                    {
                        DateFile.instance.actorsDate[nowActorId].Remove(1000010);
                    }

                    else if (GUILayout.Button("清除所有数据", GUILayout.Width(150)))
                    {
                        foreach (int id in DateFile.instance.actorsDate.Keys)
                        {
                            if (DateFile.instance.actorsDate[id].Keys.Contains<int>(1000010))
                            {
                                DateFile.instance.actorsDate[id].Remove(1000010);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                }

            }
        }

        private static void setTimeModyfyGUI()
        {
            Main.settings.lockTime = GUILayout.Toggle(Main.settings.lockTime, "锁定时间", new GUILayoutOption[0]);
        }

        private static void setStudyModifyGUI()
        {
            Main.settings.studyWithNoAgeLimit = GUILayout.Toggle(Main.settings.studyWithNoAgeLimit, "祠堂传功不受年龄限制", new GUILayoutOption[0]);
        }

        private static void setManpowModifyGUI()
        {
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Label("太吾村可调配人力设置:", new GUILayoutOption[0]);
            Main.settings.increseMaxManPowerType = GUILayout.SelectionGrid(Main.settings.increseMaxManPowerType, new string[]
            {
                "原版",
                "固定值",
                "人口数的一半",
            }, 3, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("固定人力上限", GUILayout.Width(90));
            int.TryParse(GUILayout.TextField(Main.settings.maxManPowerNum.ToString(), 10, GUILayout.Width(60)), out Main.settings.maxManPowerNum);
            GUILayout.EndHorizontal();
        }

        private static void setBuildingnumModifyGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("基础建筑数量", GUILayout.Width(90));
            int.TryParse(GUILayout.TextField(Main.settings.baseBuildingNum.ToString(), 10, GUILayout.Width(60)), out Main.settings.baseBuildingNum);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("升级提升的建筑数量", GUILayout.Width(180));
            int.TryParse(GUILayout.TextField(Main.settings.increseBuildingNum.ToString(), 10, GUILayout.Width(60)), out Main.settings.increseBuildingNum);
            GUILayout.EndHorizontal();
        }
        private static void setAllEffectGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("被动绝技正逆练效果都有的人物：");
            settings.allEffectType = GUILayout.SelectionGrid(settings.allEffectType, new string[]
            {
                "无",
                "主角",
                "敌人",
                "全部",
             }, 4, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            int count = settings.gongfaEffectArr.Count();
            if (DateFile.instance.gongFaFPowerDate.Count != 0 && settings.allEffectType != 0)
            {
                GUILayout.Label("以下被动绝技不设置为正逆练效果都有（点击可取消）：", new GUILayoutOption[0]);
                for (int i = 0; i < count; i++)
                {
                    int id = settings.gongfaEffectArr[i];
                    if (i % effectPerRow == 0)
                    {
                        GUILayout.BeginHorizontal();
                    }
                    if (id != -1 && GUILayout.Button(DateFile.instance.gongFaFPowerDate[id][0], GUILayout.Width(200)))
                    {
                        settings.gongfaEffectArr.RemoveAt(i);
                        i--;
                        count--;
                    }
                    if (i % effectPerRow == effectPerRow - 1 || id == -1)
                    {
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.BeginHorizontal();
                gongfaEffectStr = GUILayout.TextField(gongfaEffectStr, GUILayout.Width(200));
                GUILayout.Label("不设置为正逆练效果都有。", GUILayout.Width(200));
                if (GUILayout.Button("确认", GUILayout.Width(50)))
                {
                    foreach (int id in DateFile.instance.gongFaFPowerDate.Keys)
                    {
                        String name;
                        if (DateFile.instance.gongFaFPowerDate[id].TryGetValue(0, out name) &&
                            name == gongfaEffectStr.Trim() && !settings.gongfaEffectArr.Contains(id))
                        {
                            settings.gongfaEffectArr.Insert(0, id);
                            break;
                        }
                    }

                }
                GUILayout.EndHorizontal();
            }
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            //人相关
            setVillageModifyGUI();
            setLunhuiModifyGUI();
            setKeepYongGUI();
            //战斗相关
            setAllEffectGUI();
            //建筑相关
            setTimeModyfyGUI();
            setStudyModifyGUI();
            setManpowModifyGUI();
            setBuildingnumModifyGUI();

        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }


    /// <summary>
    ///  菜单设置人物属性时拦截
    /// </summary>
    [HarmonyPatch(typeof(ActorMenu), "SetActorAttr")]
    public static class ActorMenu_SetActorAttr_Patch
    {

        static void Prefix(ActorMenu __instance, int key)
        {
            Main.nowActorId = key;
        }

    }

    /// <summary>
    ///  更新人物立绘时拦截
    /// </summary>
    [HarmonyPatch(typeof(ActorFace), "UpdateFace")]
    public static class ActorFace_UpdateFace
    {
        static void Prefix(ActorFace __instance, int actorId, ref int age)
        {
            if (Main.settings.allYoung && Main.settings.allAge >= 0 && age >= Main.settings.allAge)
            {
                age = Main.settings.allAge;
            }

            if (Main.settings.villageYoung && Main.settings.villageAge >= 0 && age >= Main.settings.villageAge)
            {
                if (int.Parse(DateFile.instance.GetActorDate(actorId, 19, false)) == 16)
                {
                    age = Main.settings.villageAge;
                }
            }

            Dictionary<int, string> dic = null;
            if (DateFile.instance.actorsDate.TryGetValue(actorId, out dic))
            {
                if (dic.Keys.Contains<int>(1000010) && int.Parse(dic[1000010]) <= age)
                {
                    age = int.Parse(dic[1000010]);
                }
            }
        }

    }

    /// <summary>
    ///  寻找主角被动特效时拦截
    /// </summary>
    [HarmonyPatch(typeof(BattleSystem), "GetActorGongFa")]
    public static class BattleSystem_GetActorGongFa_Patch
    {
        public static FieldInfo actorOtherEffectInfo = typeof(BattleSystem).GetField("actorOtherEffect", BindingFlags.Instance | BindingFlags.NonPublic);
        static void Postfix(BattleSystem __instance)
        {
            if (!Main.enabled)
            {
                return;
            }
            if (Main.settings.allEffectType != 3 && Main.settings.allEffectType != 1)
            {
                return;
            }

            List<int> actorOtherEffectList = (List<int>)actorOtherEffectInfo.GetValue(__instance);
            int num = actorOtherEffectList.Count();
            for (int i = 0; i < num; i++)
            {
                int id = actorOtherEffectList[i];
                if (Main.settings.gongfaEffectArr.Contains(id) || Main.settings.gongfaEffectArr.Contains(id - 5000))
                {
                    continue;
                }
                if (id > 5200 && id < 5500)
                {
                    actorOtherEffectList.Add(id - 5000);
                }

                if (id < 500 && id > 200)
                {
                    actorOtherEffectList.Add(id + 5000);
                }
            }
            actorOtherEffectInfo.SetValue(__instance, actorOtherEffectList);
        }

    }

    /// <summary>
    ///  寻找敌人被动特效时拦截
    /// </summary>
    [HarmonyPatch(typeof(BattleSystem), "GetEnemyGongFa")]
    public static class BattleSystem_GetEnemyGongFa_Patch
    {
        public static FieldInfo enemyOtherEffectInfo = typeof(BattleSystem).GetField("enemyOtherEffect", BindingFlags.Instance | BindingFlags.NonPublic);
        static void Postfix(BattleSystem __instance)
        {
            if (!Main.enabled)
            {
                return;
            }
            if (Main.settings.allEffectType != 3 && Main.settings.allEffectType != 2)
            {
                return;
            }

            List<int> enemyOtherEffectList = (List<int>)enemyOtherEffectInfo.GetValue(__instance);
            int num = enemyOtherEffectList.Count();
            for (int i = 0; i < num; i++)
            {

                int id = enemyOtherEffectList[i];
                foreach (int effectId in Main.settings.gongfaEffectArr)
                {
                    if (id - 5000 == effectId || id == effectId)
                    {
                        continue;
                    }
                }
                if (id > 5200 && id < 5500)
                {
                    enemyOtherEffectList.Add<int>(id - 5000);
                }

                if (id < 500 && id > 200)
                {
                    enemyOtherEffectList.Add<int>(id + 5000);
                }
            }
            enemyOtherEffectInfo.SetValue(__instance, enemyOtherEffectList);
        }
    }

    /// <summary>
    ///  祠堂传功显示时
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "SetStudyWindow")]
    public static class HomeSystem_SetStudyWindow_Patch
    {
        static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled || !Main.settings.studyWithNoAgeLimit)
            {
                return;
            }
            __instance.ageLevelBar.fillAmount = 1.0f;
        }
    }

    /// <summary>
    ///  祠堂传功显示button时
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "UpdateActorStudy")]
    public static class HomeSystem_UpdateActorStudy_Patch
    {
        static void Prefix()
        {
            if (!Main.enabled || !Main.settings.studyWithNoAgeLimit)
            {
                return;
            }
            Main.inActorStudy = true;
        }

        static void Postfix()
        {
            if (!Main.enabled || !Main.settings.studyWithNoAgeLimit)
            {
                return;
            }
            Main.inActorStudy = false;
        }
    }
    /// <summary>
    ///  祠堂传功传送年龄数据时拦截
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "MianAge")]
    public static class DateFile_MianAge_Patch
    {
        static void Postfix(ref int __result)
        {
            if (!Main.enabled || !Main.settings.studyWithNoAgeLimit || !Main.inActorStudy)
            {
                return;
            }
            __result = 110;
        }
    }

    /// <summary>
    ///  增减时间时拦截
    /// </summary>
    [HarmonyPatch(typeof(UIDate), "ChangeTime")]
    public static class UIDate_ChangeTime_Patch
    {

        static void Prefix(ref int time)
        {
            if (!Main.enabled || !Main.settings.lockTime)
            {
                return;
            }
            time = 0;
        }

    }
    /// <summary>
    ///  加为同道时拦截
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "AddActorToFamily")]
    public static class DateFile_AddActorToFamily_Patch
    {
        public static void Postfix(int actorId, bool setGang)
        {
            if (!Main.enabled || !setGang)
            {
                return;
            }
            Main.setVillagePar(actorId);
        }
    }
    /// <summary>
    ///  生成新主角移除旧主角时拦截
    /// </summary>
    [HarmonyPatch(typeof(MissionSystem), "NewActorRemoveMission")]
    public static class MissionSystem_NewActorRemoveMission_Patch
    {
        static void Prefix()
        {
            if (!Main.enabled || !Main.settings.mianActorKeepLunhui)
            {
                return;
            }
            int mianActorId = DateFile.instance.MianActorID();
            int oldMianActorId = DateFile.instance.oldMianActorId;
            List<int> lunhuiList = null;
            DateFile.instance.actorLife[mianActorId].TryGetValue(801, out lunhuiList);
            if (lunhuiList == null)
            {
                DateFile.instance.actorLife[mianActorId].Add(801, new List<int> { oldMianActorId });
            }
            else if (lunhuiList.Count == 0 || !lunhuiList.Contains(oldMianActorId))
            {
                DateFile.instance.actorLife[mianActorId][801].Add(oldMianActorId);
            }
            if (DateFile.instance.deadActors.Contains(oldMianActorId))
            {
                DateFile.instance.deadActors.Remove(oldMianActorId);
            }
        }
    }

    /// <summary>
    /// 系统自动刷人时拦截
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "MakeGangActor")]
    public static class DateFile_MakeGangActor_Patch
    {
        static void Postfix(DateFile __instance, int gangId, int partId, int fatherId, int motherId, int __result)
        {
            if (!Main.enabled)
            {
                return;
            }
            if (gangId == 16)
            {
                Main.setVillagePar(__result);
            }

        }
    }

    /// <summary>
    /// 计算最大人力上限时拦截
    /// </summary>
    [HarmonyPatch(typeof(UIDate), "GetMaxManpower")]
    public static class UIDate_GetMaxManpower_Patch
    {
        static void Postfix(UIDate __instance, ref int __result)
        {
            if (!Main.enabled)
            {
                return;
            }
            if (Main.settings.increseMaxManPowerType == 0)
            {
                string text2 = "";
                text2 += string.Format("{0}{1}{2}{3}\n", new object[]
                {
                    "人力来自于太吾村村民的数量以及太吾村中「居所」的规模和数量。\n",
                    DateFile.instance.SetColoer(20008, "（你最多可以调配", false),
                    DateFile.instance.SetColoer(20003, 50.ToString(), false),
                    DateFile.instance.SetColoer(20008, "个人力。）", false)
                });
                DateFile.instance.resourceDate[7][99] = text2;
                return;
            }

            int num = UIDate.instance.GetBaseMaxManpower();
            foreach (int key in DateFile.instance.baseHomeDate.Keys)
            {
                Dictionary<int, int> dictionary = DateFile.instance.baseHomeDate[key];
                foreach (int key2 in dictionary.Keys)
                {
                    bool flag2 = dictionary[key2] != 0;
                    if (flag2)
                    {
                        Dictionary<int, int[]> dictionary2 = DateFile.instance.homeBuildingsDate[key][key2];
                        foreach (int key3 in dictionary2.Keys)
                        {
                            int[] array = dictionary2[key3];
                            float num2 = float.Parse(DateFile.instance.basehomePlaceDate[array[0]][61]);
                            bool flag3 = num2 > 0f;
                            if (flag3)
                            {
                                num += 1 + Convert.ToInt32(num2 * (float)array[1]);
                            }
                        }
                    }
                }
            }

            if (Main.settings.increseMaxManPowerType == 1 && Main.settings.maxManPowerNum > 0)
            {
                string text2 = "";
                text2 += string.Format("{0}{1}{2}{3}\n", new object[]
                {
                    "人力来自于太吾村村民的数量以及太吾村中「居所」的规模和数量。\n",
                    DateFile.instance.SetColoer(20008, "（你最多可以调配", false),
                    DateFile.instance.SetColoer(20003, Main.settings.maxManPowerNum.ToString(), false),
                    DateFile.instance.SetColoer(20008, "个人力。）", false)
                });
                DateFile.instance.resourceDate[7][99] = text2;
                __result = Mathf.Clamp(num, 0, Main.settings.maxManPowerNum);
            }
            else if (Main.settings.increseMaxManPowerType == 2)
            {
                int num3 = 0;
                List<int> list = new List<int>(DateFile.instance.baseHomeDate.Keys);
                for (int i = 0; i < list.Count; i++)
                {
                    int num4 = list[i];
                    List<int> list2 = new List<int>(DateFile.instance.baseHomeDate[num4].Keys);
                    for (int j = 0; j < list2.Count; j++)
                    {
                        int placeId = list2[j];
                        bool flag4 = int.Parse(DateFile.instance.GetNewMapDate(num4, placeId, 96)) != 0;
                        if (flag4)
                        {
                            int num5 = DateFile.instance.GetPlaceResource(num4, placeId)[8];
                            num3 = num5 / 2;
                        }
                    }
                }
                string text = "";
                text += string.Format("{0}{1}{2}{3}\n", new object[]
                {
                    "人力来自于太吾村村民的数量以及太吾村中「居所」的规模和数量。\n",
                    DateFile.instance.SetColoer(20008, "（你最多可以调配太吾村一半的「人口」作劳力，当前最多可调配", false),
                    DateFile.instance.SetColoer(20003, num3.ToString(), false),
                    DateFile.instance.SetColoer(20008, "个人力。）", false)
                });
                DateFile.instance.resourceDate[7][99] = text;
                __result = Mathf.Clamp(num, 0, num3);
            }

        }

    }

    /// <summary>
    /// 计算最大建筑数量时拦截
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "GetListHomeFavor")]
    public static class HomeSystem_GetListHomeFavor_Patch
    {
        static void Prefix()
        {
            if (!Main.enabled)
            {
                DateFile.instance.homeFavorAdd = 50;
                DateFile.instance.basehomePlaceDate[1001][7] = 15.ToString();
                return;
            }
            DateFile.instance.homeFavorAdd = Main.settings.baseBuildingNum * 5;
            DateFile.instance.basehomePlaceDate[1001][7] = (Main.settings.increseBuildingNum * 5).ToString();
        }
    }
}

