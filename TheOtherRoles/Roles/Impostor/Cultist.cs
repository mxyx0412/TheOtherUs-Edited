using System.Collections.Generic;
using TheOtherRoles.Objects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Roles.Impostor;

public static class Cultist
{
    public static PlayerControl cultist;
    public static PlayerControl currentTarget;
    public static Color color = Palette.ImpostorRed;
    public static List<Arrow> localArrows = new();
    public static bool chatTarget = true;
    public static bool chatTarget2 = true;
    public static bool isCultistGame;

    public static bool needsFollower = true;

    //public static PlayerControl currentFollower;
    public static ResourceSprite buttonSprite = new("SidekickButton.png");

    public static PlayerControl getCultistPartner(this PlayerControl player)
    {
        if (player == null) return null;
        if (Cultist.cultist == player) return Follower.follower;
        if (Follower.follower == player) return Cultist.cultist;
        return null;
    }

    public static void clearAndReload()
    {
        if (localArrows != null)
            foreach (var arrow in localArrows)
                if (arrow?.arrow != null)
                    Object.Destroy(arrow.arrow);
        localArrows = new List<Arrow>();
        cultist = null;
        currentTarget = null;
        //currentFollower = null;
        needsFollower = true;
        chatTarget = true;
        chatTarget2 = true;
    }
}

public static class Follower
{
    public static PlayerControl follower;
    public static PlayerControl currentTarget;
    public static Color color = Palette.ImpostorRed;
    public static List<Arrow> localArrows = new();
    public static bool getsAssassin;
    public static bool chatTarget = true;
    public static bool chatTarget2 = true;

    public static void clearAndReload()
    {
        if (localArrows != null)
            foreach (var arrow in localArrows)
                if (arrow?.arrow != null)
                    Object.Destroy(arrow.arrow);
        localArrows = new List<Arrow>();
        follower = null;
        currentTarget = null;
        chatTarget = true;
        chatTarget2 = true;
        getsAssassin = CustomOptionHolder.modifierAssassinCultist.getBool();
    }
}
