using BepInEx.Configuration;
using SRF;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ConfigurationManager.ConfigurationManager;

namespace ConfigurationManager.UIFactory.CustomBehaviours
{
    public sealed class TextBehaviour : MonoBehaviour
    {
        internal ConfigurationManagerAttributes Attributes;
        internal ConfigEntryBase Entry;
        internal float posy;
        internal TextMeshProUGUI value;

        private void Awake()
        {
            try
            {
                Setup();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex);
            }
        }

        private void Update()
        {
            if (transform.localPosition.y != posy) transform.localPosition = new Vector3(20, posy);
        }

        private void OnGUI()
        {
        }

        internal void Setup()
        {
            gameObject.RemoveComponentIfExists<UIKeybinding>();
            gameObject.RemoveComponentIfExists<UI_GetSettings>();

            if (Entry == null) return;
            if (Entry.Description.Tags.Length > 0)
                foreach (var descriptionTag in Entry.Description.Tags)
                    if (descriptionTag is ConfigurationManagerAttributes managerAttributes)
                        Attributes = managerAttributes;

            var label = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            label.text = Entry.Definition.Key;

            value = transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
            value.text = Entry.BoxedValue.ToString();

            var button = transform.GetChild(1).GetChild(1).GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SystemMessage.AskForTextInput($"Update {Entry.Definition.Key} Config", "Enter the desired text",
                    "OK",
                    t =>
                    {
                        Save(t); 

                        // Not covered by Config Manager's Sentry
                        Attributes?.CallbackAction?.Invoke(Entry.BoxedValue);
                    },
                    null, "Cancel", null, Entry.BoxedValue.ToString());
            });
        }

        private void Save(string t)
        {
            _logger.LogInfo($"{Entry.Definition.Key} started updating");

            Entry.BoxedValue = t;
            value.text = t;
            
            _logger.LogInfo($"{Entry.Definition.Key} has been updated");
        }
    }
}