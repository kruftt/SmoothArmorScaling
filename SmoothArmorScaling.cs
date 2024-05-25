using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;

namespace SmoothArmorScaling
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInProcess("valheim.exe")]
    public class SmoothArmorScalingPlugin : BaseUnityPlugin
    {
        private const string PLUGIN_GUID = PluginInfo.PLUGIN_GUID;
        private const string PLUGIN_NAME = "Smooth Armor Scaling";
        private const string PLUGIN_VERSION = PluginInfo.PLUGIN_VERSION;
        private const string PLUGIN_MIN_VERSION = "0.1.0";
        private readonly Harmony harmony = new (PLUGIN_GUID);
        private static ConfigEntry<bool> configLocked;
        private static ConfigEntry<double> armorMultiplier;
        private ConfigSync configSync;

        private void Awake()
        {
            configSync = new (PLUGIN_GUID) {
                DisplayName = PLUGIN_NAME,
                CurrentVersion = PLUGIN_VERSION,
                MinimumRequiredVersion = PLUGIN_MIN_VERSION
            };
            
            configLocked = config("General", "configLocked", true, "Force Server Configuration.");
            configSync.AddLockingConfigEntry(configLocked);
            armorMultiplier = config("General", "armorEffectiveness", 1.0,
                new ConfigDescription("Multiply players' effective amount of armor.",
                new RoundedValueRange(0.0, 5.0, 0.05)));
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(HitData.DamageTypes), "ApplyArmor", new [] { typeof(float), typeof(float) })]
        [HarmonyPriority(Priority.Last)]
        class ApplyArmor_Patch
        {
            static bool Prefix(float dmg, float ac, ref float __result)
            {
                float basis = 1.0f + (ac * (float)armorMultiplier.Value / dmg);
                __result = dmg / (basis * basis);
                return false;
            }
        }

        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);
            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;
            return configEntry;
        }

        class RoundedValueRange : AcceptableValueRange<double>
        {
            private double step;
            public RoundedValueRange(double min, double max, double step = 0.01f) : base(min, max)
            {
                this.step = step;
            }
            public override object Clamp(object value)
            {
                double v = Convert.ToDouble(value) + (0.5 * step);
                return (v < this.MinValue) ? this.MinValue : (v > this.MaxValue) ? this.MaxValue : v - (v % step);
            }
            public override bool IsValid(object value)
            {
                double v = Convert.ToDouble(value);
                return (v < this.MinValue) && (v > this.MaxValue) && (v % step == 0);
            }
        }
    }
}
