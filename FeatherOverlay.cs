using System;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace LightweightInventory
{
    // ExtraSlots-style empty-slot feather, using InventoryElement.m_icon.
    // Purely visual: weight patches and inventory dimensions are unchanged.
    [HarmonyPatch(typeof(InventoryGui), "UpdateInventory")]
    internal static class FeatherOverlay
    {
        private static Sprite featherSprite;
        private static bool loadAttempted;
        private static Material originalMaterial;
        private static Vector3 originalScale = Vector3.zero;

        [HarmonyPriority(Priority.Last)]
        private static void Postfix(InventoryGui __instance)
        {
            if (__instance == null || __instance.m_playerGrid == null) return;
            var player = Player.m_localPlayer;
            if (player == null) return;
            var inventory = player.GetInventory();
            if (inventory == null) return;

            if (!loadAttempted) LoadFeather();
            if (featherSprite == null) return;

            int width = inventory.GetWidth();
            int height = LightweightInventoryPlugin.GetPlayableInventoryHeight(inventory);
            var elements = AccessTools.Field(typeof(InventoryGrid), "m_elements")
                ?.GetValue(__instance.m_playerGrid) as System.Collections.Generic.List<InventoryElement>;

            if (elements == null || width <= 0 || height <= 0) return;

            int count = Math.Min(elements.Count, width * height);
            for (int i = 0; i < count; i++)
            {
                InventoryElement element = elements[i];
                if (element == null || element.m_icon == null) continue;

                Image icon = element.m_icon;
                if (originalMaterial == null && icon.material != null)
                    originalMaterial = icon.material;
                if (originalScale == Vector3.zero && icon.material != null)
                    originalScale = icon.transform.localScale;

                bool show = LightweightInventoryPlugin.Enabled != null && LightweightInventoryPlugin.Enabled.Value &&
                            LightweightInventoryPlugin.IsAffected(i / width, height) && !element.m_used;

                if (show)
                {
                    // Mirrors ExtraSlots.EquipmentPanel.SetInventorySlotBackgroundImage.
                    icon.enabled = true;
                    icon.material = null;
                    icon.sprite = featherSprite;
                    icon.transform.localScale = Vector3.one * 0.7f;
                    icon.color = Color.grey - new Color(0f, 0f, 0f, 0.7f);
                }
                else if (icon.sprite == featherSprite)
                {
                    // Avoid replacing the item sprite: the inventory UI owns occupied icons.
                    icon.material = originalMaterial;
                    icon.transform.localScale = originalScale == Vector3.zero ? Vector3.one : originalScale;
                    if (!element.m_used) icon.enabled = false;
                }
            }
        }

        private static void LoadFeather()
        {
            loadAttempted = true;
            string path = Path.Combine(
                Path.GetDirectoryName(typeof(FeatherOverlay).Assembly.Location),
                "lightenedslot.png");
            if (!File.Exists(path))
            {
                Debug.LogWarning("[LightweightInventory] Missing feather PNG: " + path);
                return;
            }
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(path)))
                    throw new InvalidDataException("Failed to decode feather PNG.");
                featherSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                Debug.Log("[LightweightInventory] Feather icon loaded.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[LightweightInventory] Feather load error: " + ex);
            }
        }
    }
}
