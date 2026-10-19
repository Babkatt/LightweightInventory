using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace LightweightInventory
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInIncompatibility("shudnal.ExtraSlots")]
    public sealed class LightweightInventoryPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "christian.lightweightinventory";
        public const string PluginName = "Lightweight Inventory";
        public const string PluginVersion = "1.0.0";

        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<int> FirstRowIndex;
        internal static ConfigEntry<float> WeightMultiplier;
        private Harmony _harmony;
        // Diagnostic Counters - uncomment when debugging.
        // internal static int WeightUpdateCalls = 0;
        // internal static int ItemWeightCalls = 0;
        internal static int GetPlayableInventoryHeight(Inventory inventory)
        {
            if (inventory == null) return 0;

            int height = inventory.GetHeight();

            bool senealLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos
                .ContainsKey("seneaL.valheim.ui");

            return Math.Max(0, height - (senealLoaded ? 5 : 0));
        }

        private void Awake()
        {
            Enabled = Config.Bind("General", "Enabled", true, "Enable reduced carried weight for items located in selected inventory rows.");
            FirstRowIndex = Config.Bind("Lightweight Rows", "FirstRowIndex", -2,
                new ConfigDescription("Negative = number of bottom rows (-2 means bottom two); zero = disabled; positive = 1-based first affected row through the bottom.",
                    new AcceptableValueRange<int>(-9, 9)));
            WeightMultiplier = Config.Bind("Lightweight Rows", "WeightMultiplier", 0.5f,
                new ConfigDescription("Item weight multiplier in affected rows: 0.5 = half weight; 1 = normal weight.",
                    new AcceptableValueRange<float>(0f, 1f)));

            Enabled.SettingChanged += OnSettingChanged;
            FirstRowIndex.SettingChanged += OnSettingChanged;
            WeightMultiplier.SettingChanged += OnSettingChanged;

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(LightweightInventoryPlugin).Assembly);
            Logger.LogInfo("Lightweight Inventory v1.0.0 loaded");
        }

        private static void OnSettingChanged(object sender, EventArgs args)
        {
            var player = Player.m_localPlayer;
            if (player == null) return;

            var inventory = player.GetInventory();
            if (inventory == null) return;

            // float before = inventory.GetTotalWeight();

            var method = AccessTools.Method(typeof(Inventory), "UpdateTotalWeight");
            method?.Invoke(inventory, null);

            // float after = inventory.GetTotalWeight();

            /* DebugLog($"Config changed: Rows={FirstRowIndex.Value}, " +
                     $"Multiplier={WeightMultiplier.Value}, " +
                     $"Weight before={before}, after={after}, " +
                     $"Method found={method != null}"); */
        }
        /* internal static void DebugLog(string message)
        {
            UnityEngine.Debug.Log("[LightweightInventory] " + message);
        } */
        private void OnDestroy()
        {
            if (_harmony != null) _harmony.UnpatchSelf();
        }

        internal static bool IsAffected(int y, int height)
        {
            if (Enabled == null || !Enabled.Value || FirstRowIndex == null || height <= 0 || y < 0 || y >= height) return false;
            int index = FirstRowIndex.Value;
            if (index == 0) return false;
            return index < 0 ? y >= Math.Max(0, height + index) : y >= index - 1;
        }


        [HarmonyPatch(typeof(Inventory), "UpdateTotalWeight")]
        private static class UpdateTotalWeightPatch
        {
            internal static bool InWeightUpdate;

            private static void Prefix(Inventory __instance, out bool __state)
            {
                __state = InWeightUpdate;

                var player = Player.m_localPlayer;
                bool isPlayerInventory = player != null &&
                                         __instance != null &&
                                         ReferenceEquals(__instance, player.GetInventory());

                InWeightUpdate = isPlayerInventory;

                /*
                // Diagnostic logging
                if (isPlayerInventory && WeightUpdateCalls++ < 10)
                {
                    DebugLog($"UpdateTotalWeight called. Height={__instance.GetHeight()}, LightweightRows={FirstRowIndex.Value}");
                }
                */
            }

            private static void Postfix(bool __state)
            {
                InWeightUpdate = __state;
            }

            private static Exception Finalizer(Exception __exception, bool __state)
            {
                InWeightUpdate = __state;
                return __exception;
            }
        }

        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetWeight))]
        private static class ItemWeightPatch
        {
            private static void Postfix(ItemDrop.ItemData __instance, ref float __result)
            {
                if (__instance == null) return;

                var player = Player.m_localPlayer;
                if (player == null) return;

                var inventory = player.GetInventory();
                if (inventory == null) return;

                bool affected = IsAffected(__instance.m_gridPos.y, GetPlayableInventoryHeight(inventory));

                /* if (ItemWeightCalls++ < 20)
                {
                    DebugLog(
                        $"GetWeight called: Position={__instance.m_gridPos}, " +
                        $"Affected={affected}, InWeightUpdate={UpdateTotalWeightPatch.InWeightUpdate}, " +
                        $"OriginalWeight={__result}"
                    );
                } */

                if (!UpdateTotalWeightPatch.InWeightUpdate ||
                    !affected ||
                    WeightMultiplier == null ||
                    WeightMultiplier.Value >= 1f)
                    return;

                __result *= WeightMultiplier.Value;
            }
        }
    }
}
