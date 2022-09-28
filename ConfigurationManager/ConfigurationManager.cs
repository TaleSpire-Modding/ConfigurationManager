// Made by Hollo / HolloFox
// Copyright 2022

using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using ConfigurationManager.Patches.GameSetting;
using LordAshes;
using ModdingTales;
using PluginUtilities;
using Sentry;
using UnityEngine.SceneManagement;

namespace ConfigurationManager
{
    /// <summary>
    /// An easy way to let user configure how a plugin behaves without the need to make your own GUI. The user can change any of the settings you expose, even keyboard shortcuts.
    /// </summary>
    [BepInPlugin(Guid, "Config Manager", Version)]
    [BepInDependency(FileAccessPlugin.Guid)]
    [BepInDependency(ConfigEditorPlugin.Guid)]
    [BepInDependency(SetInjectionFlag.Guid)]
    public sealed class ConfigurationManager : BaseUnityPlugin
    {
        /// <summary>
        /// GUID of this plugin
        /// </summary>
        public const string Guid = "com.hf.hollofox.configurationmanager";

        /// <summary>
        /// Version constant
        /// </summary>
        public const string Version = "0.9.6.0";

        internal static ManualLogSource _logger;
        internal static ConfigurationManager _instance;
        internal static SentryOptions _sentryOptions;

        public enum logToSentry
        {
            Inherited,
            Disabled,
            Prompt,
            Enabled
        }

        private ConfigEntry<ModdingUtils.LogLevel> _logLevel;
        private ConfigEntry<logToSentry> _useSentry;
        
        internal static ModdingUtils.LogLevel LogLevel => _instance._logLevel.Value == ModdingUtils.LogLevel.Inherited ? ModdingUtils.LogLevelConfig.Value : _instance._logLevel.Value;
        internal static logToSentry useSentry => _instance._useSentry.Value;

        private bool _showDebug;

        /// <inheritdoc />
        public ConfigurationManager()
        {
            _instance = this;
            _useSentry = Config.Bind("Filtering", "Send to Dashboard", logToSentry.Enabled);
            _sentryOptions = new SentryOptions()
            {
                // Tells which project in Sentry to send events to:
                Dsn = "",
                Debug = true,
                TracesSampleRate = 1.0,
                IsGlobalModeEnabled = true
            };

            if (useSentry > logToSentry.Disabled)
            {
                using (SentrySdk.Init(_sentryOptions))
                {
                    Setup();
                }
            }
            else
            {
                Setup();
            }
        }

        private void Setup()
        {
            _logLevel = Config.Bind("Filtering", "Show Logs", ModdingUtils.LogLevel.Inherited, new ConfigDescription("", null, new ConfigurationManagerAttributes
            {
                IsAdvanced = true
            }));

            _logger = this.Logger;
            _logger.LogEvent += logFowarding;

            // Do Patching
            var harmony = new HarmonyLib.Harmony(Guid);
            harmony.PatchAll();

            ModdingUtils.Initialize(this, Logger, "HolloFoxes'");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void logFowarding(object o, LogEventArgs e)
        {
            switch (useSentry)
            {
                case logToSentry.Prompt:
                    SystemMessage.AskToConfirmDelete("Do you want to provide logs?", "Do you want to submit", "Opt in","Opt out", false,
                        (t) =>
                        {
                            if (t)
                            {
                                _useSentry.Value = logToSentry.Enabled;
                                relay(o,e);
                            } 
                            else
                                _useSentry.Value = logToSentry.Disabled;
                        }, 
                        null,"Opt out");
                    break;
                case logToSentry.Enabled:
                    relay(o,e);
                    break;
                default:
                    break;
            }
        }

        private void relay(object o, LogEventArgs e)
        {
            switch (e.Level)
            {
                case BepInEx.Logging.LogLevel.None:
                    SentrySdk.CaptureMessage(e.Data.ToString());
                    break;
                case BepInEx.Logging.LogLevel.Fatal:
                    SentrySdk.CaptureMessage(e.Data.ToString(), SentryLevel.Fatal);
                    break;
                case BepInEx.Logging.LogLevel.Error:
                    SentrySdk.CaptureMessage(e.Data.ToString(), SentryLevel.Error);
                    break;
                case BepInEx.Logging.LogLevel.Warning:
                    SentrySdk.CaptureMessage(e.Data.ToString(), SentryLevel.Warning);
                    break;
                case BepInEx.Logging.LogLevel.Message:
                    SentrySdk.CaptureMessage(e.Data.ToString());
                    break;
                case BepInEx.Logging.LogLevel.Info:
                    SentrySdk.CaptureMessage(e.Data.ToString());
                    break;
                case BepInEx.Logging.LogLevel.Debug:
                    SentrySdk.CaptureMessage(e.Data.ToString(), SentryLevel.Debug);
                    break;
                case BepInEx.Logging.LogLevel.All:
                    SentrySdk.CaptureMessage(e.Data.ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameSettingsSetupPatch.AlreadyRan = false;
        }
    }
}
