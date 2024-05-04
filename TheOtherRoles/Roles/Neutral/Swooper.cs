using TheOtherRoles.Objects;
using UnityEngine;

namespace TheOtherRoles.Roles.Neutral;

public static class Swooper
{
    public static PlayerControl swooper;
    public static PlayerControl currentTarget;
    public static float cooldown = 30f;
    public static bool isInvisable = false;
    public static Color color = new Color32(224, 197, 219, byte.MaxValue);
    public static float duration = 5f;
    public static float swoopCooldown = 30f;
    public static float swoopTimer = 0f;
    public static Sprite buttonSprite;
    public static bool hasImpVision = false;

    public static Sprite getSwoopButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Swoop.png", 115f);
        return buttonSprite;
    }

    public static Vector3 getSwooperSwoopVector()
    {
        return CustomButton.ButtonPositions.upperRowLeft; //brb
    }

    public static void clearAndReload()
    {
        swooper = null;
        isInvisable = false;
        cooldown = CustomOptionHolder.swooperKillCooldown.getFloat();
        swoopCooldown = CustomOptionHolder.swooperCooldown.getFloat();
        duration = CustomOptionHolder.swooperDuration.getFloat();
        hasImpVision = CustomOptionHolder.swooperHasImpVision.getBool();
    }
}
