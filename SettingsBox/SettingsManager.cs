using System;
using System.Collections.Generic;
using System.Reflection;

namespace SettingsBox {
    public class SettingsManager {
        internal static List<Setting> settings = new List<Setting>();

        public static Setting bindSetting(string pName, string pDescription, bool pDefaultValue, string pIcon = null, Action<bool> pAction = null) {
            if (!HookManager.hooked) {
                HookManager.Hook();
            }
            
            PlayerOptionData playerOptionData = new PlayerOptionData(pName) { boolVal = pDefaultValue };
            PlayerConfig.instance.data.add(playerOptionData);

            Setting setting = new Setting(pName, pDescription, PlayerConfig.optionBoolEnabled(pName), pIcon, Assembly.GetCallingAssembly().GetName().Name);
            settings.Add(setting);

            OptionAsset optionAsset = new OptionAsset {
                id = pName,
                translation_key_description = "option_description_" + pName,
                default_bool = pDefaultValue,
                type = OptionType.Bool,
                action = delegate (OptionAsset pAsset) {
                    setting.Value = PlayerConfig.optionBoolEnabled(pAsset.id);

                    if (pAction != null) {
                        pAction((bool)setting.Value);
                    }
                }
            };

            AssetManager.options_library.add(optionAsset);
            LocalizedTextManager.instance.localizedText.Add(pName, pName);
            LocalizedTextManager.instance.localizedText.Add("option_description_" + pName, pDescription);

            return setting;
        }

        public static Setting bindSetting(string pName, string pDescription, int pDefaultValue, int pMax = 100, int pMin = 0, bool pPercent = false, string pIcon = null, Action<int> pAction = null) {
            if (!HookManager.hooked) {
                HookManager.Hook();
            }

            PlayerOptionData playerOptionData = new PlayerOptionData(pName) { intVal = pDefaultValue };
            PlayerConfig.instance.data.add(playerOptionData);

            Setting setting = new Setting(pName, pDescription, PlayerConfig.getOptionInt(pName), pIcon, Assembly.GetCallingAssembly().GetName().Name);
            settings.Add(setting);

            OptionAsset optionAsset = new OptionAsset() {
                id = pName,
                translation_key_description = "option_description_" + pName,
                type = OptionType.Int,
                default_int = pDefaultValue,
                max_value = pMax,
                min_value = pMin,
                action = delegate (OptionAsset pAsset) {
                    setting.Value = PlayerConfig.getOptionInt(pAsset.id);

                    if (pAction != null) {
                        pAction((int)setting.Value);
                    }
                }
            };

            if (pPercent) {
                optionAsset.counter_format = new ActionFormatCounterOptionAsset(AssetManager.options_library.getCounterFormat100Percent);
            }

            AssetManager.options_library.add(optionAsset);
            LocalizedTextManager.instance.localizedText.Add(pName, pName);
            LocalizedTextManager.instance.localizedText.Add("option_description_" + pName, pDescription);

            return setting;
        }
    }
}
