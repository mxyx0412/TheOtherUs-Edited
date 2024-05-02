﻿using System.Collections.Generic;
using TheOtherRoles.Objects;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Roles.Crewmate;

/*
public static class Snitch
{
    public enum Mode
    {
        Chat = 0,
        Map = 1,
        ChatAndMap = 2
    }

    public enum Targets
    {
        EvilPlayers = 0,
        Killers = 1
    }

    public static PlayerControl snitch;
    public static Color color = new Color32(184, 251, 79, byte.MaxValue);

    public static Mode mode = Mode.Chat;
    public static Targets targets = Targets.EvilPlayers;
    public static int taskCountForReveal = 1;

    public static bool isRevealed;
    public static Dictionary<byte, byte> playerRoomMap = new();
    public static TextMeshPro text;
    public static bool needsUpdate = true;

    public static void clearAndReload()
    {
        taskCountForReveal = Mathf.RoundToInt(CustomOptionHolder.snitchLeftTasksForReveal.getFloat());
        snitch = null;
        isRevealed = false;
        playerRoomMap = new Dictionary<byte, byte>();
        if (text != null) Object.Destroy(text);
        text = null;
        needsUpdate = true;
        mode = (Mode)CustomOptionHolder.snitchMode.getSelection();
        targets = (Targets)CustomOptionHolder.snitchTargets.getSelection();
    }
}
*/

public static class Snitch
{
    public static PlayerControl snitch;
    public static Color color = new Color32(184, 251, 79, byte.MaxValue);

    public static List<Arrow> localArrows = new List<Arrow>();
    public static int taskCountForReveal = 1;
    public static bool seeInMeeting = false;
    public static bool canSeeRoles = false;
    public static bool teamNeutraUseDifferentArrowColor = true;
    public static bool needsUpdate = true;

    public enum includeNeutralTeam
    {
        NoIncNeutral = 0,
        KillNeutral = 1,
        EvilNeutral = 2,
        AllNeutral = 3
    }

    public static includeNeutralTeam Team = includeNeutralTeam.KillNeutral;
    public static TextMeshPro text;
    public static bool isRevealed;


    public static void clearAndReload()
    {
        if (localArrows != null)
        {
            foreach (Arrow arrow in localArrows)
                if (arrow?.arrow != null)
                    Object.Destroy(arrow.arrow);
        }
        localArrows = new List<Arrow>();
        taskCountForReveal = Mathf.RoundToInt(CustomOptionHolder.snitchLeftTasksForReveal.getFloat());
        seeInMeeting = CustomOptionHolder.snitchSeeMeeting.getBool();
        isRevealed = false;
        if (text != null) Object.Destroy(text);
        text = null;
        needsUpdate = true;

        canSeeRoles = CustomOptionHolder.snitchCanSeeRoles.getBool();
        Team = (includeNeutralTeam)CustomOptionHolder.snitchIncludeNeutralTeam.getSelection();
        teamNeutraUseDifferentArrowColor = CustomOptionHolder.snitchTeamNeutraUseDifferentArrowColor.getBool();
        snitch = null;
    }
}
