using System.Linq;
using BepInEx;
using ConfigurationManager.Patches.UI;
using HarmonyLib;
using static ConfigurationManager.ConfigurationManager;

namespace ConfigurationManager.Patches.GameSetting
{

    /// <summary>
    /// </summary>
    [HarmonyPatch(typeof(GameSettings), "OnInstanceSetup")]
    internal sealed class GameSettingsSetupPatch
    {
        internal static bool AlreadyRan;
        private static BaseUnityPlugin[] _plugins;

        public static void Postfix()
        {
            try
            {
                Setup();
            }
            catch (System.Exception ex) 
            { 
                _logger.LogError(ex); 
            }
        }

        private static void Setup()
        {
            _logger.LogInfo($"GameSettings.OnInstanceSetup: {AlreadyRan}");
            if (AlreadyRan) return;

            // Collect all settings
            SettingSearcher.CollectSettings(out var results, out var modsWithoutSettings, out _plugins);

            // Log detail if logging all
            foreach (var entry in results)
                _logger.LogInfo($"Settings Found: {entry.Definition.Section}, {entry.Definition.Key} ");

            foreach (var plugin in _plugins)
                foreach (var entry in SettingSearcher.GetPluginConfig(plugin))
                    _logger.LogInfo(
                        $"Settings Found in {plugin.Info.Metadata.Name} : {entry.Definition.Section}, {entry.Definition.Key} ");

            // Start by adding new tab
            var go = SingletonBehaviour<GameSettings>.Instance.gameObject;
            var btnGroup = go.GetComponentInChildren<UI_SwitchButtonGroup>();
            var buttons = btnGroup.gameObject.GetComponentsInChildren<UnityEngine.UI.Button>().ToList();
            UI_SwitchButtonGroupStartPatch.Postfix(btnGroup, ref buttons, _plugins);

            // Populate post setup
            _logger.LogInfo("GameSettings SetUp Patch completed");

            AlreadyRan = true;
        }
    }
}