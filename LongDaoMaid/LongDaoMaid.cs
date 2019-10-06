using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Harmony12;
using UnityModManagerNet;
using System.Reflection;
using GameData;
using Random = UnityEngine.Random;
//using System.IO;

namespace LongDaoMaid
{

    public class Settings : UnityModManager.ModSettings
    {
        public bool stanceChange = true;
        public bool add1s = true;
        public bool sexOrientation = true;
        public int maxAge = 25;
        public int minAge = 16;
        public bool buyMaid = true;
        public int nvZhuang = 0;
        public string getNewSurname = "";
        public string getNewName = "";
        public bool ssr = true;
        public int ssrBoom = 1;
        public int liChang = 2;
        public bool xingGeChange = true;
        public int xingGe = 0;
        public bool goodFeatureOnly = false;


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static Settings settings;
        public static bool enabled;
        public static int lastNPCid = -1; //最后生成且还未判断的NPC的id，-1表示无
        static bool isInLongDaoEvent = false;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value) return false;
            enabled = value;
            Logger.Log("龙岛女仆已上线");
            return true;
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("Box");

            GUILayout.Label("<color=#FF6EB4>【龙岛女仆】</color>：龙岛只为你提供女性仆从.");
            GUILayout.Label("<color=#FF6EB4>【慧眼识珠】</color>：你总会挑到好看的仆从.");
            GUILayout.Label("<color=#FF6EB4>【品如衣服】</color>：女仆初始拥有一件下品服装.");
            GUILayout.Label("<color=#FF6EB4>【略通六艺】</color>：女仆资质比常人略高一线.");
            //GUILayout.Label("<color=#FF6EB4>【】</color>：你不接受石芯玉女.");//想不到起什么名，不显示了！
            //后续待增加功能：调整立场、再提升资质、加魅力、提高春宵率

