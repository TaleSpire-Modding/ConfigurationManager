using BepInEx.Configuration;
using SRF;
using UnityEngine;
using UnityEngine.UI;
using static ConfigurationManager.ConfigurationManager;

namespace ConfigurationManager.UIFactory.CustomBehaviours
{
    public sealed class ToggleBehaviour : MonoBehaviour
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
            if (Entry == null) return;
            gameObject.RemoveComponentIfExists<UI_GetSettings>();

            var labelref = gameObject.GetComponent<UI_LabelReference>();
            labelref.SetText(Entry.Definition.Key);

            var toggleref = gameObject.GetComponent<Toggle>();
            toggleref.onValueChanged.RemoveAllListeners();
            toggleref.onValueChanged.AddListener(b =>
            {
                    _logger.LogInfo($"{Entry.Definition.Key} started updating");

                    Entry.BoxedValue = b;
                    Attributes?.CallbackAction?.Invoke(b);

                    _logger.LogInfo($"{Entry.Definition.Key} has been updated");
            });
            toggleref.isOn = (bool)Entry.BoxedValue;

            var toggleGraphic = gameObject.GetComponentInChildren<UI_ToggleSwitchGraphic>();
            toggleGraphic.Toggle((bool)Entry.BoxedValue);

            if (Entry.Description.Tags.Length > 0)
                foreach (var descriptionTag in Entry.Description.Tags)
                    if (descriptionTag is ConfigurationManagerAttributes managerAttributes)
                        Attributes = managerAttributes;
        }
    }
}