using BepInEx.Configuration;
using ModdingTales;
using SRF;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConfigurationManager.UIFactory.CustomBehaviours
{
    public sealed class TextBehaviour : MonoBehaviour
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
                SystemMessage.AskForTextInput($"Update {Entry.Definition.Key} Config", "Enter the desired text", "OK",
                    (string t) =>
                    {
                        if (ConfigurationManager.LogLevel >= ModdingUtils.LogLevel.High)
                            ConfigurationManager._logger.LogInfo($"{Entry.Definition.Key} started updating");
                        
                        value.text = t;
                        Entry.BoxedValue = t;

                        if (ConfigurationManager.LogLevel >= ModdingUtils.LogLevel.High)
                            ConfigurationManager._logger.LogInfo($"{Entry.Definition.Key} has been updated");
                    },null,"Cancel",null,Entry.BoxedValue.ToString());
            });
        }
    }
}
