using UnityEngine;

namespace TheOtherRoles.Roles.Modifier;

public class Aftermath
{
    public static PlayerControl aftermath;
    public static Color color = new Color32(165, 255, 165, byte.MaxValue);

    public static void clearAndReload()
    {
        aftermath = null;
    }
}
