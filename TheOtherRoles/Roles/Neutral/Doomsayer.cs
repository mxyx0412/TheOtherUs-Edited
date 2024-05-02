using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles.Roles.Neutral;

public static class Doomsayer
{
    public static PlayerControl doomsayer;

    public static Color color = new(0f, 1f, 0.5f, 1f);
    public static PlayerControl currentTarget;
    public static List<PlayerControl> playerTargetinformation = new();
    public static float cooldown = 30f;
    public static int formationNum = 1;
    public static bool hasMultipleShotsPerMeeting;
    public static bool showInfoInGhostChat = true;
    public static bool canGuessNeutral;
    public static bool canGuessImpostor;
    public static bool triggerDoomsayerrWin;
    public static bool canGuess = true;
    public static bool onlineTarger;
    public static float killToWin = 3;
    public static float killedToWin;
    public static bool CanShoot = true;


    private static Sprite buttonSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SeerButton.png", 115f);
        return buttonSprite;
    }

    public static void clearAndReload()
    {
        doomsayer = null;
        currentTarget = null;
        killedToWin = 0;
        canGuess = true;
        triggerDoomsayerrWin = false;
        cooldown = CustomOptionHolder.doomsayerCooldown.getFloat();
        hasMultipleShotsPerMeeting = CustomOptionHolder.doomsayerHasMultipleShotsPerMeeting.getBool();
        showInfoInGhostChat = CustomOptionHolder.doomsayerShowInfoInGhostChat.getBool();
        canGuessNeutral = CustomOptionHolder.doomsayerCanGuessNeutral.getBool();
        canGuessImpostor = CustomOptionHolder.doomsayerCanGuessImpostor.getBool();
        formationNum = CustomOptionHolder.doomsayerDormationNum.GetInt();
        killToWin = CustomOptionHolder.doomsayerKillToWin.getFloat();
        onlineTarger = CustomOptionHolder.doomsayerOnlineTarger.getBool();
    }
}
