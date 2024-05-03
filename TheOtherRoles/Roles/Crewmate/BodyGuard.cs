﻿using UnityEngine;

namespace TheOtherRoles.Roles.Crewmate;

public static class BodyGuard
{
    public static PlayerControl bodyguard;
    public static PlayerControl guarded;
    public static Color color = new Color32(145, 102, 64, byte.MaxValue);
    public static bool reset = true;
    public static bool usedGuard;
    public static bool guardFlash;
    private static Sprite guardButtonSprite;
    public static PlayerControl currentTarget;

    public static void resetGuarded()
    {
        currentTarget = guarded = null;
        usedGuard = false;
    }


    public static Sprite getGuardButtonSprite()
    {
        if (guardButtonSprite) return guardButtonSprite;
        guardButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Shield.png", 115f);
        return guardButtonSprite;
    }

    public static void clearAndReload()
    {
        bodyguard = null;
        guardFlash = CustomOptionHolder.bodyGuardFlash.getBool();
        reset = CustomOptionHolder.bodyGuardResetTargetAfterMeeting.getBool();
        guarded = null;
        usedGuard = false;
    }
}
