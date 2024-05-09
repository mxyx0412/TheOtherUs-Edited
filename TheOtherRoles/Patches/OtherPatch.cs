using TheOtherRoles.Modules;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.Initialize))]
    public static class HatManager_Initialize
    {
        public static void Postfix(HatManager __instance)
        {
            CosmeticsUnlocker.unlockCosmetics(__instance);
        }
    }


    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPriority(Priority.Last)]
    public class FlashUnFlash
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(HudManager __instance)
        {
            foreach (var role in RoleInfo.getRoleInfoForPlayer(Grenadier.grenadier))
            {
                if (Grenadier.Flashed)
                    Grenadier.Flash();
                else if (Grenadier.Enabled) Grenadier.UnFlash();
            }
        }
    }
}
