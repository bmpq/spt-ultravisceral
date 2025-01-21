using AssetBundleLoader;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT;
using ultravisceral;

[BepInPlugin("com.tarkin.ultravisceral", "ultravisceral", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Log;

    public static ConfigEntry<float> BloodSplatterSize { get; set; }
    public static ConfigEntry<float> MinDistanceDecals { get; set; }
    public static ConfigEntry<int> MaxStaticDecalAmount { get; set; }

    private void Start()
    {
        Log = base.Logger;

        InitConfiguration();

        new PatchDeferredDecalRenderer().Enable();
        new OnGameStarted().Enable();
        new PatchDamage().Enable();

        PreloadBundles();
    }

    void PreloadBundles()
    {
        BundleLoader.LoadAssetBundle("bloodfx.bundle");
        BundleLoader.LoadAssetBundle("blood_particles.bundle");
    }

    private void InitConfiguration()
    {
        BloodSplatterSize = Config.Bind<float>("", "BloodFX Splatter Size", 1f, "");
        MinDistanceDecals = Config.Bind<float>("", "MinDistanceDecals", 0.5f, "");
        MaxStaticDecalAmount = Config.Bind<int>("", "MaxStaticDecalAmount", 200, "");
    }
}