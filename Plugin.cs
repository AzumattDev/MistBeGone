using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace MistBeGone
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class MistBeGonePlugin : BaseUnityPlugin
    {
        internal const string ModName = "MistBeGone";
        internal const string ModVersion = "1.0.2";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource MistBeGoneLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }
    }

    [HarmonyPatch(typeof(MistEmitter), nameof(MistEmitter.SetEmit))]
    static class MistEmitterAwakePatch
    {
        static void Prefix(MistEmitter __instance, ref bool emit)
        {
            try
            {
                Object.DestroyImmediate(__instance.gameObject);
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
                if (__instance != null && __instance.m_ps != null)
                    Object.DestroyImmediate(__instance.gameObject);
            }
            catch (Exception e)
            {
                MistBeGonePlugin.MistBeGoneLogger.LogError(e);
            }
        }
    }
}