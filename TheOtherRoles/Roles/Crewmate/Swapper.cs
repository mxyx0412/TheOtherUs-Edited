﻿using UnityEngine;

namespace TheOtherRoles.Roles.Crewmate;

public static class Swapper
{
    public static PlayerControl swapper;
    public static Color color = new Color32(134, 55, 86, byte.MaxValue);
    public static bool canCallEmergency;
    public static bool canOnlySwapOthers;
    public static int charges;
    public static float rechargeTasksNumber;
    public static bool canFixSabotages;
    public static float rechargedTasks;

    public static byte playerId1 = byte.MaxValue;
    public static byte playerId2 = byte.MaxValue;

    public static ResourceSprite spriteCheck = new("SwapperCheck.png", 150f);

    public static void clearAndReload()
    {
        swapper = null;
        playerId1 = byte.MaxValue;
        playerId2 = byte.MaxValue;
        canCallEmergency = CustomOptionHolder.swapperCanCallEmergency.getBool();
        canOnlySwapOthers = CustomOptionHolder.swapperCanOnlySwapOthers.getBool();
        canFixSabotages = CustomOptionHolder.swapperCanFixSabotages.getBool();
        charges = Mathf.RoundToInt(CustomOptionHolder.swapperSwapsNumber.getFloat());
        rechargeTasksNumber = Mathf.RoundToInt(CustomOptionHolder.swapperRechargeTasksNumber.getFloat());
        rechargedTasks = Mathf.RoundToInt(CustomOptionHolder.swapperRechargeTasksNumber.getFloat());
    }
}
