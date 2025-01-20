using DeferredDecals;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace ultravisceral
{
    internal class PatchMaxDecals : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(DeferredDecalRenderer), nameof(DeferredDecalRenderer.Awake));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref DeferredDecalRenderer __instance)
        {
            FieldInfo field = typeof(DeferredDecalRenderer).GetField("_maxDecals", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(__instance, Plugin.MaxStaticDecalAmount.Value);

            return true;
        }
    }
}
