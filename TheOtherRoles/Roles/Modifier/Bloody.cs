﻿using System.Collections.Generic;

namespace TheOtherRoles.Roles.Modifier;

public static class Bloody
{
    public static List<PlayerControl> bloody = new();
    public static Dictionary<byte, float> active = new();
    public static Dictionary<byte, byte> bloodyKillerMap = new();

    public static float duration = 5f;

    public static void clearAndReload()
    {
        bloody.Clear();
        active.Clear();
        bloodyKillerMap.Clear();
        duration = CustomOptionHolder.modifierBloodyDuration.getFloat();
    }
}
