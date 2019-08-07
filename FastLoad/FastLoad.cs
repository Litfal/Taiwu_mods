using ArchiveSystem.GameData;
using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace FastLoad
{
    public static class Main
    {
        public static bool Enabled { get; private set; }
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static Settings settings;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            
            return true;
        }


        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical();
            settings.enableFastLoad = GUILayout.Toggle(settings.enableFastLoad, "快速讀取", new GUILayoutOption[0]);
            GUILayout.Label("效果：讀取進入遊戲時，使用快取方式跳過重複讀取的主存檔.");
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            settings.enableSkipMenu = GUILayout.Toggle(settings.enableSkipMenu, "快速選擇人物", new GUILayoutOption[0]);
            GUILayout.Label("效果：進入選擇人物清單時，使用相同的資料，加快啟動遊戲速度。");
            GUILayout.Label("畫面會顯示 <color=#FF6EB4>三個一樣的存檔</color> 但依然能正常的讀取遊戲.");
            GUILayout.Label("如果你有多個存檔，且檔案都不小(玩了一陣子)，這個功能可以讓你有感的加快啟動遊戲.");
            GUILayout.Label("<color=#FF3030>!注意!</color>：建立新角色，請先關閉此功能後，重新啟動遊戲.");
            GUILayout.EndHorizontal();
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }


    class StateHelper
    {
        public static Guid LoadingState { get; internal set; } = Guid.Empty;
        public static bool IsLoadingIntoGame { get { return LoadingState != Guid.Empty; } }

#if DEBUG
        public static System.Diagnostics.Stopwatch Stopwatch = new System.Diagnostics.Stopwatch();
#endif 
    }

    // 攔截讀取並進入遊戲的行為, 用以控制狀態
    [HarmonyPatch(typeof(MainMenu), "SetLoadIndex")]
    class MainMenu_SetLoadIndex_Patch
    {
        private static void Prefix(int index)
        {
            StateHelper.LoadingState = Guid.NewGuid();
#if DEBUG
            StateHelper.Stopwatch.Restart();
#endif
        }
}

    // 攔截讀取並進入遊戲結束的行為, 用以控制狀態
    [HarmonyPatch(typeof(Loading), "LoadEnd")]
    public class Loading_LoadEnd_Patch
    {
        private static void Postfix()
        {
            StateHelper.LoadingState = Guid.Empty;
#if DEBUG
            Main.Logger.Log($"讀取遊戲花費了 {StateHelper.Stopwatch.ElapsedMilliseconds} ms");
#endif
        }
    }

    // 攔截讀檔時(後)
    [HarmonyPatch(typeof(DefaultData), "Load")]
    public class DefaultData_Load_Patch
    {
        static Guid _lastCalledLoadingState = Guid.Empty;
        static object _lastCalledLoadingStateData = null;

        private static bool Prefix(ref object __result)
        {
            if (!Main.Enabled) return true;
            if (!StateHelper.IsLoadingIntoGame && Main.settings.enableSkipMenu && 
                _lastCalledLoadingStateData != null)
            {
                __result = _lastCalledLoadingStateData;
                return false;
            }
            if (StateHelper.IsLoadingIntoGame && Main.settings.enableFastLoad)
            {
                if (_lastCalledLoadingState == StateHelper.LoadingState &&
                    _lastCalledLoadingStateData != null)
                {
#if DEBUG
                    Main.Logger.Log($"Use the same loaded state cache.");
#endif
                    __result = _lastCalledLoadingStateData;
                    return false;
                }
            }

            return true;
        }

        private static void Postfix(object __result)
        {
            if (!Main.Enabled) return;
            _lastCalledLoadingStateData = __result;
            if (StateHelper.IsLoadingIntoGame && Main.settings.enableFastLoad)
            {
                _lastCalledLoadingState = StateHelper.LoadingState;
            }
        }
    }

}
