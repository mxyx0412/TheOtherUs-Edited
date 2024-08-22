using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles.Roles.Neutral;

public static class Akujo
{
    public static Color color = new Color32(142, 69, 147, byte.MaxValue);
    public static PlayerControl akujo;
    public static PlayerControl honmei;
    public static List<PlayerControl> keeps;
    public static PlayerControl currentTarget;
    public static DateTime startTime;

    public static float timeLimit = 1300f;
    public static bool knowsRoles = true;
    public static bool honmeiCannotFollowWin;
    public static bool honmeiOptimizeWin;
    public static int timeLeft;
    public static bool forceKeeps;
    public static int keepsLeft;
    public static int numKeeps;

    public static ResourceSprite honmeiSprite = new("AkujoHonmeiButton.png");
    public static ResourceSprite keepSprite = new("AkujoKeepButton.png");

    public static bool existing()
    {
        return honmei != null && !honmei.Data.Disconnected;
    }

    public static bool existingWithKiller()
    {
        return existing() && isKiller(honmei);
    }

    public static void breakLovers(PlayerControl lover)
    {
        if ((Lovers.lover1 != null && lover == Lovers.lover1) || (Lovers.lover2 != null && lover == Lovers.lover2))
        {
            PlayerControl otherLover = lover.getPartner();
            if (otherLover != null)
            {
                Lovers.clearAndReload();
                otherLover.MurderPlayer(otherLover, MurderResultFlags.Succeeded);
                GameHistory.overrideDeathReasonAndKiller(otherLover, DeadPlayer.CustomDeathReason.LoveStolen);
            }
        }
    }

    public static void clearAndReload()
    {
        akujo = null;
        honmei = null;
        keeps = new List<PlayerControl>();
        currentTarget = null;
        startTime = DateTime.UtcNow;
        timeLimit = CustomOptionHolder.akujoTimeLimit.getFloat();
        forceKeeps = CustomOptionHolder.akujoForceKeeps.getBool();
        knowsRoles = CustomOptionHolder.akujoKnowsRoles.getBool();
        honmeiCannotFollowWin = CustomOptionHolder.akujoHonmeiCannotFollowWin.getBool();
        honmeiOptimizeWin = CustomOptionHolder.akujoHonmeiOptimizeWin.getBool();
        timeLeft = (int)Math.Ceiling(timeLimit - (DateTime.UtcNow - startTime).TotalSeconds);
        numKeeps = Math.Min((int)CustomOptionHolder.akujoNumKeeps.getFloat(), PlayerControl.AllPlayerControls.Count - 2);
        keepsLeft = numKeeps;
    }
}
