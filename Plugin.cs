using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MistBeGone
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class MistBeGonePlugin : BaseUnityPlugin
    {
        internal const string ModName = "MistBeGone";
        internal const string ModVersion = "1.0.3";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        private static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion, ModRequired = true };
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource MistBeGoneLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        /// <summary>
        /// Cached boolean indicating if we should remove mist.
        /// Updated only when config changes or a new global key is set/removed.
        /// </summary>
        internal static bool SRemoveMist = false;

        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        internal static ConfigEntry<Toggle> UseGlobalKeys = null!;
        private static ConfigEntry<string> _globalKeysNeededList = null!;

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription = new(description.Description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"), description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        internal ConfigEntry<T> textEntryConfig<T>(string group, string name, T value, string desc, bool synchronizedSetting = true)
        {
            ConfigurationManagerAttributes attributes = new()
            {
                CustomDrawer = TextAreaDrawer
            };
            return config(group, name, value, new ConfigDescription(desc, null, attributes), synchronizedSetting);
        }

        internal static void TextAreaDrawer(ConfigEntryBase entry)
        {
            GUILayout.ExpandHeight(true);
            GUILayout.ExpandWidth(true);
            entry.BoxedValue = GUILayout.TextArea((string)entry.BoxedValue, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }

        #endregion

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, new ConfigDescription("If on, the configuration is locked and can be changed by server admins only.", null, new ConfigurationManagerAttributes() { Order = 3 }));
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            UseGlobalKeys = config("1 - General", "Use Global Keys", Toggle.Off, new ConfigDescription("If on, the mist will only be removed if all required global keys are set.", null, new ConfigurationManagerAttributes() { Order = 2 }));

            _globalKeysNeededList = textEntryConfig("1 - General", "Global Keys Needed", "defeated_eikthyr,defeated_dragon,defeated_gdking,defeated_bonemass,defeated_goblinking", $"Comma-separated list of global keys required. Example: 'defeated_eikthyr,defeated_dragon'\nAccepted values are {string.Join(", ", Enum.GetNames(typeof(GlobalKeys)))}");

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);

            RecalculateShouldRemoveMist();

            SetupWatcher();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Config.Reload();
                RecalculateShouldRemoveMist();
            }
            catch
            {
                MistBeGoneLogger.LogError($"There was an issue loading your {ConfigFileName}");
                MistBeGoneLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        public static bool AreAllRequiredGlobalKeysSet()
        {
            HashSet<GlobalKeys> requiredSet = ParseGlobalKeysFromString(_globalKeysNeededList.Value);

            if (requiredSet.Count == 0) return true;

            if (ZoneSystem.instance == null)
            {
                return false;
            }

            foreach (GlobalKeys key in requiredSet)
            {
                if (!ZoneSystem.instance.m_globalKeysEnums.Contains(key))
                {
                    //MistBeGoneLogger.LogError($"Required global key {key} is missing in the game.");
                    return false;
                }
            }

            MistBeGoneLogger.LogDebug("All required global keys are set.");
            return true;
        }


        internal static void RecalculateShouldRemoveMist()
        {
            bool shouldRemove;
            try
            {
                shouldRemove = UseGlobalKeys.Value == Toggle.Off || AreAllRequiredGlobalKeysSet();
            }
            catch (Exception ex)
            {
                MistBeGoneLogger.LogError($"Error calculating ShouldRemoveMist:\n{ex}");
                shouldRemove = false;
            }

            SRemoveMist = shouldRemove;
        }

        public static HashSet<GlobalKeys> ParseGlobalKeysFromString(string input)
        {
            HashSet<GlobalKeys> results = [];

            if (string.IsNullOrWhiteSpace(input))
                return results; // nothing to parse => empty set


            string[] split = input.Split([','], StringSplitOptions.RemoveEmptyEntries);

            foreach (string raw in split)
            {
                string trimmed = raw.Trim();
                if (Enum.TryParse(trimmed, out GlobalKeys parsed))
                {
                    MistBeGoneLogger.LogDebug($"Parsed global key: '{parsed}'");
                    results.Add(parsed);
                }
                else
                {
                    MistBeGoneLogger.LogWarning($"Invalid GlobalKey in config: '{trimmed}'");
                }
            }

            return results;
        }
    }

    // The ZoneSystem patches here should trigger on both the client and the server.
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GlobalKeyAdd), new Type[] { typeof(string), typeof(bool) })]
    public static class Patch_ZoneSystem_GlobalKeyAdd
    {
        public static void Postfix()
        {
            MistBeGonePlugin.RecalculateShouldRemoveMist();
        }
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GlobalKeyRemove), new Type[] { typeof(string), typeof(bool) })]
    public static class Patch_ZoneSystem_GlobalKeyRemove
    {
        public static void Postfix()
        {
            MistBeGonePlugin.RecalculateShouldRemoveMist();
        }
    }

    [HarmonyPatch(typeof(MistEmitter), nameof(MistEmitter.SetEmit))]
    static class MistEmitterAwakePatch
    {
        static void Prefix(MistEmitter __instance, ref bool emit)
        {
            try
            {
                //Object.Destroy(__instance.gameObject);
                __instance.m_emit = !MistBeGonePlugin.SRemoveMist;
            }
            catch (Exception e)
            {
                MistBeGonePlugin.MistBeGoneLogger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(MistEmitter), nameof(MistEmitter.Update))]
    static class SetBackMistEmitterUpdatePatch
    {
        static void Prefix(MistEmitter __instance)
        {
            try
            {
                __instance.m_emit = !MistBeGonePlugin.SRemoveMist;
            }
            catch (Exception e)
            {
                MistBeGonePlugin.MistBeGoneLogger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(ParticleMist), nameof(ParticleMist.Awake))]
    static class ParticleMistAwakePatch
    {
        static void Postfix(ParticleMist __instance)
        {
            try
            {
                if (__instance == null || __instance.m_ps == null) return;
                ParticleSystem.EmissionModule emissionModule = __instance.m_ps.emission;
                emissionModule.enabled = !MistBeGonePlugin.SRemoveMist;
            }
            catch (Exception e)
            {
                MistBeGonePlugin.MistBeGoneLogger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(ParticleMist), nameof(ParticleMist.Update))]
    static class SetBackParticleMistUpdatePatch
    {
        static void Prefix(ParticleMist __instance)
        {
            try
            {
                if (__instance == null || __instance.m_ps == null) return;
                ParticleSystem.EmissionModule emissionModule = __instance.m_ps.emission;
                emissionModule.enabled = !MistBeGonePlugin.SRemoveMist;
            }
            catch (Exception e)
            {
                MistBeGonePlugin.MistBeGoneLogger.LogError(e);
            }
        }
    }
}