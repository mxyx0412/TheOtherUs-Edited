using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles.Roles.Modifier;
public class Assassin
{
    public static List<PlayerControl> assassin = [];
    public static Color color = Palette.ImpostorRed;

    public static int remainingShotsEvilGuesser = 2;
    public static bool assassinMultipleShotsPerMeeting;
    public static bool assassinKillsThroughShield = true;
    public static bool evilGuesserCanGuessCrewmate = true;
    public static bool evilGuesserCanGuessSpy = true;
    public static bool guesserCantGuessSnitch;

    public static void clearAndReload()
    {
        assassin.Clear();
        remainingShotsEvilGuesser = Mathf.RoundToInt(CustomOptionHolder.modifierAssassinNumberOfShots.getFloat());
        assassinMultipleShotsPerMeeting = CustomOptionHolder.modifierAssassinMultipleShotsPerMeeting.getBool();
        assassinKillsThroughShield = CustomOptionHolder.modifierAssassinKillsThroughShield.getBool();
        evilGuesserCanGuessCrewmate = CustomOptionHolder.guesserEvilCanKillCrewmate.getBool();
        evilGuesserCanGuessSpy = CustomOptionHolder.guesserEvilCanKillSpy.getBool();
        guesserCantGuessSnitch = CustomOptionHolder.guesserCantGuessSnitchIfTaksDone.getBool();
    }
}