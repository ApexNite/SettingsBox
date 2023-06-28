using Proyecto26;
using System;
using UnityEngine;
using Steamworks;
using System.IO;
using System.Collections.Generic;
using System.Timers;

namespace SettingsBox {
    internal static class Analytics {
        public class UserData {
            public string id = SystemInfo.deviceUniqueIdentifier;
            public bool optedOut = false;
            public bool anonymized;
            public string discordId;
            public string discordUsername;
            public string discordDiscriminator;
            public string steamId;
            public string steamUsername;
            public string lastLaunchTime;
            public string settingsBoxVersion;
            public string worldBoxVersion;
            public float batteryLevel;
            public int processorCount;
            public int systemMemorySize;
            public int modCount;
            public int fileCount;
            public GameStatsData gameStats;
            public List<string> mods = new List<string>();
            public List<string> files = new List<string>();
        }

        public class OptedOutUserData {
            public string id = SystemInfo.deviceUniqueIdentifier;
            public bool optedOut = true;
        }

        private static Setting settingAnalytics;
        private static Setting settingAnonymousAnalytics;

        internal static void initialize() {
            settingAnalytics = SettingsManager.bindSetting("Analytics",
                "Toggle to share important analytics data. Opting out deletes analytic data.",
                true, 
                pAction: changeSharingStatus);
            settingAnonymousAnalytics = SettingsManager.bindSetting("Anonymous Analytics",
                "Toggle to share anonymous analytics data. Enabling anonymizes saved analytics.",
                false,
                pAction: anonymizeData);

            if ((bool)settingAnonymousAnalytics.Value) {
                putAnonymousAnalytics();
                return;
            }

            if ((bool)settingAnalytics.Value) {
                Delayed(60000, putAnalytics);
                return;
            }

            optOutOfAnalytics();
        }

        private static void changeSharingStatus(bool status) {
            if (status) {
                Delayed(10000, putAnalytics);
            } else {
                optOutOfAnalytics();
            }
        }

        private static void anonymizeData(bool status) {
            if (status) {
                putAnonymousAnalytics();
            } else {
                Delayed(10000, putAnalytics);
            }
        }

        private static void putAnonymousAnalytics() {
            UserData userData = new UserData();

            userData = getBaseData(userData);
            userData.anonymized = true;

            RestClient.Put("https://settings-box-default-rtdb.firebaseio.com/users/" + userData.id + "/.json", userData);
        }

        private static void putAnalytics() {
            if (!(bool)settingAnalytics.Value) {
                return;
            }

            UserData userData = new UserData();

            if (DiscordTracker.haveUser) {
                userData.discordId = Config.discordId;
                userData.discordUsername = Config.discordName;
                userData.discordDiscriminator = Config.discordDiscriminator;
            }

            if (SteamClient.IsValid) {
                userData.steamId = SteamClient.SteamId.Value.ToString();
                userData.steamUsername = SteamClient.Name;
            }

            userData = getBaseData(userData);
            userData.anonymized = false;

            RestClient.Put("https://settings-box-default-rtdb.firebaseio.com/users/" + userData.id + "/.json", userData);
        }

        private static void optOutOfAnalytics() {
            OptedOutUserData userData = new OptedOutUserData();
            RestClient.Put("https://settings-box-default-rtdb.firebaseio.com/users/" + userData.id + "/.json", userData);
        }

        private static UserData getBaseData(UserData pUserData) {
            string mainPath = Path.GetFullPath(Path.Combine(Application.dataPath, @"..\"));
            pUserData = searchDirectory(pUserData, mainPath + "Mods", true);
            pUserData = searchDirectory(pUserData, mainPath + "BepInEx/plugins", true);
            pUserData = searchDirectory(pUserData, Application.streamingAssetsPath + "/Mods", false);

            pUserData.lastLaunchTime = DateTime.Now.ToUniversalTime().Ticks.ToString();
            pUserData.settingsBoxVersion = "1.0.0";
            pUserData.worldBoxVersion = Application.version;
            pUserData.batteryLevel = SystemInfo.batteryLevel;
            pUserData.processorCount = SystemInfo.processorCount;
            pUserData.systemMemorySize = SystemInfo.systemMemorySize;
            pUserData.modCount = pUserData.mods.Count;
            pUserData.modCount = pUserData.files.Count;
            pUserData.gameStats = MapBox.instance.gameStats.data;

            return pUserData;
        }

        private static UserData searchDirectory(UserData pUserData, string pPath, bool pDirectories) {
            if (Directory.Exists(pPath)) {
                FileInfo[] files = new DirectoryInfo(pPath).GetFiles();
                DirectoryInfo[] directories = new DirectoryInfo(pPath).GetDirectories();

                for (int i = 0; i < files.Length; i++) {
                    if (files[i].Extension.Equals(".dll") || files[i].Extension.Equals(".mod") || files[i].Extension.Equals(".zip")) {
                        pUserData.mods.Add(files[i].Name);
                    } else {
                        pUserData.files.Add(files[i].Name);
                    }
                }

                if (pDirectories) {
                    for (int i = 0; i < directories.Length; i++) {
                        pUserData.mods.Add(directories[i].Name);
                    }
                }
            }

            return pUserData;
        }

        // https://stackoverflow.com/a/14126074
        private static void Delayed(int delay, Action action) {
            Timer timer = new Timer();
            timer.Interval = delay;
            timer.Elapsed += (s, e) => {
                action();
                timer.Stop();
            };
            timer.Start();
        }
    }
}
