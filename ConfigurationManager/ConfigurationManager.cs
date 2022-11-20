// Made by Hollo / HolloFox
// Copyright 2022

using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ConfigurationManager.Patches.GameSetting;
using ConfigurationManager.Utilities;
using HarmonyLib;
using LordAshes;
using ModdingTales;
using PluginUtilities;
using Sentry;
using UnityEngine.SceneManagement;

namespace ConfigurationManager
{
    /// <summary>
    ///     An easy way to let user configure how a plugin behaves without the need to make your own GUI. The user can change
    ///     any of the settings you expose, even keyboard shortcuts.
    /// </summary>
    [BepInPlugin(Guid, "Config Manager", Version)]
    [BepInDependency(FileAccessPlugin.Guid)]
    [BepInDependency(ConfigEditorPlugin.Guid)]
    [BepInDependency(SetInjectionFlag.Guid)]
    public sealed class ConfigurationManager : BaseUnityPlugin
    {
        public enum logToSentry
        {
            Inherited,
            Disabled,

            // Prompt,
            Enabled
        }

        /// <summary>
        ///     GUID of this plugin
        /// </summary>
        public const string Guid = "com.hf.hollofox.configurationmanager";

        /// <summary>
        ///     Version constant
        /// </summary>
        public const string Version = "0.10.0.0";

        internal static ManualLogSource _logger;
        internal static ConfigurationManager _instance;
        
        // Sentry Relaged
        internal static SentryOptions _sentryOptions = new SentryOptions
        {
            // Tells which project in Sentry to send events to:
            Dsn = "https://77b85586308d445184b518ccab1542cb@o1208746.ingest.sentry.io/6778961",
            Debug = true,
            TracesSampleRate = 0.2,
            IsGlobalModeEnabled = true,
            AttachStacktrace = true
        };

        internal static Action<Scope> _scope = scope =>
        {
            scope.User = new User
            {
                Username = BackendManager.Username,
            };
            scope.Release = Version;
        };

        // Config
        private static ConfigEntry<ModdingUtils.LogLevel> _logLevel;
        public static ConfigEntry<logToSentry> _useSentry;
        internal static ModdingUtils.LogLevel LogLevel => _logLevel.Value == ModdingUtils.LogLevel.Inherited ? ModdingUtils.LogLevelConfig.Value : _logLevel.Value;
        internal static logToSentry useSentry => _useSentry.Value;

        /// <inheritdoc />
        public ConfigurationManager()
        {
            _instance = this;
            _useSentry = Config.Bind("Filtering", "Send Errors to Dashboard", logToSentry.Disabled);
            _logger = Logger;

            Utils.SentryInvoke(Setup);
        }

        private void Setup()
        {
            _logLevel = Config.Bind("Filtering", "Show Logs", ModdingUtils.LogLevel.Inherited, new ConfigDescription("",
                null, new ConfigurationManagerAttributes
                {
                    IsAdvanced = true
                }));

            _logger.LogEvent += logFowarding;

            // Do Patching
            var harmony = new Harmony(Guid);
            harmony.PatchAll();

            ModdingUtils.Initialize(this, Logger, "HolloFoxes'");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void logFowarding(object o, LogEventArgs e)
        {
            if (useSentry != logToSentry.Enabled) return;
            switch (e.Level)
            {
                case BepInEx.Logging.LogLevel.Fatal:
                    SentrySdk.CaptureMessage(e.Data.ToString(), _scope, SentryLevel.Fatal);
                    break;
                case BepInEx.Logging.LogLevel.Error:
                    SentrySdk.CaptureMessage(e.Data.ToString(), _scope, SentryLevel.Error);
                    break;
            }
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameSettingsSetupPatch.AlreadyRan = false;
        }
    }
}