using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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
        private const string PLUGIN_MIN_VERSION = "0.2.0";
        
        private static ConfigSync configSync;
        private static ConfigEntry<int> playerBaseArmor;
        private static ConfigEntry<float> gearArmorMultiplier;
        private static ConfigEntry<int> gearArmorPerLevel;
        private static ConfigEntry<int> gearFlatArmor;
        private static ConfigEntry<int> capeArmorPerLevel;
        private static ConfigEntry<int> capeFlatArmor;

        private void Awake()
        {
            configSync = new (PLUGIN_GUID) {
                DisplayName = PLUGIN_NAME,
                CurrentVersion = PLUGIN_VERSION,
                MinimumRequiredVersion = PLUGIN_MIN_VERSION
            };

            configSync.AddLockingConfigEntry(config("0 - Config", "configLocked", true, "Force Server Config"));

            playerBaseArmor = config("1 - Player", "playerBaseArmor", 0,
                new ConfigDescription("Adds flat armor to players.",
                new AcceptableValueRange<int>(-50, 150)));

            gearArmorMultiplier = config("2 - Gear", "gearArmorMultiplier", 1.0f,
                new ConfigDescription("Multiply base armor of Head, Chest, and Legs.",
                new RoundedValueRange(0.0f, 5.0f, 0.05f)));
            gearArmorPerLevel = config("2 - Gear", "gearArmorPerLevel", 2,
                new ConfigDescription("Armor added per quality level for Head, Chest, Legs.",
                new AcceptableValueRange<int>(0, 10)));
            gearFlatArmor = config("2 - Gear", "gearFlatArmor", 0,
                new ConfigDescription("Adds additional flat armor to Head, Chest, Legs.",
                new AcceptableValueRange<int>(0, 40)));
            
            capeArmorPerLevel = config("3 - Shoulders", "capeArmorPerLevel", 1,
                new ConfigDescription("Armor added per quality level for Shoulders.",
                new AcceptableValueRange<int>(0, 6)));
            capeFlatArmor = config("3 - Shoulders", "capeFlatArmor", 0,
                new ConfigDescription("Adds additional flat armor to Shoulders.",
                new AcceptableValueRange<int>(0, 10)));

            new Harmony(PLUGIN_GUID).PatchAll(typeof(SmoothArmorScalingPatches));
        }

        class SmoothArmorScalingPatches
        {
            private static readonly MethodInfo m_TransformArmor = AccessTools.Method(typeof(SmoothArmorScalingPatches), "TransformArmor");
            private static readonly MethodInfo m_TransformArmorPerLevel = AccessTools.Method(typeof(SmoothArmorScalingPatches), "TransformArmorPerLevel");
            private static FieldInfo f_Armor = AccessTools.Field(typeof(ItemDrop.ItemData.SharedData), "m_armor");
            private static FieldInfo f_ArmorPerLevel = AccessTools.Field(typeof(ItemDrop.ItemData.SharedData), "m_armorPerLevel");

            static float TransformArmor(float armor)
            {
                return (float)Math.Round((armor > 0) ? (gearArmorMultiplier.Value * armor) + gearFlatArmor.Value : capeFlatArmor.Value - armor);
            }

            static float TransformArmorPerLevel(float armor)
            {
                return (armor > 0) ? gearArmorPerLevel.Value : capeArmorPerLevel.Value;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ZNetScene), "Awake")]
            [HarmonyPriority(Priority.Last)]
            static void ZNetSceneAwakePostfix()
            {
                List<Recipe> recipes = ObjectDB.instance.m_recipes.FindAll((r) => r.name.Contains("Cape"));
                foreach (Recipe recipe in recipes)
                {
                    recipe.m_item.m_itemData.m_shared.m_armor = -recipe.m_item.m_itemData.m_shared.m_armor;
                    recipe.m_item.m_itemData.m_shared.m_armorPerLevel = -recipe.m_item.m_itemData.m_shared.m_armorPerLevel;
                }
            }

            [HarmonyTranspiler]
            [HarmonyPatch(typeof(ItemDrop.ItemData), "GetArmor", new[] { typeof(int), typeof(float) })]
            static IEnumerable<CodeInstruction> GetArmorTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    yield return instruction;
                    if (instruction.LoadsField(f_Armor))
                    {
                        yield return new CodeInstruction(OpCodes.Call, m_TransformArmor);
                    }
                    else if (instruction.LoadsField(f_ArmorPerLevel))
                    {
                        yield return new CodeInstruction(OpCodes.Call, m_TransformArmorPerLevel);
                    }
                }
            }

            [HarmonyTranspiler]
            [HarmonyPatch(typeof(Player), "GetBodyArmor")]
            static IEnumerable<CodeInstruction> GetBodyArmorTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    if (instruction.opcode == OpCodes.Ret)
                    {
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(SmoothArmorScalingPlugin), "playerBaseArmor"));
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(BepInEx.Configuration.ConfigEntry<int>), "get_Value"));
                        yield return new CodeInstruction(OpCodes.Conv_R4);
                        yield return new CodeInstruction(OpCodes.Add);
                    }
                    yield return instruction;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HitData.DamageTypes), "ApplyArmor", new [] { typeof(float), typeof(float) })]
            [HarmonyPriority(Priority.Last)]
            static bool ApplyArmorPrefix(float dmg, float ac, ref float __result)
            {
                float basis = 1.0f + (ac / dmg);
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

        class RoundedValueRange : AcceptableValueRange<float>
        {
            private float step;
            public RoundedValueRange(float min, float max, float step = 0.01f) : base(min, max)
            {
                this.step = step;
            }
            public override object Clamp(object value)
            {
                float v = Convert.ToSingle(value) + (0.5f * step);
                return (v < this.MinValue) ? this.MinValue : (v > this.MaxValue) ? this.MaxValue : v - (v % step);
            }
            public override bool IsValid(object value)
            {
                float v = Convert.ToSingle(value);
                return (v < this.MinValue) && (v > this.MaxValue) && (v % step == 0);
            }
        }
    }
}
