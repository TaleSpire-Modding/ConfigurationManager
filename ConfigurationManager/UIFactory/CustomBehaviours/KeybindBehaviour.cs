using BepInEx.Configuration;
using SRF;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ConfigurationManager.ConfigurationManager;

namespace ConfigurationManager.UIFactory.CustomBehaviours
{
    public sealed class KeybindBehaviour : MonoBehaviour
    {
        internal ConfigurationManagerAttributes Attributes;
        internal ConfigEntryBase Entry;

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

        internal void Setup()
        {
            gameObject.RemoveComponentIfExists<UIKeybinding>();
            gameObject.transform.GetChild(0).gameObject.RemoveComponentIfExists<UIListItemClickEvents>();

            if (Entry == null) return;

            if (Entry.Description.Tags.Length > 0)
                foreach (var descriptionTag in Entry.Description.Tags)
                    if (descriptionTag is ConfigurationManagerAttributes managerAttributes)
                        Attributes = managerAttributes;

            gameObject.transform.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
            gameObject.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(RevertToDefault);
            gameObject.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                Entry.BoxedValue.ToString();
            gameObject.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().SetText(Entry.Definition.Key);
        }

        private void RevertToDefault()
        {
            Entry.BoxedValue = Entry.DefaultValue;
            gameObject.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                Entry.BoxedValue.ToString();
        }

        private void Update()
        {
            if (gameObject.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text !=
                Entry.Definition.Key)
                Setup();
        }
    }
}