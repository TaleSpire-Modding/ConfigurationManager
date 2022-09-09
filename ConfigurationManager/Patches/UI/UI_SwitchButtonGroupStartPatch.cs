using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using ConfigurationManager.Utilities;
using LordAshes;
using SRF;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ConfigurationManager.Patches.UI
{

    public static class UI_SwitchButtonGroupStartPatch
    {
        internal static Button clone;
        internal static GameObject content;
        internal static GameObject original;

        private static GameObject ModNameHeader;
        private static GameObject SectionHeader;

        private static GameObject KeybindTemplate;
        private static Vector3 pos;


        private static void AddButton(UI_SwitchButtonGroup __instance, int i, Button template, string key, ref List<Button> ____buttons, float distance)
        {
            clone = Object.Instantiate(template);
            clone.transform.SetParent(__instance.transform, false);
            if (i >= ____buttons.Count)
                ____buttons.Add(clone);
            else
                ____buttons[i] = clone;
            clone.gameObject.name = key;
            var newPost = new Vector3(clone.transform.position.x + (distance * i), clone.transform.position.y, clone.transform.position.z);
            var rot = clone.transform.rotation;
            clone.transform.SetPositionAndRotation(newPost, rot);
            clone.onClick.RemoveAllListeners();
            clone.onClick.AddListener(click);
            var TextOnHover = clone.gameObject.GetComponent<MouseTextOnHover>();
            TextOnHover.mouseHoverText = "Plugin Configurations";
            clone.GetComponentsInChildren<Image>()[2].sprite = FileAccessPlugin.Image.LoadSprite("Images/Icons/plug.png");

            var ScrollViewContent = template.transform.parent.parent.GetChild(0).GetChild(0).GetChild(0).GetChild(2);
            SectionHeader = ScrollViewContent.GetChild(2).GetChild(0).GetChild(0).gameObject;
            KeybindTemplate = ScrollViewContent.GetChild(2).GetChild(0).GetChild(1).GetChild(0).gameObject;
        }

        private static void click()
        {
            content.transform.SetPositionAndRotation(original.transform.position,original.transform.rotation);
            var t = content.transform.GetComponent<RectTransform>();
            t.sizeDelta = new Vector2(0,-pos.y);

            var setting = clone.transform.parent.parent.parent.gameObject.GetComponent<GameSettings>();
            setting.SwitchTab(3);
        }

        internal static void Postfix(UI_SwitchButtonGroup __instance, ref List<Button> ____buttons, BaseUnityPlugin[] plugins)
        {
            if (__instance.gameObject.name == "ToggleGroup" && __instance.transform.parent.name == "Settings")
            {
                var template = ____buttons[0];
                var t2 = ____buttons[1];
                var ScrollViewContent = template.transform.parent.parent.GetChild(0).GetChild(0).GetChild(0);


                // replace old content
                var old = ScrollViewContent.GetChild(3);
                old.parent = null;
                Object.Destroy(old);

                var distance = t2.transform.localPosition.x - template.transform.localPosition.x;
                AddButton(__instance,3,template,"Mod Config",ref ____buttons,distance);
                AddContent(ScrollViewContent, plugins);

                // Clear added config's orange background
                t2.onClick.Invoke();
                template.onClick.Invoke();
            }
        }



        private static void AddContent(Transform scrollView, BaseUnityPlugin[] plugins)
        {
            Debug.Log($"Configuration Manager:{scrollView.GetChild(0).name}");
            original = scrollView.GetChild(0).gameObject;
            content = Object.Instantiate(original);
            content.transform.parent = scrollView;
            content.name = "MOD OPTIONS";

            var title = content.transform.GetChild(0);
            var sampleText = title.GetChild(0);
            pos = sampleText.position;
            var rot = sampleText.rotation;

            ModNameHeader = title.gameObject;
            
            content.transform.DetachChildren();
            

            foreach (var plugin in plugins)
            {
                var entries = SettingSearcher.GetPluginConfig(plugin);
                var sections = entries.Select(e => e.Definition.Section).Distinct();

                if (!entries.Any()) continue;

                var pluginTitle = Object.Instantiate(ModNameHeader).transform;
                pluginTitle.gameObject.name = plugin.Info.Metadata.Name;


                var text = pluginTitle.GetChild(0);
                text.GetComponent<TextMeshProUGUI>().text = plugin.Info.Metadata.Name;
                pluginTitle.parent = content.transform;
                pluginTitle.transform.DetachChildren();
                text.parent = pluginTitle;
                text.SetPositionAndRotation(pos, rot);
                pos += 53.1f * Vector3.down;

                foreach (var section in sections)
                {
                    var header = Object.Instantiate(SectionHeader);
                    header.transform.SetParent(content.transform);
                    header.transform.localPosition = new Vector3(0, pos.y);
                    header.GetComponent<TextMeshProUGUI>().text = section;
                    pos += 22f * Vector3.down;

                    header.gameObject.name = $"{plugin.Info.Metadata.Name}_{section}";

                    foreach (var entry in entries.Where(e => e.Definition.Section == section))
                    {
                        ConfigurationManager._logger.LogInfo($"{entry.Definition.Section}:{entry.Definition.Key}:{entry.BoxedValue}");

                        if (entry.BoxedValue is KeyboardShortcut)
                        {
                            var e = Object.Instantiate(KeybindTemplate);
                            e.transform.SetParent(content.transform);
                            e.transform.localPosition = new Vector3(20, pos.y);
                            pos += 28f * Vector3.down;

                            e.RemoveComponentIfExists<UIKeybinding>();
                            e.transform.GetChild(0).gameObject.RemoveComponentIfExists<UIListItemClickEvents>();
                            var kbh = e.AddComponent<UIFactory.CustomBehaviours.KeybindBehaviour>();

                            e.transform.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
                            e.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(
                                () =>
                                {
                                    entry.BoxedValue = entry.DefaultValue;
                                    e.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = ((KeyboardShortcut)entry.BoxedValue).ToString();
                                });
                            e.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = ((KeyboardShortcut)entry.BoxedValue).ToString();
                            ConfigurationManager._logger.LogInfo(entry.Definition.Key);
                            e.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().SetText(entry.Definition.Key);
                        } else if (Utils.IsNumber(entry.BoxedValue))
                        {
                            //TODO
                            // pos += 28f * Vector3.down;
                        }
                        else if (entry.BoxedValue is string)
                        {
                            // TODO
                            // pos += 28f * Vector3.down;
                        }
                        else if (entry.BoxedValue.GetType().IsEnum)
                        {
                            // TODO
                            // pos += 28f * Vector3.down;
                        }
                    }
                    pos += 11f * Vector3.down;
                }
                pos += 33.1f * Vector3.down;
            }
        }
    }
}
