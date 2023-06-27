using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace SettingsBox {
    internal static class HookManager {
        internal static bool hooked = false;
        internal static bool objectsInitiated = false;

        internal static void Hook() {
            if (!Config.gameLoaded) {
                throw new System.Exception("Game hooked too early. This error occurs when bindSetting was executed before the game was loaded (Config.gameLoaded == true).");
            }

            Harmony.CreateAndPatchAll(typeof(HookManager));
            hooked = true;

            Analytics.initialize();
        }
        
        [HarmonyPatch(typeof(ScrollWindow), "showWindow")]
        [HarmonyPostfix]
        internal static void showWindow_PostFix(string pWindowID) {
            if (!pWindowID.Equals("settings") || objectsInitiated) {
                return;
            }

            Transform content = ScrollWindow.allWindows[pWindowID].transform.Find("Background/Scroll View/Viewport/Content");
            GameObject graphicsGrid = content.Find("Graphics/Grid").gameObject;
            GameObject dummyBoolOption = graphicsGrid.transform.Find("bloom").gameObject;
            GameObject dummyIntOption = graphicsGrid.transform.Find("age_overlay_effect").gameObject;
            dummyBoolOption.GetComponent<OptionButton>().enabled = false;
            dummyIntOption.GetComponent<OptionButton>().enabled = false;

            foreach (Setting setting in SettingsManager.settings) {
                GameObject newSection = content.Find(setting.Section) != null ? content.Find(setting.Section).gameObject : GameObject.Instantiate<GameObject>(content.Find("Other").gameObject, content);

                if (content.Find(setting.Section) == null) {
                    newSection.name = setting.Section;

                    LocalizedTextManager.instance.localizedText.Add(setting.Section, setting.Section);
                    newSection.transform.Find("Title").gameObject.GetComponent<LocalizedText>().key = setting.Section;

                    GameObject.Destroy(newSection.transform.Find("Grid/cursor_lights").gameObject);
                    GameObject.Destroy(newSection.transform.Find("Grid/tooltips").gameObject);
                    GameObject.Destroy(newSection.transform.Find("Grid/experimental").gameObject);
                }

                switch (setting.Value.GetType().FullName) {
                    case "System.Boolean":
                        GameObject newBoolOption = GameObject.Instantiate<GameObject>(dummyBoolOption, newSection.transform.Find("Grid"));
                        GameObject iconBoolObject = newBoolOption.transform.Find("OptionArea/Icon").gameObject;
                        newBoolOption.name = setting.Name;
                        newBoolOption.GetComponent<OptionButton>().enabled = true;

                        if (setting.Icon == null) {
                            iconBoolObject.SetActive(false);
                        } else {
                            iconBoolObject.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite(setting.Icon);
                        }
                        break;
                    case "System.Int32":
                        GameObject newIntOption = GameObject.Instantiate<GameObject>(dummyIntOption, newSection.transform.Find("Grid"));
                        GameObject iconIntObject = newIntOption.transform.Find("OptionArea/Icon").gameObject;
                        newIntOption.name = setting.Name;
                        newIntOption.GetComponent<OptionButton>().enabled = true;

                        if (setting.Icon == null) {
                            iconIntObject.SetActive(false);
                        } else {
                            iconIntObject.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite(setting.Icon);
                        }
                        break;
                }
            }

            dummyBoolOption.GetComponent<OptionButton>().enabled = true;
            dummyIntOption.GetComponent<OptionButton>().enabled = true;
            objectsInitiated = true;
        }
    }
}
