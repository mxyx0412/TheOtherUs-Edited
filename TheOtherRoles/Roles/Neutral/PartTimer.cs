using UnityEngine;

namespace TheOtherRoles.Roles.Neutral;

public class PartTimer
{
    public static PlayerControl partTimer;
    public static Color color = new Color32(0, 255, 0, byte.MaxValue);
    public static PlayerControl currentTarget;
    public static PlayerControl target;

    public static bool canSetTarget = true;

    public static float cooldown;
    public static int deathTurn;
    public static bool checkTargetRole;

    public static ResourceSprite buttonSprite = new("PartTimerButton.png");

    public static void clearAndReload()
    {
        partTimer = null;
        currentTarget = null;
        target = null;
        canSetTarget = true;
        cooldown = CustomOptionHolder.partTimerCooldown.getFloat();
        deathTurn = CustomOptionHolder.partTimerDeathTurn.GetInt();
        checkTargetRole = CustomOptionHolder.partTimerIsCheckTargetRole.getBool();
    }
}
