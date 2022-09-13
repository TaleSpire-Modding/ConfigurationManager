// Made by Hollo / HolloFox
// Copyright 2022


using System.ComponentModel;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using LordAshes;
using ModdingTales;
using PluginUtilities;

namespace ConfigurationManager
{
    /// <summary>
    /// An easy way to let user configure how a plugin behaves without the need to make your own GUI. The user can change any of the settings you expose, even keyboard shortcuts.
    /// 
    /// </summary>
    [BepInPlugin(GUID, "Config Manager", Version)]
    [BepInDependency(FileAccessPlugin.Guid)]
    [BepInDependency(SetInjectionFlag.Guid)]
    [Browsable(false)]
    public sealed class ConfigurationManager : BaseUnityPlugin
    {
        /// <summary>
        /// GUID of this plugin
        /// </summary>
        public const string GUID = "com.hf.hollofox.configurationmanager";

        /// <summary>
        /// Version constant
        /// </summary>
        public const string Version = "0.9.1.0";

        internal static ManualLogSource _logger;

        internal static ConfigurationManager _instance;

        // private readonly ConfigEntry<bool> _showAdvanced;
        // private readonly ConfigEntry<bool> _showKeybinds;
        // private readonly ConfigEntry<bool> _showSettings;
        // private readonly ConfigEntry<bool> _hideSingleSection;
        // private readonly ConfigEntry<bool> _pluginConfigCollapsedDefault; // Don't need right now
        private readonly ConfigEntry<ModdingUtils.LogLevel> _logLevel;
        // private readonly ConfigEntry<bool> _toolTips;

        internal static ModdingUtils.LogLevel LogLevel => _instance._logLevel.Value == ModdingUtils.LogLevel.Inherited ? ModdingUtils.LogLevelConfig.Value : _instance._logLevel.Value;

        private bool _showDebug;

        /// <inheritdoc />
        public ConfigurationManager()
        {
            _logger = base.Logger;
            _instance = this;
            
            // Do Config
            /*
            _showAdvanced = Config.Bind("Filtering", "Show advanced", false);
            _showKeybinds = Config.Bind("Filtering", "Show keybinds", true);
            _showSettings = Config.Bind("Filtering", "Show settings", true);
            */
            _logLevel = Config.Bind("Filtering", "Show Logs", ModdingUtils.LogLevel.Inherited, new ConfigDescription("",null, new ConfigurationManagerAttributes
            {
                IsAdvanced = true
            }));
            // _hideSingleSection = Config.Bind("General", "Hide single sections", false, new ConfigDescription("Show section title for plugins with only one section"));
            // _pluginConfigCollapsedDefault = Config.Bind("General", "Plugin collapsed default", true, new ConfigDescription("If set to true plugins will be collapsed when opening the configuration manager window"));
            // _toolTips = Config.Bind("General", "Setting Tooltips", true, new ConfigDescription("Hovering over setting label will provide a tooltip if enabled."));

            // Do Patching
            var harmony = new HarmonyLib.Harmony(GUID);
            harmony.PatchAll();

            ModdingUtils.Initialize(this, Logger, "HolloFoxes'");
        }
    }
}
