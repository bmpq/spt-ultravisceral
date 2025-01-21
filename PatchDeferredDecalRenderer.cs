using AssetBundleLoader;
using DeferredDecals;
using EFT.Ballistics;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Systems.Effects;
using UnityEngine;
using static DeferredDecals.DeferredDecalRenderer;

namespace ultravisceral
{
    internal class PatchDeferredDecalRenderer : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(DeferredDecalRenderer), nameof(DeferredDecalRenderer.Awake));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref DeferredDecalRenderer __instance)
        {
            MaxDecals(__instance);

            return true;
        }

        [PatchPostfix]
        private static void PatchPostfix(ref DeferredDecalRenderer __instance)
        {
            ReplaceGenericDecal(__instance);
        }

        static void MaxDecals(DeferredDecalRenderer __instance)
        {
            FieldInfo field = typeof(DeferredDecalRenderer).GetField("_maxDecals", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(__instance, Plugin.MaxStaticDecalAmount.Value);
        }

        static void ReplaceGenericDecal(DeferredDecalRenderer __instance)
        {
            FieldInfo field4 = typeof(DeferredDecalRenderer).GetField("_genericDecal", BindingFlags.Instance | BindingFlags.NonPublic);
            SingleDecal genericDecal = (SingleDecal)field4.GetValue(__instance);

            SingleDecal decal = new SingleDecal();

            Material mat = genericDecal.DecalMaterial;
            mat.mainTexture = BundleLoader.LoadAssetBundle("ultrablood").LoadAsset<Texture>("BloodSheet");
            mat.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            decal.TileSheetRows = 4;
            decal.TileSheetColumns = 4;

            decal.DecalSize = new Vector2(3f, 3f); // has no effect?
            decal.DecalMaterial = mat;
            decal.DynamicDecalMaterial = decal.DecalMaterial;
            decal.DecalMaterialType = new MaterialType[] { MaterialType.None };
            decal.Init();

            field4.SetValue(__instance, decal);
        }
    }
}
