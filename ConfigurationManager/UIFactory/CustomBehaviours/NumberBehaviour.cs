using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using ModdingTales;
using SRF;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConfigurationManager.UIFactory.CustomBehaviours
{
    public sealed class NumberBehaviour : MonoBehaviour
    {
        internal ConfigurationManagerAttributes Attributes;
        internal ConfigEntryBase Entry;
        internal float posy;

        private void Awake()
        {
            Setup();
        }

        private void Update()
        {
            if (transform.localPosition.y != posy)
            {
                transform.localPosition = new Vector3(20, posy);
            }
        }

        internal void Setup()
        {
            gameObject.RemoveComponentIfExists<UIKeybinding>();
            gameObject.RemoveComponentIfExists<UI_GetSettings>();

            if (Entry == null) return;
            if (Entry.Description.Tags.Length > 0)
            {
                foreach (var descriptionTag in Entry.Description.Tags)
                {
                    if (descriptionTag is ConfigurationManagerAttributes managerAttributes)
                        Attributes = managerAttributes;
                }
            }

            var label = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            label.text = Entry.Definition.Key;

            var value = transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
            value.text = Entry.BoxedValue.ToString();

            var button = transform.GetChild(1).GetChild(1).GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SystemMessage.AskForTextInput($"Update {Entry.Definition.Key} Config", "Enter the desired number", "OK",
                    (string t) =>
                    {
                        Dictionary<Type, int> typeDict = new Dictionary<Type, int>
                        {
                            {typeof(sbyte),0},
                            {typeof(byte),1},
                            {typeof(short),2},
                            {typeof(ushort),3},
                            {typeof(int),4},
                            {typeof(uint),5},
                            {typeof(long),6},
                            {typeof(ulong),7},
                            {typeof(float),8},
                            {typeof(double),9},
                            {typeof(decimal),10},
                        };

                        if (ConfigurationManager.LogLevel >= ModdingUtils.LogLevel.High)
                            ConfigurationManager._logger.LogInfo($"{Entry.Definition.Key} started updating");

                        switch (typeDict[Entry.BoxedValue.GetType()])
                        {
                            case 0:
                                if (sbyte.TryParse(t, out var s))
                                    Entry.BoxedValue = s;
                                break;
                            case 1:
                                if (byte.TryParse(t, out var b))
                                    Entry.BoxedValue = b;
                                break;
                            case 2:
                                if (short.TryParse(t, out var sho))
                                    Entry.BoxedValue = sho;
                                break;
                            case 3:
                                if (ushort.TryParse(t, out var usho))
                                    Entry.BoxedValue = usho;
                                break;
                            case 4:
                                if (int.TryParse(t, out var i))
                                    Entry.BoxedValue = i;
                                break;
                            case 5:
                                if (uint.TryParse(t, out var ui))
                                    Entry.BoxedValue = ui;
                                break;
                            case 6:
                                if (long.TryParse(t, out var l))
                                    Entry.BoxedValue = l;
                                break;
                            case 7:
                                if (ulong.TryParse(t, out var ul))
                                    Entry.BoxedValue = ul;
                                break;
                            case 8:
                                if (float.TryParse(t, out var f))
                                    Entry.BoxedValue = f;
                                break;
                            case 9:
                                if (double.TryParse(t, out var dou))
                                    Entry.BoxedValue = dou;
                                break;
                            case 10:
                                if (decimal.TryParse(t, out var dec))
                                    Entry.BoxedValue = dec;
                                break;
                        }
                        value.text = Entry.BoxedValue.ToString();

                        if (ConfigurationManager.LogLevel >= ModdingUtils.LogLevel.High)
                            ConfigurationManager._logger.LogInfo($"{Entry.Definition.Key} has been updated");
                    }, null,"Cancel", null, Entry.BoxedValue.ToString());
            });
        }
    }
}
