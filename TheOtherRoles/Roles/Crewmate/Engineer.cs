using UnityEngine;

namespace TheOtherRoles.Roles.Crewmate;

public class Engineer
{
    public static PlayerControl engineer;
    public static Color color = new Color32(0, 40, 245, byte.MaxValue);
    public static ResourceSprite buttonSprite = new("RepairButton.png");

    public static bool resetFixAfterMeeting;

    //public static bool expertRepairs = false;
    public static bool remoteFix = true;
    public static int remainingFixes = 1;
    public static bool highlightForImpostors = true;
    public static bool highlightForTeamJackal = true;

    public static void resetFixes()
    {
        remainingFixes = Mathf.RoundToInt(CustomOptionHolder.engineerNumberOfFixes.getFloat());
    }

    public static void clearAndReload()
    {
        engineer = null;
        remoteFix = CustomOptionHolder.engineerRemoteFix.getBool();
        //expertRepairs = CustomOptionHolder.engineerExpertRepairs.getBool();
        resetFixAfterMeeting = CustomOptionHolder.engineerResetFixAfterMeeting.getBool();
        remainingFixes = Mathf.RoundToInt(CustomOptionHolder.engineerNumberOfFixes.getFloat());
        highlightForImpostors = CustomOptionHolder.engineerHighlightForImpostors.getBool();
        highlightForTeamJackal = CustomOptionHolder.engineerHighlightForTeamJackal.getBool();
    }
}

