using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AllAdvanced
{
 

    public class Settings : UnityModManager.ModSettings
    {
        //战斗
        public bool halfHalpUp = true;
        public bool hasDebuff = true;
        //角色
        public bool lunhuiAdvanced = true;
        public bool npcNumberLimited = true;
        public bool tombNumberLimited = true;
        public bool actorChildrenNumberLimited = false;
        public int npcMaxNumber = 100;
        public int tombMaxNumber = 5;
        //建筑
        [XmlIgnore]
        public int labelSize = 20;
        [XmlIgnore]
        public int buttonSize = 20;
        public KeyCode key = KeyCode.F11;
        public bool buildingAdvanced = true;
        public int buildingAdvancedType = 0;
        public bool crossMonth = true;
        public bool neighborNine = true;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
    }


    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool inUpdatePlaceActor = false;
        public static bool uiIsShow = false;
        public static bool bindingKey = false;
        public static int nowSettingField = 0;


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = Settings.Load<Settings>(modEntry);
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            if (!Main.uiIsShow)
            {
                leftTrunChangeTimesShowUI.Load();
                leftTrunChangeTimesShowUI.key = settings.key;
                Main.uiIsShow = true;
                //Logger.Log("scan测试");
            }
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }

        public static void setBattleAdvanced()
        {
            settings.halfHalpUp = GUILayout.Toggle(settings.halfHalpUp, "提气架势同时上升", new GUILayoutOption[0]);
            settings.hasDebuff = GUILayout.Toggle(settings.hasDebuff, "伤势Debuff", new GUILayoutOption[0]);
        }

        public static void setPeopleAdvanced()
        {
            GUILayout.BeginVertical();
            Main.settings.lunhuiAdvanced = GUILayout.Toggle(Main.settings.lunhuiAdvanced, "新产生的无父无母NPC会有轮回。", new GUILayoutOption[0]);

            Main.settings.npcNumberLimited = GUILayout.Toggle(Main.settings.npcNumberLimited, "超过每个村子的NPC人口上限后NPC不会生育。", new GUILayoutOption[0]);
            Main.settings.actorChildrenNumberLimited = GUILayout.Toggle(Main.settings.actorChildrenNumberLimited, "太吾村人口超过上限时主角不会生育", new GUILayoutOption[0]);
            Main.settings.tombNumberLimited = GUILayout.Toggle(Main.settings.tombNumberLimited, "墓碑只显示最新的若干个", new GUILayoutOption[0]);
            GUILayout.BeginHorizontal();
            GUILayout.Label("NPC人口上限", GUILayout.Width(90));
            int.TryParse(GUILayout.TextField(Main.settings.npcMaxNumber.ToString(), 10, GUILayout.Width(60)), out Main.settings.npcMaxNumber);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("显示墓碑上限", GUILayout.Width(90));
            int.TryParse(GUILayout.TextField(Main.settings.tombMaxNumber.ToString(), 10, GUILayout.Width(60)), out Main.settings.tombMaxNumber);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        public static void setBuildingAdvanced()
        {
            Main.settings.crossMonth = GUILayout.Toggle(Main.settings.crossMonth, "闭关模式", new GUILayoutOption[0]);
            Main.settings.buildingAdvanced = GUILayout.Toggle(Main.settings.buildingAdvanced, "建筑优化", new GUILayoutOption[0]);
            Main.settings.neighborNine = GUILayout.Toggle(Main.settings.neighborNine, "扩大空地和临近建筑的范围",new GUILayoutOption[0]);
            Event e = Event.current;
            if (e.isKey && Input.anyKeyDown)
            {
                if (bindingKey)
                {
                    if ((e.keyCode >= KeyCode.A && e.keyCode <= KeyCode.Z)
                        || (e.keyCode >= KeyCode.F1 && e.keyCode <= KeyCode.F12)
                        || (e.keyCode >= KeyCode.Alpha0 && e.keyCode <= KeyCode.Alpha9)
                        )
                    {
                        settings.key = e.keyCode;
                    }
                    bindingKey = false;
                }
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("设置显示闭关次数快捷键： Ctrl +", GUILayout.Width(210));
            if (GUILayout.Button((bindingKey ? "请按键" : settings.key.ToString()),
                GUILayout.Width(80)))
            {
                bindingKey = !bindingKey;
            }
            GUILayout.Label("（支持0-9,A-Z,F1-F12）");
            GUILayout.EndHorizontal();
        }

        public static void setOtherAdvanced()
        {


        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("当前设置页面：", new GUILayoutOption[0]);
            nowSettingField = GUILayout.SelectionGrid(nowSettingField, new string[]
{
                "角色",
                "建筑",
                "战斗",
                "其他"
            }, 4, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("取消所有修改", GUILayout.Width(120)))
            {
                settings = new Settings();
            }
            switch (nowSettingField)
            {
                case 0:
                    setPeopleAdvanced();
                    break;
                case 1:
                    setBuildingAdvanced();
                    break;
                case 2:
                    setBattleAdvanced();
                    break;
                case 3:
                    setOtherAdvanced();
                    break;
            }

        }
    }


    /// <summary>
    ///  提气和架势更新时拦截
    /// </summary>
    [HarmonyPatch(typeof(BattleSystem), "UpdateBattlerMagicAndStrength")]
    public static class BattleSystem_UpdateBattlerMagicAndStrength_Patch
    {
        private static bool inUpdating = false;
        public static void Prefix(BattleSystem __instance,bool isActor,int power,ref float  ___actorMagic,ref float ___enemyMagic, MethodBase __originalMethod)
        {
            if(!Main.enabled || !Main.settings.halfHalpUp || inUpdating)
            {
                return;
            }
            float oldMagic = isActor ? ___actorMagic : ___enemyMagic;
            const float maxMagic = 15000f;
            if (isActor)
            {
                ___actorMagic = maxMagic;
            }
            else
            {
                ___enemyMagic = maxMagic;
            }
            inUpdating = true;
            __originalMethod.Invoke(__instance, new System.Object[] { isActor, power });
            inUpdating = false;
            if (isActor)
            {
                ___actorMagic = oldMagic;
            }
            else
            {
                ___enemyMagic = oldMagic;
            }
        }
    }

    public static class ActorDebuff
    {
        public enum DebuffType { Breast, Waist, Head, LeftHand, RightHand, LeftLeg, RightLeg, Mind, Posion, Body };
        public enum DebuffSubType { Hp,Sp};
        private static Dictionary<DebuffType, int> debuffRate;
        public static DebuffType getDebuffType(int injuryId) { return (DebuffType)(injuryId / 6); }
        public static DebuffSubType getDebuffSubType(int injuryId) { return (DebuffSubType)((injuryId % 6) / 3); }
        static ActorDebuff()
        {
            debuffRate = new Dictionary<DebuffType, int>()
            {
                {DebuffType.Breast,0 },
                {DebuffType.Waist,0 },
                {DebuffType.Head,0 },
                {DebuffType.LeftHand,60 },
                {DebuffType.RightHand,60 },
                {DebuffType.LeftLeg,60 },
                {DebuffType.RightLeg,60 },
                {DebuffType.Mind,60 },
                {DebuffType.Posion,60 },
                {DebuffType.Body,0 },
            };
        }

        public static int getDebuffLevel(int actorId,int injuryPower,int injuryId)
        {
            DebuffType debuffType = getDebuffType(injuryId);
            DebuffSubType debuffSubType = getDebuffSubType(injuryId);
            int injuryDamage = int.Parse(DateFile.instance.injuryDate[injuryId][(int)debuffSubType+1]);
            int maxCanDamage = debuffSubType == 0 ? ActorMenu.instance.MaxHp(actorId) : ActorMenu.instance.MaxSp(actorId);
            return injuryDamage * debuffRate[debuffType] / maxCanDamage;
        }

        public static int GetActorDebuff(int actorId, int injuryId)
        {
            int injuryPower = 0;
            if (DateFile.instance.ActorIsInBattle(actorId) != 0)
            {
                if (DateFile.instance.battleActorsInjurys[actorId].Keys.Contains<int>(injuryId))
                {
                    injuryPower = DateFile.instance.battleActorsInjurys[actorId][injuryId][0];
                }
            }
            else
            {
                DateFile.instance.actorInjuryDate[actorId].TryGetValue(injuryId, out injuryPower);
            }
            return getDebuffLevel(actorId, injuryPower, injuryId);
        }

        public static int GetActorTotalDebuff(int actorId,DebuffType countDebuffType)
        {
            int totalHpDebuffValue = 0;
            int totalSpDebuffValue = 0;
            if (DateFile.instance.ActorIsInBattle(actorId) != 0)
            {
                List<int> list = new List<int>(DateFile.instance.battleActorsInjurys[actorId].Keys);
                for (int i = 0; i < list.Count; i++)
                {
                    int injuryId = list[i];
                    int debuffValue = ActorDebuff.GetActorDebuff(actorId, injuryId);
                    if (countDebuffType == getDebuffType(injuryId))
                    {
                        if(getDebuffSubType(injuryId) == DebuffSubType.Hp)
                        {
                            totalHpDebuffValue += debuffValue;
                        }
                        else if(getDebuffSubType(injuryId) == DebuffSubType.Sp)
                        {
                            totalSpDebuffValue += debuffValue;
                        }
                    }
                }
            }
            else
            {
                List<int> list2 = new List<int>(DateFile.instance.actorInjuryDate[actorId].Keys);
                for (int j = 0; j < list2.Count; j++)
                {
                    int injuryId = list2[j];
                    int debuffValue = ActorDebuff.GetActorDebuff(actorId, injuryId);
                    if (countDebuffType == getDebuffType(injuryId))
                    {
                        if (getDebuffSubType(injuryId) == DebuffSubType.Hp)
                        {
                            totalHpDebuffValue += debuffValue;
                        }
                        else if (getDebuffSubType(injuryId) == DebuffSubType.Sp)
                        {
                            totalSpDebuffValue += debuffValue;
                        }
                    }
                }
            }

            return Math.Max(totalHpDebuffValue, totalSpDebuffValue);
        }
    }
    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwtich_Patch
    {
        public static String getDebuffStr(int debuffValue, ActorDebuff.DebuffType debuffType)
        {
            String str = "";
            switch (debuffType)
            {
                case ActorDebuff.DebuffType.LeftHand:
                case ActorDebuff.DebuffType.RightHand:
                    str = "攻击时力道、精妙、迅疾- ";
                    break;
                case ActorDebuff.DebuffType.LeftLeg:
                case ActorDebuff.DebuffType.RightLeg:
                    str = "防御时卸力、拆招、闪避- ";
                    break;
                case ActorDebuff.DebuffType.Mind:
                    str = "功法施展速度- ";
                    break;
                case ActorDebuff.DebuffType.Posion:
                    str = "全部毒抗- ";
                    break;
            }
            return (str + debuffValue + " %。\n");
        }
        public static void Postfix(WindowManage __instance, bool on, GameObject tips = null)
        {
            if (!Main.enabled || !Main.settings.hasDebuff)
            {
                return;
            }
            bool flag = false;
            if (tips != null && on)
            {
                string tag = tips.tag;
                if (tag == "ActorInjury")
                {
                    int injuryId = int.Parse(tips.transform.parent.name.Split(new char[]
                        {
                        ','
                        })[1]);
                    int actorId = ActorMenu.instance.acotrId;
                    int debuffValue = ActorDebuff.GetActorDebuff(actorId, injuryId);
                    __instance.informationMassage.text += getDebuffStr(debuffValue, ActorDebuff.getDebuffType(injuryId));

                }
            }

        }
    }

    [HarmonyPatch(typeof(BattleVaule), "GetWeaponHit")]
    public static class BattleValue_GetWeaponHit_Patch
    {
        public static void Postfix(int actorId,ref int __result)
        {
            if (!Main.enabled || !Main.settings.hasDebuff)
            {
                return;
            }

            if (actorId <= 0)
            {
                return;
            }
            int totalDebuff = 0;
            totalDebuff += ActorDebuff.GetActorTotalDebuff(actorId, ActorDebuff.DebuffType.LeftHand);
            totalDebuff += ActorDebuff.GetActorTotalDebuff(actorId, ActorDebuff.DebuffType.RightHand);
            __result = ((100 - totalDebuff * (__result) / 100));
        }
    }

    [HarmonyPatch(typeof(BattleVaule), "SetGongFaValue")]
    public static class BattleValue_SetGongFaValue_Patch
    {
        public static void Postfix(int actorId, ref int __result,int index)
        {
            if (!Main.enabled || !Main.settings.hasDebuff)
            {
                return;
            }

            if(actorId <= 0)
            {
                return;
            }
            if(index != 601 && index != 602 && index != 603 ){
                return;
            }

            int totalDebuff = 0;
            totalDebuff += ActorDebuff.GetActorTotalDebuff(actorId, ActorDebuff.DebuffType.LeftHand);
            totalDebuff += ActorDebuff.GetActorTotalDebuff(actorId, ActorDebuff.DebuffType.RightHand);
            __result = ((100 - totalDebuff * (__result) / 100));
        }
    }

    [HarmonyPatch(typeof(BattleVaule), "GetDeferDefuse")]
    public static class BattleValue_GetDeferDefuse_Patch
    {
        public static void Postfix(int actorId, ref int __result)
        {
            if (!Main.enabled || !Main.settings.hasDebuff)
            {
                return;
            }

            if (actorId <= 0)
            {
                return;
            }

            int totalDebuff = 0;
            totalDebuff += ActorDebuff.GetActorTotalDebuff(actorId, ActorDebuff.DebuffType.LeftLeg);
            totalDebuff += ActorDebuff.GetActorTotalDebuff(actorId, ActorDebuff.DebuffType.RightLeg);
            __result = ((100 - totalDebuff * (__result) / 100));
        }
    }

    [HarmonyPatch(typeof(BattleVaule), "GetGongFaUseingSpeed")]
    public static class BattleValue_GetGongFaUseingSpeed_Patch
    {
        public static void Postfix(int actorId, ref float __result)
        {
            if (!Main.enabled || !Main.settings.hasDebuff)
            {
                return;
            }

            if (actorId <= 0)
            {
                return;
            }

            int totalDebuff = 0;
            totalDebuff += ActorDebuff.GetActorTotalDebuff(actorId, ActorDebuff.DebuffType.Mind);
            __result = ((100 - totalDebuff * (__result) / 100));
        }
    }

    [HarmonyPatch(typeof(ActorMenu), "MaxPosion")]
    public static class ActorMenu_MaxPosion_Patch
    {
        public static void Postfix(int key, ref int __result)
        {
            if (!Main.enabled || !Main.settings.hasDebuff)
            {
                return;
            }
            int actorId = key;
            if (actorId <= 0)
            {
                return;
            }

            int totalDebuff = 0;
            totalDebuff += ActorDebuff.GetActorTotalDebuff(actorId, ActorDebuff.DebuffType.Posion);
            __result = ((100 - totalDebuff * (__result) / 100));
        }
    }

    /// <summary>
    /// 生成龙岛忠仆或找人建筑产生人时拦截
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "MakeNewActor")]
    public static class DateFile_MakeNewActor_Patch
    {
        static void Postfix(DateFile __instance, int __result)
        {

            if (!Main.enabled || !Main.settings.lunhuiAdvanced)
            {
                return;
            }

            if (__instance.deadActors.Count > 0)
            {

                int num32 = __instance.deadActors[UnityEngine.Random.Range(0, __instance.deadActors.Count)];
                List<int> value = new List<int>(__instance.GetLifeDateList(num32, 801, false))
                {
                    num32
                };
                __instance.actorLife[__result].Add(801, value);
                if (__instance.GetActorFavor(false, __instance.MianActorID(), num32, false, false) >= 30000)
                {
                    UIDate.instance.changTrunEvents.Add(new int[]
                    {
                        239,
                        num32,
                        __instance.GetActorAtPlace(__instance.MianActorID())[0]
                    });
                }
                __instance.deadActors.Remove(num32);

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

            if (!Main.enabled || !Main.settings.lunhuiAdvanced)
            {
                return;
            }
            if (__instance.deadActors.Count > 0 && fatherId == 0 && motherId == 0)
            {
                int num32 = __instance.deadActors[UnityEngine.Random.Range(0, __instance.deadActors.Count)];
                List<int> value = new List<int>(__instance.GetLifeDateList(num32, 801, false))
                {
                    num32
                };
                __instance.actorLife[__result].Add(801, value);
                if (__instance.GetActorFavor(false, __instance.MianActorID(), num32, false, false) >= 30000)
                {
                    UIDate.instance.changTrunEvents.Add(new int[]
                    {
                        239,
                        num32,
                        partId
                    });
                }
                __instance.deadActors.Remove(num32);

            }
        }
    }

    /// <summary>
    /// 更新地点人员时拦截拦截
    /// </summary>
    [HarmonyPatch(typeof(WorldMapSystem), "UpdatePlaceActor", typeof(int), typeof(int))]
    public static class WorldMapSystem_UpdatePlaceActor_Patch
    {
        static void Prefix(WorldMapSystem __instance, int partId, int placeId)
        {
            if (!Main.enabled || !Main.settings.tombNumberLimited)
            {
                return;
            }

            Main.inUpdatePlaceActor = true;
        }

        static void Postfix(WorldMapSystem __instance, int partId, int placeId)
        {
            if (!Main.enabled || !Main.settings.tombNumberLimited)
            {
                return;
            }

            Main.inUpdatePlaceActor = false;
        }
    }
    /// <summary>
    /// 产生当地死亡人员序号列表时拦截
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "HaveActor")]
    public static class DateFile_HaveActor_Patch
    {
        static void Postfix(DateFile __instance, int partId, int placeId, bool getNormal, bool getDieActor, bool getEnemy, bool getChild,
            ref List<int> __result)
        {
            if (!Main.enabled || !Main.settings.tombNumberLimited || !Main.inUpdatePlaceActor)
            {
                return;
            }

            if (getDieActor && (!getNormal) && (!getEnemy))
            {
                int count = __result.Count();
                if (Main.settings.tombMaxNumber > 0 && count > Main.settings.tombMaxNumber)
                {
                    __result = __result.Reverse<int>().Take<int>(Main.settings.tombMaxNumber).ToList<int>();

                }
            }
        }
    }

    /// <summary>
    /// NPC生孩子时拦截
    /// </summary>
    [HarmonyPatch(typeof(PeopleLifeAI), "AISetChildren")]

    public static class PeopleLifeAI_AISetChildren_Patch
    {
        [HarmonyPriority(Priority.VeryHigh)]
        static bool Prefix(PeopleLifeAI __instance, int fatherId, int motherId, int setFather, int setMother)
        {

            if (!Main.enabled || !Main.settings.npcNumberLimited)
            {
                return true;
            }
            if ((!Main.settings.actorChildrenNumberLimited) && (fatherId == DateFile.instance.MianActorID() || motherId == DateFile.instance.MianActorID()))
            {
                return true;
            }
            if (fatherId <= 0 || motherId <= 0)
            {
                return true;
            }
            int gangId = int.Parse(DateFile.instance.GetActorDate(fatherId, 19, false));
            if (setMother == 1 && setFather == 0)
            {
                gangId = int.Parse(DateFile.instance.GetActorDate(motherId, 19, false));
            }

            int gangActorNum = 0;
            int key = int.Parse(DateFile.instance.GetGangDate(gangId, 3));
            int key2 = int.Parse(DateFile.instance.GetGangDate(gangId, 4));
            if (DateFile.instance.gangGroupDate.ContainsKey(key) && DateFile.instance.gangGroupDate[key].ContainsKey(key2))
            {
                List<int> list = new List<int>(DateFile.instance.gangGroupDate[key][key2].Keys);
                for (int i = 0; i < list.Count; i++)
                {
                    gangActorNum += DateFile.instance.GetGangActor(gangId, list[i]).Count();

                }
            }

            if (Main.settings.npcMaxNumber > 0 && gangActorNum >= Main.settings.npcMaxNumber)
            {

                return false;

            }
            return true;
        }

    }

    public static class TrunChange
    {
        public static int leftTrunChangeTimes = 0;
        public static bool trunChanging = false;
        public static void setAfterCrossTime(int useTime)
        {
            while (useTime > DateFile.instance.dayTime)
            {
                useTime -= DateFile.instance.dayTime;
                DateFile.instance.dayTime = DateFile.instance.GetMaxDayTime();
                TrunChange.leftTrunChangeTimes++;
            }
            DateFile.instance.dayTime -= useTime;
        }
    }

    public class leftTrunChangeTimesShowUI : MonoBehaviour
    {
        internal static bool Load()
        {
            try
            {
                new GameObject(typeof(leftTrunChangeTimesShowUI).FullName, typeof(leftTrunChangeTimesShowUI));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return false;
            //GUILayout.Button("");
        }

        private static leftTrunChangeTimesShowUI mInstance = null;
        private bool mInit = false;

        private bool mOpened = false;
        public bool Opened { get { return mOpened; } }

        private Rect mWindowRect = new Rect(0, 0, 0, 0);
        float windowWidth = Screen.width * 0.08f;
        private float mLogTimer = 0;
        public static KeyCode key;

        GUIStyle windowStyle;
        GUIStyle labelStyle;
        GUIStyle buttonStyle;
        public static leftTrunChangeTimesShowUI Instance
        {
            get { return mInstance; }
        }

        private void Awake()
        {
            mInstance = this;
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            CalculateWindowPos();
        }

        private void Update()
        {
            if (mOpened)
                mLogTimer += Time.unscaledDeltaTime;

            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
                && Input.GetKeyUp(Main.settings.key) && Main.settings.crossMonth)
            {
                ToggleWindow();
            }
        }

        private void CalculateWindowPos()
        {

            mWindowRect = new Rect(Screen.width * 0.45f, Screen.height * 0.08f, windowWidth, 0);
        }
        public static GUIStyle window = null;
        private static RectOffset RectOffset(int value)
        {
            return new RectOffset(value, value, value, value);
        }

        private static RectOffset RectOffset(int x, int y)
        {
            return new RectOffset(x, x, y, y);
        }
        private void PrepareGUI()
        {
            windowStyle = new GUIStyle
            {
                name = "window",
                padding = new RectOffset(5, 5, 5, 5),
            };

            labelStyle = new GUIStyle
            {
                name = "label",
                alignment = TextAnchor.MiddleCenter,
                fontSize = Main.settings.labelSize,
            };
            labelStyle.normal.textColor = Color.white;
            labelStyle.richText = true;

            buttonStyle = new GUIStyle
            {
                name = "button",
                alignment = TextAnchor.MiddleCenter,
                fontSize = Main.settings.buttonSize,
                border = new RectOffset(5, 5, 5, 5),

            };
            buttonStyle.normal.textColor = Color.white;
        }
        private void OnGUI()
        {
            if (!mInit)
            {
                mInit = true;
                PrepareGUI();
            }

            if (mOpened)
            {
                var backgroundColor = GUI.backgroundColor;
                var color = GUI.color;
                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
                mWindowRect = GUILayout.Window(667, mWindowRect, WindowFunction, "", windowStyle, GUILayout.Height(Screen.height - 200));
                GUI.backgroundColor = backgroundColor;
                GUI.color = color;
            }
        }

        private void WindowFunction(int windowId)
        {
            if (TrunChange.leftTrunChangeTimes == 0)
            {
                GUILayout.Label("未闭关", labelStyle);
            }
            else
            {
                GUILayout.Label(String.Format("已闭关 <color=#FF0000>{0}</color> 个月", TrunChange.leftTrunChangeTimes), labelStyle);
            }
        }
        internal bool GameCursorLocked { get; set; }
        public void ToggleWindow()
        {
            ToggleWindow(!mOpened);
        }

        public void ToggleWindow(bool open)
        {
            mOpened = open;
            if (!mOpened)
            {
                //SaveSettingsAndParams();
            }
            if (open)
            {
                GameCursorLocked = Cursor.lockState == CursorLockMode.Locked || !Cursor.visible;
                if (GameCursorLocked)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
            else
            {
                if (GameCursorLocked)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        private GameObject mCanvas = null;
    }

    /// <summary>
    ///  关闭建筑时拦截
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "CloseBuildingWindow")]
    public static class HomeSystem_CloseBuildingWindow_Patch
    {

        static void Postfix(HomeSystem __instance)
        {
            __instance.StartCoroutine(trunChange());
        }

        static System.Collections.IEnumerator trunChange()
        {
            if (!Main.enabled)
            {
                yield break;
            }
            if (Main.settings.crossMonth)
            {
                int nowTime = DateFile.instance.dayTime;
                while (TrunChange.leftTrunChangeTimes > 0)
                {
                    TrunChange.trunChanging = true;
                    UIDate.instance.ChangeTrun(true);
                    while (TrunChange.trunChanging)
                    {
                        yield return DateFile.instance.waitForFrame;
                    }
                    TrunChange.leftTrunChangeTimes--;
                }
                DateFile.instance.dayTime = nowTime;
            }

        }

    }


    /// <summary>
    ///  时节切换完的一个函数拦截
    /// </summary>
    [HarmonyPatch(typeof(WorldMapSystem), "UpdateMiniMapWindow")]
    public static class WorldMapSystem_UpdateMiniMapWindow_Patch
    {

        static void Postfix(WorldMapSystem __instance)
        {
            if (!Main.enabled || !Main.settings.crossMonth)
            {
                return;
            }
            if (TrunChange.trunChanging == true)
            {
                TrunChange.trunChanging = false;
            }
        }

    }

    /// <summary>
    ///  生成时节事件界面拦截
    /// </summary>
    [HarmonyPatch(typeof(UIDate), "ShowTrunChangeWindow")]
    public static class UIDate_ShowTrunChangeWindow_Patch
    {

        static bool Prefix(UIDate __instance)
        {
            if (!Main.enabled || !Main.settings.crossMonth)
            {
                return true;
            }

            if (TrunChange.leftTrunChangeTimes > 1)
            {
                return false;
            }
            return true;
        }
    }

    /// <summary>
    ///  生成事件时拦截
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "SetEvent")]
    public static class DateFile_SetEvent_Patch
    {

        static bool Prefix(DateFile __instance)
        {
            if (!Main.enabled || !Main.settings.crossMonth)
            {
                return true;
            }
            if (TrunChange.leftTrunChangeTimes > 1)
            {

                return false;
            }
            return true;
        }

    }

    public static class BuildingEffect
    {
        public enum BuildingType { ReduceReadTime,ReduceLevelUpTime,ReduceStudyTime,ReduceRead, ReduceFamilyLevel, ReduceHard };
        public static Dictionary<BuildingType, float> valuePerLevel;
        public static Dictionary<BuildingType, int> attrs;
        public static Dictionary<BuildingType, int> bounds;
        public static bool isGongfa = false;
        public static int gongfaOrSkillId = -1;
        public static int makeType = -1;

        public static int levelUpTime = 20;
        public static int studyTime = 1;
        public static int readTime = 10;

        public static int readReducedTime = 0;
        public static int levelUpReducedTime = 0;
        static BuildingEffect()
        {
            valuePerLevel = new Dictionary<BuildingType, float>
            {
                {BuildingType.ReduceReadTime,0.25f },
                {BuildingType.ReduceStudyTime, 3.0f},
                {BuildingType.ReduceLevelUpTime, 0.5f},
                {BuildingType.ReduceHard,5.0f },
                {BuildingType.ReduceFamilyLevel,10.0f },
            };
            attrs = new Dictionary<BuildingType, int>
            {
                {BuildingType.ReduceReadTime,66 },
                {BuildingType.ReduceStudyTime, 66},
                {BuildingType.ReduceLevelUpTime, 66},
                {BuildingType.ReduceHard,67 },
                {BuildingType.ReduceFamilyLevel,80 },
            };
            bounds = new Dictionary<BuildingType, int>
            {
                {BuildingType.ReduceReadTime,5 },
                {BuildingType.ReduceStudyTime,60},
                {BuildingType.ReduceLevelUpTime, 10},
                {BuildingType.ReduceHard,200 },
                {BuildingType.ReduceFamilyLevel,1000 },
            };
        }
        public static int gongfaOrSkillType()
        {
            int baseTy = 0;
            int addTy = 0;
            if (isGongfa)
            {
                baseTy = 101;
                addTy = int.Parse(DateFile.instance.gongFaDate[gongfaOrSkillId][1]);
            }

            else
            {
                baseTy = 1;
                addTy = int.Parse(DateFile.instance.skillDate[gongfaOrSkillId][3]);
            }
            return baseTy + addTy;
        }

        public static int buildingAttrNeedValue(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.ReduceReadTime:
                case BuildingType.ReduceHard:
                case BuildingType.ReduceLevelUpTime:
                case BuildingType.ReduceStudyTime:
                    return gongfaOrSkillType();
                case BuildingType.ReduceFamilyLevel:
                    return makeType;
                default:
                    return 1;
            }
        }

        public static int getBuildingEffectValue(BuildingType type,int partId,int placeId,int buildingIndex)
        {
            float num = 0f;

            foreach (int key in HomeSystem.instance.GetBuildingNeighbor(partId, placeId, buildingIndex, 1))
            {
                if (DateFile.instance.homeBuildingsDate[partId][placeId].ContainsKey(key))
                {
                    int[] array = DateFile.instance.homeBuildingsDate[partId][placeId][key];
                    int id = array[0];
                    if (id > 0 && int.Parse(DateFile.instance.basehomePlaceDate[id][attrs[type]]) == buildingAttrNeedValue(type))
                    {
                        num += array[1] * valuePerLevel[type];
                    }

                }
            }
            int realNum = (int)(num);
            return Math.Min(realNum, bounds[type]);
;        }
    }

    [HarmonyPatch(typeof(HomeSystem), "UpdateReadBookWindow")]
    public static class HomeSystem_UpdateReadBookWindow_Patch
    {
        public static int readTime = BuildingEffect.readTime;
        static void Prefix(HomeSystem __instance)
        {
            UIDate.instance.ChangeTime(false,-readTime);
        }
        static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled)
            {
                return;
            }
            UIDate.instance.ChangeTime(false, readTime);
            if (Main.settings.crossMonth && !Main.settings.buildingAdvanced)
            {
                __instance.actorIntText.text = DateFile.instance.SetColoer(20005, DateFile.instance.dayTime + " / " + readTime, false);
            }

            if(__instance.readBookId <= 0)
            {
                return;
            }
            if (Main.settings.buildingAdvanced)
            {
                BuildingEffect.isGongfa = (__instance.studySkillTyp == 17);
                BuildingEffect.gongfaOrSkillId = int.Parse(DateFile.instance.GetItemDate(__instance.readBookId, 32, true));
                BuildingEffect.readReducedTime = BuildingEffect.getBuildingEffectValue(BuildingEffect.BuildingType.ReduceReadTime,
                    __instance.homeMapPartId, __instance.homeMapPlaceId, __instance.homeMapbuildingIndex);
                int time = readTime - BuildingEffect.readReducedTime;
                if(time >= DateFile.instance.dayTime || Main.settings.crossMonth)
                {
                    __instance.actorIntText.text = DateFile.instance.SetColoer(20005, DateFile.instance.dayTime + " / " + time, false);
                }
                else
                {
                    __instance.actorIntText.text = DateFile.instance.SetColoer(20010, DateFile.instance.dayTime + " / " + time, false);
                    __instance.startReadBookButton.interactable = false;
                }
            }

        }

    }

    /// <summary>
    ///  消耗研读时间时拦截
    /// </summary>
    [HarmonyPatch(typeof(ReadBook), "SetReadBookWindow")]
    public static class ReadBook_SetReadBookWindow_Patch
    {

        static void Prefix(ReadBook __instance)
        {
            int time = BuildingEffect.readTime;
            if (!Main.enabled)
            {
                return;
            }
            if (Main.settings.buildingAdvanced)
            {
                UIDate.instance.ChangeTime(false, -BuildingEffect.readReducedTime);
                BuildingEffect.readReducedTime = 0;
            }

            if (Main.settings.crossMonth && DateFile.instance.dayTime < time)
            {
               TrunChange.setAfterCrossTime(time);
               UIDate.instance.ChangeTime(false, -time);
            }

        }
    }

    /// <summary>
    ///  更新突破界面时拦截
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "UpdateLevelUPSkillWindow")]
    public static class HomeSystem_UpdateLevelUPSkillWindow_Patch
    {

        public static int levelUpTime = BuildingEffect.levelUpTime;
        static void Prefix(HomeSystem __instance)
        {
            UIDate.instance.ChangeTime(false, -levelUpTime);
        }
        static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled)
            {
                return;
            }
            UIDate.instance.ChangeTime(false, levelUpTime);
            if (Main.settings.crossMonth && !Main.settings.buildingAdvanced)
            {
                __instance.levelUPActorLevelText.text = DateFile.instance.SetColoer(20005, DateFile.instance.dayTime + " / " + levelUpTime, false);
            }

            if (__instance.levelUPSkillId <= 0)
            {
                return;
            }
            if (Main.settings.buildingAdvanced)
            {
                BuildingEffect.isGongfa = (__instance.studySkillTyp == 17);
                BuildingEffect.gongfaOrSkillId = __instance.levelUPSkillId;
                BuildingEffect.levelUpReducedTime = BuildingEffect.getBuildingEffectValue(BuildingEffect.BuildingType.ReduceLevelUpTime,
                    __instance.homeMapPartId, __instance.homeMapPlaceId, __instance.homeMapbuildingIndex);
                int time = levelUpTime - BuildingEffect.levelUpReducedTime;
                if (time >= DateFile.instance.dayTime || Main.settings.crossMonth)
                {
                    __instance.levelUPActorLevelText.text = DateFile.instance.SetColoer(20005, DateFile.instance.dayTime + " / " + time, false);
                }
                else
                {
                    __instance.levelUPActorLevelText.text = DateFile.instance.SetColoer(20010, DateFile.instance.dayTime + " / " + time, false);
                    __instance.StartLevelUPButton.interactable = false;
                }
            }

        }

    }

    /// <summary>
    ///  消耗突破时间时拦截
    /// </summary>
    [HarmonyPatch(typeof(StudyWindow), "StartStudy")]
    public static class StudyWindow_StartStduy_Patch
    {

        static void Prefix(ReadBook __instance)
        {
            int time = BuildingEffect.levelUpTime;
            if (!Main.enabled)
            {
                return;
            }
            if (Main.settings.buildingAdvanced)
            {
                UIDate.instance.ChangeTime(false, -BuildingEffect.levelUpReducedTime);
                BuildingEffect.levelUpReducedTime = 0;
            }

            if (Main.settings.crossMonth && DateFile.instance.dayTime < time)
            {
                TrunChange.setAfterCrossTime(time);
                UIDate.instance.ChangeTime(false, -time);
            }
        }
    }

    /// <summary>
    ///  更新修习界面时拦截
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "UpdateStudySkillWindow")]
    public static class HomeSystem__UpdateStudySkillWindow_Patch
    {
        static void Postfix(HomeSystem __instance)
        {
            if (Main.enabled && Main.settings.crossMonth)
            {
                int time = 1;
                __instance.actorLevelText.text = DateFile.instance.SetColoer(20005, DateFile.instance.dayTime + " / " + time, false);
            }
        }
    }

    /// <summary>
    ///  计算修习时间时拦截
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "StudySkillUp")]
    public static class HomeSystem__StyudySkillUp_Patch
    {
        public static int studyTime = BuildingEffect.readTime;
        static void Postfix(HomeSystem __instance, int ___studySkillId)
        {
            if (!Main.enabled)
            {
                return;
            }
            if (Main.settings.crossMonth)
            {
                __instance.levelUPActorLevelText.text = DateFile.instance.SetColoer(20005, DateFile.instance.dayTime + " / " + studyTime, false);
            }

            if (___studySkillId <= 0)
            {
                return;
            }
            int time = studyTime;
            if (Main.settings.buildingAdvanced)
            {
                BuildingEffect.isGongfa = (__instance.studySkillTyp == 17);
                BuildingEffect.gongfaOrSkillId = ___studySkillId;
                int studyNoTimeProp = BuildingEffect.getBuildingEffectValue(BuildingEffect.BuildingType.ReduceStudyTime,
                    __instance.homeMapPartId, __instance.homeMapPlaceId, __instance.homeMapbuildingIndex);
                if (UnityEngine.Random.Range(0, 100) < studyNoTimeProp)
                {
                    UIDate.instance.ChangeTime(false, -time);
                    time = 0;
                }

            }
            if (Main.settings.crossMonth && DateFile.instance.dayTime < time)
            {
                TrunChange.setAfterCrossTime(time);
                UIDate.instance.ChangeTime(false, -time);
            }
        }

    }

    /// <summary>
    ///  制造计算所需造诣时拦截
    /// </summary>
    [HarmonyPatch(typeof(MakeSystem), "GetItemNeedSkillValue")]
    public static class MakeSystem_GetItemNeedSkillValue_Patch
    {
        static bool Prefix(MakeSystem __instance, int itemId, int makeTyp, int partId, int placeId, int buildingIndex, bool getDownValue, ref int __result)
        {
            if (!Main.enabled || !Main.settings.buildingAdvanced)
            {
                return true;
            }
            if (itemId <= 0)
            {
                __result = 0;
            }

            else
            {
                BuildingEffect.makeType = makeTyp;
                int num = BuildingEffect.getBuildingEffectValue(BuildingEffect.BuildingType.ReduceFamilyLevel,
                    partId, placeId, buildingIndex);
                if (getDownValue)
                {
                    __result = num;
                }
                else
                {
                    __result = Mathf.Max(int.Parse(DateFile.instance.GetItemDate(itemId, 43, true)) - num, 0);
                }
            }
            return false;
        }
    }

    /// <summary>
    ///  计算研读难度时拦截
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "GetNeedInt")]
    public static class HomeSystem_GetNeedInt_Patch
    {
        static void Postfix(HomeSystem __instance, int actorValue, int readSkillId, ref int __result)
        {
            if (!Main.enabled || !Main.settings.buildingAdvanced)
            {
                return;
            }

            int reducedHard = BuildingEffect.getBuildingEffectValue(BuildingEffect.BuildingType.ReduceHard,
                    __instance.homeMapPartId, __instance.homeMapPlaceId, __instance.homeMapbuildingIndex);

            __result -= reducedHard;
            if (DateFile.instance.readPower)
            {
                __result = 0;
            }
            else
            {
                __result = Mathf.Max(50, __result);
            }
        }
    }

    /// <summary>
    ///  获取临近的建筑时拦截
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "GetBuildingNeighbor")]
    public static class HomeSystem_GetBuildingNeighbor_Patch
    {
        static void checkIndex(ref int[] array,int buildingIndex,int colNum,int width)
        {
            int nowCol = buildingIndex % colNum;
            int leftCol = Math.Max(nowCol - width, 0);
            int rightCol = Math.Min(nowCol + width, colNum - 1);
            for(int i = 0;i < array.Length; i++)
            {
                if(array[i] >=0 && (array[i] < leftCol || array[i] > rightCol))
                {
                    array[i] = -1;
                }
            }
        }
        static void Postfix(HomeSystem __instance, int partId, int placeId, int buildingIndex,int size, ref int[] __result)
        {
            if (!Main.enabled || !Main.settings.neighborNine)
            {
                return;
            }
            int[] array;
            int num = int.Parse(DateFile.instance.GetNewMapDate(partId, placeId, 32));
            if (size > 1)
            {
                array = new int[]
                {
                buildingIndex - num * 2 - 2,
                buildingIndex - num * 2 - 1,
                buildingIndex - num * 2 + 1,
                buildingIndex - num * 2 + 2,
                buildingIndex - num - 2,
                buildingIndex - num + 2,
                buildingIndex + num - 2,
                buildingIndex + num + 2,
                buildingIndex + num * 2 - 2,
                buildingIndex + num * 2 - 1,
                buildingIndex + num * 2 + 1,
                buildingIndex + num * 2 + 2,
                };
                checkIndex(ref array, buildingIndex, num, 2);
            }
            else
            {
                array = new int[]
                {
                    buildingIndex - num - 1,
                    buildingIndex - num + 1,
                    buildingIndex + num + 1,
                    buildingIndex + num - 1,
                };
                checkIndex(ref array, buildingIndex, num, 1);
            }
            foreach (int index in array)
            {
                if (!__result.Contains(index))
                {
                    __result.Add(index);
                }
            }
        }
    }
}

