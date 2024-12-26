using BepInEx;
using BepInEx.Logging;
using EFT;
using ultravisceral;

[BepInPlugin("com.tarkin.ultravisceral", "ultravisceral", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Log;

    private void Start()
    {
        Log = base.Logger;

        InitConfiguration();

        new OnGameStarted().Enable();
        new PatchDamage().Enable();
    }

    private void InitConfiguration()
    {
    }
}