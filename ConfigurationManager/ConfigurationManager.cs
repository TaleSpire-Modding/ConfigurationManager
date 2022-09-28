// Made by Hollo / HolloFox
// Copyright 2022

using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using ConfigurationManager.Patches.GameSetting;
using ConfigurationManager.Utilities;
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
        public const string Version = "0.9.6.3";

        internal static ManualLogSource _logger;
        internal static ConfigurationManager _instance;
        internal static SentryOptions _sentryOptions;
        internal static Action<Scope> _scope;

        public enum logToSentry
        {
            Inherited,
            Disabled,
            // Prompt,
            Enabled
        }

        private ConfigEntry<ModdingUtils.LogLevel> _logLevel;
        private ConfigEntry<logToSentry> _useSentry;
        
        internal static ModdingUtils.LogLevel LogLevel => _instance._logLevel.Value == ModdingUtils.LogLevel.Inherited ? ModdingUtils.LogLevelConfig.Value : _instance._logLevel.Value;
        internal static logToSentry useSentry => _instance._useSentry.Value;

        /// <inheritdoc />
        public ConfigurationManager()
        {
            _instance = this;
            _useSentry = Config.Bind("Filtering", "Send Errors to Dashboard", logToSentry.Disabled);
            _sentryOptions = new SentryOptions()
            {
                // Tells which project in Sentry to send events to:
                Dsn = "https://77b85586308d445184b518ccab1542cb@o1208746.ingest.sentry.io/6778961",
                Debug = true,
                TracesSampleRate = 0.2,
                IsGlobalModeEnabled = true,
                AttachStacktrace = true,
            };
            _scope = (scope) =>
                {
                    scope.User = new User
                    {
                        Username = BackendManager.Username,
                    };
                };

            Utils.SentryInvoke(Setup);
        }

        private void Setup()
        {
            _logLevel = Config.Bind("Filtering", "Show Logs", ModdingUtils.LogLevel.Inherited, new ConfigDescription("", null, new ConfigurationManagerAttributes
            {
                IsAdvanced = true
            }));

            _logger = Logger;
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
                // case logToSentry.Prompt:
                //     SystemMessage.AskToConfirmDelete("Do you want to provide logs?", "Do you want to submit", "Opt in","Opt out", false,
                //         (t) =>
                //         {
                //             if (t)
                //             {
                //                 _useSentry.Value = logToSentry.Enabled;
                //                 relay(o,e);
                //             } 
                //             else
                //                 _useSentry.Value = logToSentry.Disabled;
                //         }, 
                //         null,"Opt out");
                //     break;
                case logToSentry.Enabled:
                    relay(o,e);
                    break;
            }
        }

        private void relay(object o, LogEventArgs e)
        {
            switch (e.Level)
            {
                case BepInEx.Logging.LogLevel.Fatal:
                    SentrySdk.CaptureMessage(e.Data.ToString(),_scope, SentryLevel.Fatal);
                    break;
                case BepInEx.Logging.LogLevel.Error:
                    SentrySdk.CaptureMessage(e.Data.ToString(), _scope, SentryLevel.Error);
                    break;
                default:
                    break;
            }
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameSettingsSetupPatch.AlreadyRan = false;
        }
    }
}
