using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using ModdingTales;
using SRF;
using TMPro;
using UnityEngine;

namespace ConfigurationManager.UIFactory.CustomBehaviours
{
    public class DropDownBehaviour : MonoBehaviour
    {
        internal ConfigurationManagerAttributes Attributes;
        internal ConfigEntryBase Entry;

        private void Awake()
        {
            Setup();
        }

        internal void Setup()
        {
            if (Entry == null) return;
            // gameObject.RemoveComponentIfExists<UI_LabelReference>();
            gameObject.RemoveComponentIfExists<UI_GetSettings>();

            var labelref = gameObject.GetComponent<UI_LabelReference>();
            labelref.SetText(Entry.Definition.Key);

            var tmp_dropdown = gameObject.GetComponent<TMP_Dropdown>();
            tmp_dropdown.ClearOptions();
            var values = Enum.GetValues(Entry.BoxedValue.GetType());
            List<string> convertedValues = new List<string>();
            foreach (var value in values)
            {
                convertedValues.Add(value.ToString());
            }
            tmp_dropdown.AddOptions(convertedValues);

            var dropdownValue = -1;
            for (int i = 0; i < values.Length; i++)
            {
                if (convertedValues[i] == Entry.BoxedValue.ToString())
                {
                    dropdownValue = i;
                }
            }
            
            tmp_dropdown.onValueChanged.RemoveAllListeners();
            tmp_dropdown.onValueChanged.AddListener((int i) =>
            {
                if (ConfigurationManager.LogLevel >= ModdingUtils.LogLevel.Medium)
                    ConfigurationManager._logger.LogInfo($"{Entry.Definition.Key} has been updated");
                Entry.BoxedValue = Enum.Parse(Entry.BoxedValue.GetType(), tmp_dropdown.options[i].text);
                Attributes?.CallbackAction?.Invoke(i);
            });
            tmp_dropdown.value = dropdownValue;
            
            if (Entry.Description.Tags.Length > 0)
            {
                foreach (var descriptionTag in Entry.Description.Tags)
                {
                    if (descriptionTag is ConfigurationManagerAttributes managerAttributes)
                        Attributes = managerAttributes;
                }
            }
        }

        private void RevertToDefault()
        {
            Entry.BoxedValue = Entry.DefaultValue;
            gameObject.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = Entry.BoxedValue.ToString();
        }
    }
}
