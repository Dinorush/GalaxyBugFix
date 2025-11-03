using BepInEx;
using BepInEx.Unity.IL2CPP;
using GalaxyBugFix.Patches;
using HarmonyLib;

namespace GalaxyBugFix
{
    [BepInPlugin("Dinorush." + MODNAME, MODNAME, "1.0.1")]
    internal sealed class EntryPoint : BasePlugin
    {
        public const string MODNAME = "GalaxyBugFix";

        public override void Load()
        {
            VolumeNativePatch.Init();
            new Harmony(MODNAME).PatchAll();
            Log.LogMessage("Loaded " + MODNAME);
        }
    }
}