            GUILayout.BeginHorizontal("Box");
            settings.add1s = GUILayout.Toggle(settings.add1s, "<color=#FF6EB4>【再续十年】</color> ");
            GUILayout.Label("效果：女仆被龙神赋予额外十年阳寿.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            settings.sexOrientation = GUILayout.Toggle(settings.sexOrientation, "<color=#FF6EB4>【后宫展开】</color> ");
            GUILayout.Label("效果：太吾与女仆同性时，女仆为双性恋，否则为异性恋.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            settings.buyMaid = GUILayout.Toggle(settings.buyMaid, "<color=#FF6EB4>【人口买卖】</color> ");
            GUILayout.Label("效果：获得女仆后，花费9998银钱赎回消耗的地区恩义.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            settings.ssr = GUILayout.Toggle(settings.ssr, "<color=#FF6EB4>【ＳＳＲ！】</color> ");
            int.TryParse(GUILayout.TextField(settings.ssrBoom.ToString(), 3, GUILayout.Width(30)), out settings.ssrBoom);
            GUILayout.Label("%的爆率爆出剑冢相貌的女仆（无法生育）.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("<color=#FF6EB4>　 【挑老拣少】</color>：你与伏龙坛主商议，只挑走", GUILayout.Width(270));
            int.TryParse(GUILayout.TextField(settings.minAge.ToString(), 3, GUILayout.Width(30)), out settings.minAge);
            GUILayout.Label("~", GUILayout.Width(10));
            int.TryParse(GUILayout.TextField(settings.maxAge.ToString(), 3, GUILayout.Width(30)), out settings.maxAge);
            GUILayout.Label("岁的女仆");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("<color=#FF6EB4>　 【再生父母】</color>：你为招募到的女仆　赐姓：", GUILayout.Width(280));
            settings.getNewSurname = GUILayout.TextField(settings.getNewSurname, 5, GUILayout.Width(80));
            GUILayout.Label("　赐名：", GUILayout.Width(60));
            settings.getNewName = GUILayout.TextField(settings.getNewName, 5, GUILayout.Width(80));
            GUILayout.Label("　　　（赐姓赐名皆可选填）");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            settings.stanceChange = GUILayout.Toggle(settings.stanceChange, "<color=#FF6EB4>【朋党之说】</color> ：玩家只招募对应立场的女仆.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            settings.liChang = GUILayout.SelectionGrid(settings.liChang, new string[]
            {
                "<color=#EFF284>刚正</color>",
                "<color=#3987D6>仁善</color>",
                "中庸",
                "<color=#B688DA>叛逆</color>",
                "<color=#FF0000>唯我</color>"
            }, 5, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            settings.xingGeChange = GUILayout.Toggle(settings.xingGeChange, "<color=#FF6EB4>【入职培训】</color> ：玩家将对招募来的女仆进行初级调教.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            settings.xingGe = GUILayout.SelectionGrid(settings.xingGe, new string[]
            {
                "随机性格",
                "<color=#3987D6>知书达理</color>",
                "<color=#FF0000>毒舌傲娇</color>",

            }, 3, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("　 <color=#FF6EB4>【女装山脉】</color> ：你有时想要些不一样的<color=#FF6EB4>女仆</color>.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            settings.nvZhuang = GUILayout.SelectionGrid(settings.nvZhuang, new string[]
            {
                "<color=#FF6EB4>正常女仆</color>",
                "<color=#FF6EB4>女生男相</color>",
                "<color=#3987D6>男生女相</color>",
                "<color=#3987D6>正常男仆</color>",
            }, 4, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box", Array.Empty<GUILayoutOption>());
            Main.settings.goodFeatureOnly = GUILayout.Toggle(Main.settings.goodFeatureOnly, "<color=#FF6EB4>【盡美盡善】</color> ", Array.Empty<GUILayoutOption>());
            GUILayout.Label("效果：女仆只會出現好的特性.", Array.Empty<GUILayoutOption>());
            GUILayout.EndHorizontal();

            GUILayout.Label("<color=#FF0000>注意</color>：请在1.0.4及过去版本使用过赐姓功能的存档中点击1次下面的按钮，修复将来赐姓女仆生子时产生的BUG！");

            //检测存档
            DateFile tbl = DateFile.instance;
            if (tbl == null || !Characters.HasChar(tbl.mianActorId))
            {
                GUILayout.Label("  存档未载入!");
            }
            else if (GUILayout.Button("重置隐藏姓氏"))
            {
                resetTrueSurName();
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// 检测所有非太吾的角色第29项(真实姓氏)，如果是string，改为int的5
        /// </summary>
        public static void resetTrueSurName()
        {
            Logger.Log("开始重置真实姓氏");
            foreach (var charId in Characters.GetAllCharIds())
            {
                if (Characters.HasCharProperty(charId, 29))
                {
                    var val = Characters.GetCharProperty(charId, 29);
                    if (!IsNumber(val))
                    {
                        Logger.Log($"{Characters.GetCharProperty(charId, 5)}{Characters.GetCharProperty(charId, 0)}已重置真实姓氏");
                        Characters.SetCharProperty(charId, 29, "5");
                    }
                }
            }
            Logger.Log("重置完毕！");
        }

        /// <summary>
        /// 判断字符串是否为纯数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumber(string str)
        {
            if (str == null || str.Length == 0)
                return false;
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] bytestr = ascii.GetBytes(str);

            foreach (byte c in bytestr)
            {
                if (c < 48 || c > 57)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取新生NPC的ID
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "MakeNewActor")]
        public static class DateFile_MakeNewActor_Patch
        {
            private static void Postfix(DateFile __instance, int __result)
            {
                if (!Main.enabled)
                {
                    return;
                }
                lastNPCid = __result;
                __instance.ActorFeaturesCacheReset(__result); //刷新特性
                return;
            }
        }

        /// <summary>
        /// 创建NPC之后，显示新相知之前执行的函数，用来修改龙岛忠仆
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "ChangeFavor")]
        public static class DateFile_ChangeFavor_Patch
        {
            private static void Postfix()
            {
                if (!Main.enabled)
                {
                    return;
                }
                if (lastNPCid != -1)
                {
                    if (isLongDaoZhongPu(lastNPCid))
                    {
                        npcChange(lastNPCid);
                    }
                    lastNPCid = -1;
                }
            }
        }

        /// <summary>
        /// 若開啟盡美盡善功能, 於龍島事件中, patch 建立新角色的特性
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "NewActorFeatures")]
        public static class DateFile_NewActorFeatures_Patch
        {
            private static void Prefix(ref bool goodFeature)
            {
                if (Main.enabled && 
                    isInLongDaoEvent &&
                    Main.settings.goodFeatureOnly)
                {
                    goodFeature = true;
                }
            }
        }

        /// <summary>
        /// 攔截恩義事件中的龍島事件, 並以flag供其他功能判斷是否正在龍島事件內
        /// </summary>
        [HarmonyPatch(typeof(MessageEventManager), "EndEvent9010_1")]
        public class MessageEventManager_EndEvent9010_1_Patch
        {
            private static void Prefix(MessageEventManager __instance)
            {
                isInLongDaoEvent = __instance.EventValue[1] == 11;
            }

            private static void Postfix()
            {
                isInLongDaoEvent = false;
            }
        }

        /// <summary>
        /// 判断指定idNPC是否为龙岛忠仆
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool isLongDaoZhongPu(int id)
        {
            bool flag = false;
            List<int> npcFeature = DateFile.instance.GetActorFeature(lastNPCid);
            for (int i = 0; i < npcFeature.Count; i++)
            {
                if (npcFeature[i] == 4005) //4005为龙岛忠仆特性
                {
                    flag = true;
                }
            }
            return flag;
        }

        /// <summary>
        /// 修改指定idNPC数据
        /// </summary>
        /// <param name="id"></param>
        public static void npcChange(int id)
        {
            DateFile df = DateFile.instance;
            var playerId = df.mianActorId;
            // 目前新版本性別/性取向使用模板人物, (為了減少存檔負擔?)
            // 對應的 GetCharProperty 不一定會有值
            // 這邊從善如流, 必須修改的情況才用 SetCharProperty 強制複寫
            var originGenderStr = df.GetActorDate(id, 14, false);

            if (string.IsNullOrEmpty(originGenderStr))
                Main.Logger.Error("居然會產生無性別的人物?? 這不合理阿");
            Characters.SetCharProperty(id, 17,  (settings.nvZhuang == 1 || settings.nvZhuang == 2) ? "1" : "0"); // 女生男相 或 男生女相
            string genderStr = settings.nvZhuang <= 1 ? "2" : "1";
            if (originGenderStr != genderStr)
            {
                Characters.SetCharProperty(id, 14,  genderStr);
                // 換性別 重新取名字
                df.MakeActorName(id, int.Parse(df.GetActorDate(id, 29, false)), "", true);
            }

            //3.改变取向
            if (settings.sexOrientation == true)
            {
                var newSexOrientation = df.GetActorDate(playerId, 14, false) == genderStr ? "1" : "0"; // 同性則雙(1), 異性則直(0)
                if (df.GetActorDate(id, 21, false) != newSexOrientation)
                    Characters.SetCharProperty(id, 21, newSexOrientation);
            }
            //麻蛋！原来只要不是0（异性恋）就是双性恋！我还以为1是同2是双！原来这游戏没有同！！！

            //遍历所有特性，找到并消除特性
            List<int> npcFeature = df.GetActorFeature(id);
            for (int i = 0; i < npcFeature.Count; i++)
            {
                if (npcFeature[i] == 1002 | npcFeature[i] == 1001)//石芯玉女 无根之人
                {
                    df.RemoveActorFeature(id, i);
                }

                if (settings.xingGeChange && settings.xingGe >= 43 && settings.xingGe <= 48)//入职培训
                {
                    df.RemoveActorFeature(id, i);
                }
            }

            //1.调整立场
            if (settings.stanceChange == true)
                Characters.SetCharProperty(id, 16,  Convert.ToString(settings.liChang * 250)); //修改立场

            //2.每个龙岛女仆都被龙神赋予更多的阳寿。因为自动调整过年龄，所以增加10年补贴可能的亏损。
            int countTemp13 = Convert.ToInt32(Characters.GetCharProperty(id, 13));
            int countTemp12 = Convert.ToInt32(Characters.GetCharProperty(id, 12));
            countTemp13 += 10;
            countTemp12 += 10;

            if (settings.add1s == true)
            {
                countTemp13 += 10;
                countTemp12 += 10;
            }
            Characters.SetCharProperty(id, 13,  Convert.ToString(countTemp13));
            Characters.SetCharProperty(id, 12,  Convert.ToString(countTemp12));
            


            //4.年龄区间
            if (settings.maxAge > 0 && settings.minAge > 0)
            {
                Characters.SetCharProperty(id, 11,  Convert.ToString(Random.Range(settings.minAge, settings.maxAge)));
            }

            //5.人口买卖
            if (settings.buyMaid && Convert.ToInt32(Characters.GetCharProperty(playerId, 406)) >= 9998)
            {
                df.baseWorldDate[int.Parse(df.gangDate[14][11])][int.Parse(df.gangDate[14][3])][3] += 300;
                UIDate.instance.ChangeResource(df.mianActorId, 5, -9998, true);
            }

            //6.再生父母
            if (settings.getNewSurname != "")
            {
                Characters.SetCharProperty(id, 5,  settings.getNewSurname);
            }

            if (settings.getNewName != "")
                Characters.SetCharProperty(id, 0,  settings.getNewName);

            //7.正式培训
            if (settings.xingGeChange)
            {
                int tempXingGe = settings.xingGe;
                if (settings.xingGe == 0) { tempXingGe = Random.Range(1, 2); }
                df.AddActorFeature(id, 42 + Random.Range(1 * tempXingGe, 3 * tempXingGe));
            }

            //慧眼识珠
            //0|鼻子|特征|眼睛|眉毛|嘴|胡子|头发
            string rdHair = Convert.ToString(Random.Range(0, 54));
            string rdNose = Convert.ToString(Random.Range(30, 44));
            string rdSign = Convert.ToString(Random.Range(20, 29));
            string rdEyes = Convert.ToString(Random.Range(30, 44));
            string rdBrow = Convert.ToString(Random.Range(30, 44));
            string rdMouse = Convert.ToString(Random.Range(30, 44));

            Characters.SetCharProperty(id, 995,  "0" + "|" + rdNose + "|" + rdSign + "|" + rdEyes + "|" + rdBrow + "|" + rdMouse + "|" + "0" + "|" + rdHair);
            if (Random.Range(1, 4) != 1)
                Characters.SetCharProperty(id, 996, 
                    Convert.ToString(Random.Range(0, 4)) + "|" +
                    Convert.ToString(Random.Range(0, 4)) + "|" +
                    Convert.ToString(Random.Range(0, 4)) + "|" +
                    Convert.ToString(Random.Range(0, 4)) + "|" +
                    Convert.ToString(Random.Range(0, 4)) + "|" +
                    Convert.ToString(Random.Range(0, 4)) + "|" +
                    Convert.ToString(Random.Range(0, 4)) + "|" +
                    Convert.ToString(Random.Range(0, 4)));//75%概率，发肤颜色取前5

            //重新roll魅力
            Characters.SetCharProperty(id, 15,  Convert.ToString(Random.Range(501, 900)));

            if (Random.Range(1, 10) == 1)//再roll，10%概率发肤色与太吾一致，好歹给黑妹留点机会
                Characters.SetCharProperty(id, 996,  Characters.GetCharProperty(playerId, 996));

            //List<int> love = DateFile.instance.GetActorSocial(id, 312, false, false);

            //资质均衡
            Characters.SetCharProperty(id, 551,  "2");
            Characters.SetCharProperty(id, 651,  "2");

            //挑选最出众的资质
            int yiID = 501;
            for (int i = 0; i < 16; i++)
            {
                if (Int32.Parse(Characters.GetCharProperty(id, yiID)) < Int32.Parse(Characters.GetCharProperty(id, 501 + i)))
                {
                    yiID = 501 + i;
                }
            }
            int wuID = 604;
            for (int i = 0; i < 11; i++)
            {
                if (Int32.Parse(Characters.GetCharProperty(id, wuID)) < Int32.Parse(Characters.GetCharProperty(id, 604 + i)))
                {
                    wuID = 604 + i;
                }
            }

            //略微强化资质
            if (int.Parse(Characters.GetCharProperty(id, yiID)) <= 100)
                Characters.SetCharProperty(id, yiID,  Convert.ToString(Convert.ToInt32(Characters.GetCharProperty(id, yiID)) + 20));
            if (int.Parse(Characters.GetCharProperty(id, wuID)) <= 100)
                Characters.SetCharProperty(id, wuID,  Convert.ToString(Convert.ToInt32(Characters.GetCharProperty(id, wuID)) + 20));
            if (int.Parse(Characters.GetCharProperty(id, 601)) <= 100)
                Characters.SetCharProperty(id, 601,  Convert.ToString(Convert.ToInt32(Characters.GetCharProperty(id, 601)) + 15));
            if (int.Parse(Characters.GetCharProperty(id, 602)) <= 100)
                Characters.SetCharProperty(id, 602,  Convert.ToString(Convert.ToInt32(Characters.GetCharProperty(id, 601)) + 15));
            if (int.Parse(Characters.GetCharProperty(id, 603)) <= 100)
                Characters.SetCharProperty(id, 603,  Convert.ToString(Convert.ToInt32(Characters.GetCharProperty(id, 601)) + 15));

            for (int x = 1; x <= 6; x++)
            {
                int rdyiID = Random.Range(501, 516);
                if (int.Parse(Characters.GetCharProperty(id, rdyiID)) <= 100)
                    Characters.SetCharProperty(id, rdyiID,  Convert.ToString(Convert.ToInt32(Characters.GetCharProperty(id, rdyiID)) + 20));
            }

            //SSR
            if (settings.ssr && Random.Range(1, 100) <= settings.ssrBoom)
            {
                Characters.SetCharProperty(id, 995,  Convert.ToString(Random.Range(2001, 2009)));
                if (Characters.GetCharProperty(id, 14) == "2") df.AddActorFeature(id, 1002);
                else df.AddActorFeature(id, 1001);
            }

            //给衣服
            //73601——73703 73801——73809
            int newItem;
            if (Random.Range(1, 2) == 1)
            {
                newItem = 73000 + Random.Range(1, 3) + Random.Range(6, 7) * 100;
            }
            else
            {
                newItem = 73800 + Random.Range(1, 9);
            }
            Characters.SetCharProperty(id, 305,  df.MakeNewItem(newItem).ToString());

            df.ActorFeaturesCacheReset(id); //刷新特性

        }


    }
}