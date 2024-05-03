﻿using System;
using System.Collections.Generic;
using TheOtherRoles.Objects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Roles.Crewmate;

public static class Prophet
{
    public static PlayerControl prophet;
    public static Color32 color = new Color32(255, 204, 127, byte.MaxValue);

    public static float cooldown = 25f;
    public static bool killCrewAsRed = false;
    public static bool benignNeutralAsRed = false;
    public static bool evilNeutralAsRed = true;
    public static bool canCallEmergency = false;
    public static int examineNum = 3;
    public static int examinesToBeRevealed = 1;
    public static int examinesLeft;
    public static bool revealProphet = true;
    public static bool isRevealed = false;
    public static List<Arrow> arrows = new List<Arrow>();

    public static Dictionary<PlayerControl, bool> examined = new Dictionary<PlayerControl, bool>();
    public static PlayerControl currentTarget;

    private static Sprite buttonSprite;
    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SeerButton.png", 115f);
        return buttonSprite;
    }

    public static bool IsKiller(PlayerControl p)
    {
        return Helpers.isKiller(p)
            || p.Data.Role.IsImpostor
            || (p == Sheriff.sheriff
            || p == Deputy.deputy
            || p == Veteren.veteren)
            && killCrewAsRed
            || Helpers.isEvil(p) && evilNeutralAsRed
            || !Helpers.isEvil(p) && benignNeutralAsRed
        ;
    }

    public static void clearAndReload()
    {
        prophet = null;
        currentTarget = null;
        isRevealed = false;
        examined = new Dictionary<PlayerControl, bool>();
        revealProphet = CustomOptionHolder.prophetIsRevealed.getBool();
        cooldown = CustomOptionHolder.prophetCooldown.getFloat();
        examineNum = Mathf.RoundToInt(CustomOptionHolder.prophetNumExamines.getFloat());
        killCrewAsRed = CustomOptionHolder.prophetKillCrewAsRed.getBool();
        benignNeutralAsRed = CustomOptionHolder.prophetBenignNeutralAsRed.getBool();
        evilNeutralAsRed = CustomOptionHolder.prophetEvilNeutralAsRed.getBool();
        canCallEmergency = CustomOptionHolder.prophetCanCallEmergency.getBool();
        examinesToBeRevealed = Math.Min(examineNum, Mathf.RoundToInt(CustomOptionHolder.prophetExaminesToBeRevealed.getFloat()));
        examinesLeft = examineNum;
        if (arrows != null)
        {
            foreach (Arrow arrow in arrows)
                if (arrow?.arrow != null)
                    Object.Destroy(arrow.arrow);
        }
        arrows = new List<Arrow>();
    }
}
