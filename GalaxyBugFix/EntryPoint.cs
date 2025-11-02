using BepInEx;
using BepInEx.Unity.IL2CPP;
using GalaxyBugFix.Patches;

namespace GalaxyBugFix
{
    [BepInPlugin("Dinorush." + MODNAME, MODNAME, "1.0.0")]
    internal sealed class EntryPoint : BasePlugin
    {
        public const string MODNAME = "GalaxyBugFix";

        public override void Load()
        {
            VolumeNativePatch.Init();
            Log.LogMessage("Loaded " + MODNAME);
        }
    }
}