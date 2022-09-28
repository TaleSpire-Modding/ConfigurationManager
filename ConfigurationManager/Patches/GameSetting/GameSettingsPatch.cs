using System.Linq;
using BepInEx;
using ConfigurationManager.Patches.UI;
using HarmonyLib;
using ModdingTales;
using Sentry;
using static ConfigurationManager.ConfigurationManager;

namespace ConfigurationManager.Patches.GameSetting
{

    [HarmonyPatch(typeof(GameSettings), "SwitchTab")]
    sealed class GameSettingsSwitchPatch
    {
        public static void Postfix(int index)
        {
            if (LogLevel == ModdingUtils.LogLevel.All)
                _logger.LogInfo($"switching to tab: {index}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [HarmonyPatch(typeof(GameSettings), "OnInstanceSetup")]
    sealed class GameSettingsSetupPatch
    {
        internal static bool AlreadyRan;
        private static BaseUnityPlugin[] _plugins;

        public static void Postfix()
        {
            if (useSentry > logToSentry.Disabled)
            {
                using (SentrySdk.Init(_sentryOptions))
                {
                    Do();
                }
            }
            else
            {
                Do();
            }
        }

        private static void Do()
        {
            if (LogLevel == ModdingUtils.LogLevel.All)
                _logger.LogInfo($"GameSettings.OnInstanceSetup: {AlreadyRan}");
            if (AlreadyRan) return;

            // Collect all settings
            SettingSearcher.CollectSettings(out var results, out var modsWithoutSettings, out _plugins);

            // Log detail if logging all
            if (LogLevel == ModdingUtils.LogLevel.All)
            {
                foreach (var entry in results)
                    _logger.LogInfo($"Settings Found: {entry.Definition.Section}, {entry.Definition.Key} ");

                foreach (var plugin in _plugins)
                    foreach (var entry in SettingSearcher.GetPluginConfig(plugin))
                        _logger.LogInfo($"Settings Found in {plugin.Info.Metadata.Name} : {entry.Definition.Section}, {entry.Definition.Key} ");
            }

            // Start by adding new tab
            var go = SingletonBehaviour<GameSettings>.Instance.gameObject;
            var btnGroup = go.GetComponentInChildren<UI_SwitchButtonGroup>();
            var buttons = btnGroup.gameObject.GetComponentsInChildren<UnityEngine.UI.Button>().ToList();
            UI_SwitchButtonGroupStartPatch.Postfix(btnGroup, ref buttons, _plugins);

            // Populate post setup
            if (LogLevel >= ModdingUtils.LogLevel.Medium)
                    _logger.LogInfo("GameSettings SetUp Patch completed");

            AlreadyRan = true;
        }

    }
}
