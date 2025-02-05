﻿using UnityEngine;

namespace TheOtherRoles.Roles.Modifier;

public static class Cursed
{
    public static PlayerControl cursed;
    public static Color crewColor = new Color32(0, 247, 255, byte.MaxValue);
    public static Color impColor = Palette.ImpostorRed;
    public static Color color = crewColor;
    public static bool hideModifier;

    public static void clearAndReload()
    {
        cursed = null;
        hideModifier = CustomOptionHolder.modifierHideCursed.getBool();
    }
}
