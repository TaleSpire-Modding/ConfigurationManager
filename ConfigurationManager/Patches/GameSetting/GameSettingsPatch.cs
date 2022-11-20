using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BepInEx;
using ConfigurationManager.Patches.UI;
using ConfigurationManager.Utilities;
using HarmonyLib;
using ModdingTales;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static ConfigurationManager.ConfigurationManager;

namespace ConfigurationManager.Patches.GameSetting
{

    [HarmonyPatch(typeof(LocalClient), "SetLocalClientMode")]
    internal sealed class temp
    {
        public static void Postfix(ref ClientMode mode)
        {
            var enabled = mode == ClientMode.GameMaster;
            for (var index = 0; index < trackedTexts.Count; index++)
            {
                var asset = trackedTexts[index];
                TextMeshPro creatureStateText = asset.GetComponent<TextMeshPro>();
                creatureStateText.enabled = enabled;
            }
        }


        // Managing in the list will remove GameObject.Find(asset.CreatureId + ".GMInfoBlock"); which is also performant heavy
        internal static List<GameObject> trackedTexts;

        void Update()
        {
            // ... prior logic
            if (LocalClient.IsInGmMode)
            {
                var creaturePos = new NativeArray<Vector3>(trackedTexts.Count, Allocator.Persistent);
                var blockRot = new NativeArray<Quaternion>(trackedTexts.Count, Allocator.Persistent);


                for (var i = 0; i < creaturePos.Length; ++i)
                    creaturePos[i] = trackedTexts[i].transform.position;
                

                var job = new TextRotationJob
                {
                    CreaturePositions = creaturePos,
                    BlockRotation = blockRot,
                    CameraPosition = Camera.main.transform.position
                };

                JobHandle handle = job.Schedule(creaturePos.Length, 1);
                handle.Complete();
                for (int i = 0; i < blockRot.Length; i++)
                {
                    trackedTexts[i].transform.rotation = blockRot[i];
                }

                creaturePos.Dispose();
                blockRot.Dispose();
            }
            
            // ... post logic
        }


        [BurstCompile]
        struct TextRotationJob: IJobParallelFor
        {
            public NativeArray<Vector3> CreaturePositions;
            public NativeArray<Quaternion> BlockRotation;
            public Vector3 CameraPosition;

            public void Execute(int i)
            {
                BlockRotation[i] = Quaternion.LookRotation(CreaturePositions[i] - CameraPosition);
            }
        }

    }



    



    [HarmonyPatch(typeof(GameSettings), "SwitchTab")]
    internal sealed class GameSettingsSwitchPatch
    {
        public static void Postfix(int index)
        {
            if (LogLevel == ModdingUtils.LogLevel.All)
                _logger.LogInfo($"switching to tab: {index}");
        }
    }

    /// <summary>
    /// </summary>
    [HarmonyPatch(typeof(GameSettings), "OnInstanceSetup")]
    internal sealed class GameSettingsSetupPatch
    {
        internal static bool AlreadyRan;
        private static BaseUnityPlugin[] _plugins;

        public static void Postfix()
        {
            Utils.SentryInvoke(Setup);
        }

        private static void Setup()
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
                    _logger.LogInfo(
                        $"Settings Found in {plugin.Info.Metadata.Name} : {entry.Definition.Section}, {entry.Definition.Key} ");
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