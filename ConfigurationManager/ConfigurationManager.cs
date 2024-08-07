﻿// Made by Hollo / HolloFox
// Copyright 2022

using System;
using BepInEx;
using BepInEx.Logging;
using ConfigurationManager.Patches.GameSetting;
using HarmonyLib;
using ModdingTales;
using PluginUtilities;
using UnityEngine.SceneManagement;

namespace ConfigurationManager
{
    /// <summary>
    ///     An easy way to let user configure how a plugin behaves without the need to make your own GUI. The user can change
    ///     any of the settings you expose, even keyboard shortcuts.
    /// </summary>
    [BepInPlugin(Guid, "Config Manager", Version)]
    [BepInDependency(SetInjectionFlag.Guid)]
    public sealed class ConfigurationManager : BaseUnityPlugin
    {

        /// <summary>
        ///     GUID of this plugin
        /// </summary>
        public const string Guid = "com.hf.hollofox.configurationmanager";

        /// <summary>
        ///     Version constant
        /// </summary>
        public const string Version = "0.17.0.0";

        internal static ManualLogSource _logger;
        internal static ConfigurationManager _instance;

        /// <inheritdoc />
        public ConfigurationManager()
        {
            _instance = this;
            _logger = Logger;

            Setup();
        }

        private void Setup()
        {
            // Patching
            try
            {
                var harmony = new Harmony(Guid);
                harmony.PatchAll();

                ModdingUtils.AddPluginToMenuList(this, "HolloFoxes'");
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameSettingsSetupPatch.AlreadyRan = false;
        }
    }
}