using Proyecto26;
using System;
using UnityEngine;
using Steamworks;
using System.IO;
using System.Collections.Generic;

namespace SettingsBox {
    public static class Analytics {
        [Serializable]
        public class UserData {
            public string id = SystemInfo.deviceUniqueIdentifier;
            public bool optedOut;
            public bool anonymized;
            public string steamId;
            public string steamUsername;
            public string lastLaunchTime;
            public string settingsBoxVersion;
            public string worldBoxVersion;
            public float batteryLevel;
            public int processorCount;
            public int systemMemorySize;
            public int modCount;
            public GameStatsData gameStats;
            public List<string> mods = new List<string>();
            public List<string> files = new List<string>();
        }

        private static Setting settingAnalytics;
        private static Setting settingAnonymousAnalytics;

        public static void initialize() {
            settingAnalytics = SettingsManager.bindSetting("Analytics",
                "Toggle to share important analytics data. Opting out deletes analytic data.",
                true, 
                pAction: changeSharingStatus);
            settingAnonymousAnalytics = SettingsManager.bindSetting("Anonymous Analytics",
                "Toggle to share anonymous analytics data. Enabling anonymizes saved analytics.",
                true, 
                pAction: anonymizeData);

            if ((bool)settingAnonymousAnalytics.Value) {
                putAnonymousAnalytics();
                return;
            }

            if ((bool)settingAnalytics.Value) {
                putAnalytics();
                return;
            } else {
                optOutOfAnalytics();
                return;
            }
        }

        private static void putAnonymousAnalytics() {
            UserData userData = new UserData();

            userData = getBaseData(userData);

            userData.optedOut = false;
            userData.anonymized = true;

            RestClient.Put("https://settings-box-default-rtdb.firebaseio.com/users/" + userData.id + "/.json", userData);
        }

        private static void putAnalytics() {
            UserData userData = new UserData();

            if (SteamClient.IsValid) {
                userData.steamId = SteamClient.SteamId.Value.ToString();
                userData.steamUsername = SteamClient.Name;
            }

            userData = getBaseData(userData);

            userData.optedOut = false;
            userData.anonymized = false;

            RestClient.Put("https://settings-box-default-rtdb.firebaseio.com/users/" + userData.id + "/.json", userData);
        }

        private static UserData getBaseData(UserData pUserData) {
            string mainPath = Path.GetFullPath(Path.Combine(Application.dataPath, @"..\"));
            pUserData = searchDirectory(pUserData, mainPath + "/Mods", true);
            pUserData = searchDirectory(pUserData, mainPath + "../BepInEx/plugins", true);
            pUserData = searchDirectory(pUserData, Application.streamingAssetsPath + "/Mods");

            pUserData.lastLaunchTime = DateTime.Now.ToUniversalTime().Ticks.ToString();
            pUserData.settingsBoxVersion = "1.0.0";
            pUserData.worldBoxVersion = Application.version;
            pUserData.batteryLevel = SystemInfo.batteryLevel;
            pUserData.processorCount = SystemInfo.processorCount;
            pUserData.systemMemorySize = SystemInfo.systemMemorySize;
            pUserData.modCount = pUserData.mods.Count;
            pUserData.gameStats = MapBox.instance.gameStats.data;

            return pUserData;
        }

        private static UserData searchDirectory(UserData pUserData, string pPath, bool pDirectories = false) {
            if (Directory.Exists(pPath)) {
                FileInfo[] files = new DirectoryInfo(pPath).GetFiles();
                DirectoryInfo[] directories = new DirectoryInfo(pPath).GetDirectories();

                for (int i = 0; i < files.Length; i++) {
                    if (files[i].Extension.Contains("dll") || files[i].Extension.Contains("mod")) {
                        pUserData.mods.Add(files[i].Name);
                    } else {
                        pUserData.files.Add(files[i].Name);
                    }
                }

                for (int i = 0; i < directories.Length; i++) {
                    if (!directories[i].Name.Contains("Example") && pDirectories) {
                        pUserData.mods.Add(directories[i].Name);
                    }
                }
            }

            return pUserData;
        }

        private static void optOutOfAnalytics() {
            UserData userData = new UserData();

            userData.optedOut = true;

            RestClient.Put("https://settings-box-default-rtdb.firebaseio.com/users/" + userData.id + "/.json", userData);
        }

        private static void changeSharingStatus(bool pStatus) {
            if (pStatus) {
                putAnalytics();
            } else {
                optOutOfAnalytics();
            }
        }

        private static void anonymizeData(bool pStatus) {
            if (pStatus) {
                putAnonymousAnalytics();
            } else {
                putAnalytics();
            }
        }
    }
}
