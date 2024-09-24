using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles.Roles.Impostor;

public static class Eraser
{
    public static PlayerControl eraser;
    public static Color color = Palette.ImpostorRed;

    public static List<byte> alreadyErased = new();

    public static List<PlayerControl> futureErased = new();
    public static PlayerControl currentTarget;
    public static float cooldown = 30f;
    public static bool canEraseAnyone;
    public static bool canEraseGuess;

    public static ResourceSprite buttonSprite = new("EraserButton.png");

    public static void clearAndReload()
    {
        eraser = null;
        futureErased.Clear();
        currentTarget = null;
        cooldown = CustomOptionHolder.eraserCooldown.getFloat();
        canEraseAnyone = CustomOptionHolder.eraserCanEraseAnyone.getBool();
        canEraseGuess = CustomOptionHolder.erasercanEraseGuess.getBool();
        alreadyErased.Clear();
    }
}
