﻿using System;
using BepInEx;
using BepInEx.Configuration;
using ConfigurationManager.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ModdingTales;

namespace ConfigurationManager
{
    internal static class SettingSearcher
    {
        public static void CollectSettings(out IEnumerable<ConfigEntryBase> results, out List<string> modsWithoutSettings, out BaseUnityPlugin[] plugins)
        {
            modsWithoutSettings = new List<string>();

            List<ConfigEntryBase> entries = new List<ConfigEntryBase>();

            try
            {
                entries.AddRange(GetBepInExCoreConfig());
            }
            catch (Exception ex)
            {
                results = Enumerable.Empty<ConfigEntryBase>();
                if (ConfigurationManager.LogLevel != ModdingUtils.LogLevel.None)
                    ConfigurationManager._logger.LogError(ex);
            }

            plugins = Utils.FindPlugins();
            
            results = entries.AsEnumerable();
        }

        /// <summary>
        /// Bepinex 5 config
        /// </summary>
        private static IEnumerable<ConfigEntryBase> GetBepInExCoreConfig()
        {
            var coreConfigProp = typeof(ConfigFile).GetProperty("CoreConfig", BindingFlags.Static | BindingFlags.NonPublic);
            if (coreConfigProp == null) throw new ArgumentNullException(nameof(coreConfigProp));

            var coreConfig = (ConfigFile)coreConfigProp.GetValue(null, null);

            switch (ConfigurationManager.LogLevel)
            {
                case ModdingUtils.LogLevel.All:
                {
                    var bepinMeta = new BepInPlugin("BepInEx", "BepInEx", typeof(BepInEx.Bootstrap.Chainloader).Assembly.GetName().Version.ToString());
                    ConfigurationManager._logger.LogInfo(bepinMeta);
                    break;
                }
            }
            return coreConfig.GetConfigEntries();
        }

        /// <summary>
        /// Used by bepinex 5 plugins
        /// </summary>
        internal static IEnumerable<ConfigEntryBase> GetPluginConfig(BaseUnityPlugin plugin)
        {

            if (ConfigurationManager.LogLevel != ModdingUtils.LogLevel.None)
                ConfigurationManager._logger.LogInfo(plugin);
            return plugin.Config.GetConfigEntries();
        }
    }
}