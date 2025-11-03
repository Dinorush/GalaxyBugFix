using CullingSystem;
using HarmonyLib;

namespace GalaxyBugFix.Patches
{
    [HarmonyPatch]
    internal static class PortalPatch
    {
        [HarmonyPatch(typeof(C_Portal), nameof(C_Portal.SetupEdges))]
        [HarmonyPostfix]
        private static void FixBottomPlane(C_Portal __instance)
        {
            var plane = __instance.m_portalPlanes[2];
            plane.m_Distance += 0.5f;
            __instance.m_portalPlanes[2] = plane;
        }
    }
}
