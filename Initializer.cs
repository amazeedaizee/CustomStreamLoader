using BepInEx;
using HarmonyLib;
using ngov3;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomStreamLoader
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Windose.exe")]
    public class Initializer : BaseUnityPlugin
    {
        public const string pluginGuid = "needy.girl.customstream";
        public const string pluginName = "Custom Stream Loader";
        public const string pluginVersion = "1.0.0.1";

        public static PluginInfo PInfo { get; private set; }

        public void Awake()
        {
            PInfo = Info;
            Logger.LogInfo("A plugin to load in custom streams. Press the Home key on the Login/Caution screen to begin.");
            Harmony harmony = new Harmony(pluginGuid);
            var originalSetScenario = AccessTools.FirstMethod(typeof(Live), m => m.Name == "SetScenario");
            var patchSetScenario = AccessTools.Method(typeof(EventPatcher), nameof(EventPatcher.AwaitCustomStream));
            enabled = true;
            harmony.Patch(originalSetScenario, new HarmonyMethod(patchSetScenario));
            harmony.PatchAll();
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Home) && SceneManager.GetActiveScene().name == "BiosToLoad")
            {
                Logger.LogInfo("Loading stream...");
                SingletonMonoBehaviour<Settings>.Instance.saveNumber = 5;
                StreamLoader.GetCustomStream();
                SceneManager.LoadScene("WindowUITestScene");
            }
        }


        public void OnApplicationQuit()
        {
            MediaExporter.DeleteAddressBundlesFromPath();
        }
    }
}
