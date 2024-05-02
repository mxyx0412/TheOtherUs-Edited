using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Objects;
using TheOtherRoles.Objects.Map;
using TheOtherRoles.Roles.Crewmate;
using TheOtherRoles.Roles.Impostor;
using TheOtherRoles.Roles.Modifier;
using TheOtherRoles.Roles.Neutral;
using TheOtherRoles.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace TheOtherRoles;

[HarmonyPatch]
public static partial class TheOtherRoles
{
    public static readonly Random rnd = new((int)DateTime.Now.Ticks);

    public static void clearAndReloadRoles()
    {
        // MapOptions
        ResetButtonCooldown.clearAndReload();
        ElectricPatch.Reset();

        Jester.clearAndReload();
        Mayor.clearAndReload();
        Portalmaker.clearAndReload();
        Poucher.clearAndReload();
        Mimic.clearAndReload();
        Engineer.clearAndReload();
        Sheriff.clearAndReload();
        Cursed.clearAndReload();
        Deputy.clearAndReload();
        Amnisiac.clearAndReload();
        Lighter.clearAndReload();
        Godfather.clearAndReload();
        Mafioso.clearAndReload();
        Janitor.clearAndReload();
        Detective.clearAndReload();
        Werewolf.clearAndReload();
        TimeMaster.clearAndReload();
        BodyGuard.clearAndReload();
        Veteren.clearAndReload();
        Medic.clearAndReload();
        PrivateInvestigator.clearAndReload();
        Shifter.clearAndReload();
        Swapper.clearAndReload();
        Lovers.clearAndReload();
        Seer.clearAndReload();
        Morphling.clearAndReload();
        Camouflager.clearAndReload();
        Cultist.clearAndReload();
        Hacker.clearAndReload();
        Tracker.clearAndReload();
        Vampire.clearAndReload();
        Snitch.clearAndReload();
        Jackal.clearAndReload();
        Sidekick.clearAndReload();
        Follower.clearAndReload();
        Eraser.clearAndReload();
        Spy.clearAndReload();
        Trickster.clearAndReload();
        Cleaner.clearAndReload();
        Undertaker.clearAndReload();
        Warlock.clearAndReload();
        SecurityGuard.clearAndReload();
        Arsonist.clearAndReload();
        BountyHunter.clearAndReload();
        Vulture.clearAndReload();
        Medium.clearAndReload();
        Bomber.clearAndReload();
        Lawyer.clearAndReload();
        Pursuer.clearAndReload();
        Witch.clearAndReload();
        Jumper.clearAndReload();
        Prophet.clearAndReload();
        Escapist.clearAndReload();
        Ninja.clearAndReload();
        Blackmailer.clearAndReload();
        Thief.clearAndReload();
        Miner.clearAndReload();
        Trapper.clearAndReload();
        Terrorist.clearAndReload();
        Juggernaut.clearAndReload();
        Doomsayer.clearAndReload();
        //Guesser.clearAndReload();
        Swooper.clearAndReload();
        Akujo.clearAndReload();

        // Modifier
        Bait.clearAndReload();
        Bloody.clearAndReload();
        AntiTeleport.clearAndReload();
        Tiebreaker.clearAndReload();
        Sunglasses.clearAndReload();
        Torch.clearAndReload();
        Flash.clearAndReload();
        Blind.clearAndReload();
        Watcher.clearAndReload();
        Radar.clearAndReload();
        Tunneler.clearAndReload();
        Multitasker.clearAndReload();
        Disperser.clearAndReload();
        Mini.clearAndReload();
        Indomitable.clearAndReload();
        Slueth.clearAndReload();
        Vip.clearAndReload();
        Invert.clearAndReload();
        Chameleon.clearAndReload();
        ButtonBarry.clearAndReload();
        LastImpostor.clearAndReload();

        // Gamemodes
        HandleGuesser.clearAndReload();
        HideNSeek.clearAndReload();
        PropHunt.clearAndReload();
    }

    public static class PreventTaskEnd
    {
        public static bool Enable = false;
        public static void clearAndReload()
        {
            Enable = CustomOptionHolder.preventTaskEnd.getBool();
        }
    }

    public static class ResetButtonCooldown
    {
        public static float killCooldown = 20f;
        public static void clearAndReload()
        {
            killCooldown = CustomOptionHolder.resteButtonCooldown.getFloat();
        }
    }

    public static class Cultist
    {
        public static PlayerControl cultist;
        public static PlayerControl currentTarget;
        public static Color color = Palette.ImpostorRed;
        public static List<Arrow> localArrows = new();
        public static bool chatTarget = true;
        public static bool chatTarget2 = true;
        public static bool isCultistGame = false;

        public static bool needsFollower = true;

        //      public static PlayerControl currentFollower;
        public static Sprite buttonSprite;


        public static Sprite getSidekickButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SidekickButton.png", 115f);
            return buttonSprite;
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
            //            currentFollower = null;
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

    public static class Crew
    {
        public static PlayerControl crew;
        public static Color color = Palette.White;

        public static void clearAndReload()
        {
            crew = null;
        }
    }

    public static class Godfather
    {
        public static PlayerControl godfather;
        public static Color color = Palette.ImpostorRed;

        public static void clearAndReload()
        {
            godfather = null;
        }
    }

    public static class Mafioso
    {
        public static PlayerControl mafioso;
        public static Color color = Palette.ImpostorRed;

        public static void clearAndReload()
        {
            mafioso = null;
        }
    }


    public static class Janitor
    {
        public static PlayerControl janitor;
        public static Color color = Palette.ImpostorRed;

        public static float cooldown = 30f;

        private static Sprite buttonSprite;

        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CleanButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload()
        {
            janitor = null;
            cooldown = CustomOptionHolder.janitorCooldown.getFloat();
        }
    }
}

public static class Lovers
{
    public static PlayerControl lover1;
    public static PlayerControl lover2;
    public static Color color = new Color32(232, 57, 185, byte.MaxValue);

    public static bool bothDie = true;

    public static bool enableChat = true;

    // Lovers save if next to be exiled is a lover, because RPC of ending game comes before RPC of exiled
    public static bool notAckedExiledIsLover;

    public static bool existing()
    {
        return lover1 != null && lover2 != null && !lover1.Data.Disconnected && !lover2.Data.Disconnected;
    }

    public static bool existingAndAlive()
    {
        return existing() && !lover1.Data.IsDead && !lover2.Data.IsDead &&
               !notAckedExiledIsLover; // ADD NOT ACKED IS LOVER
    }

    public static PlayerControl otherLover(PlayerControl oneLover)
    {
        if (!existingAndAlive()) return null;
        if (oneLover == lover1) return lover2;
        if (oneLover == lover2) return lover1;
        return null;
    }

    public static bool existingWithKiller()
    {
        return existing() && (lover1 == Jackal.jackal || lover2 == Jackal.jackal
                           || lover1 == Sidekick.sidekick || lover2 == Sidekick.sidekick
                           || lover1 == Werewolf.werewolf || lover2 == Werewolf.werewolf
                           || lover1 == Juggernaut.juggernaut || lover2 == Juggernaut.juggernaut
                           || lover1.Data.Role.IsImpostor || lover2.Data.Role.IsImpostor);
    }

    public static bool hasAliveKillingLover(this PlayerControl player)
    {
        if (!existingAndAlive() || !existingWithKiller())
            return false;
        return player != null && (player == lover1 || player == lover2);
    }

    public static void clearAndReload()
    {
        lover1 = null;
        lover2 = null;
        notAckedExiledIsLover = false;
        bothDie = CustomOptionHolder.modifierLoverBothDie.getBool();
        enableChat = CustomOptionHolder.modifierLoverEnableChat.getBool();
    }

    public static PlayerControl getPartner(this PlayerControl player)
    {
        if (player == null)
            return null;
        if (lover1 == player)
            return lover2;
        if (lover2 == player)
            return lover1;
        return null;
    }
}

public static class Jackal
{
    public static PlayerControl jackal;

    public static Color color = new Color32(0, 180, 235, byte.MaxValue);

    //public static Color color = new Color32(224, 197, 219, byte.MaxValue);
    public static PlayerControl fakeSidekick;
    public static PlayerControl currentTarget;
    public static List<PlayerControl> formerJackals = new();

    public static float cooldown = 30f;
    public static float createSidekickCooldown = 30f;
    public static bool canUseVents = true;
    public static bool canCreateSidekick = true;
    public static Sprite buttonSprite;
    public static Sprite buttonSprite2;
    public static bool jackalPromotedFromSidekickCanCreateSidekick = true;
    public static bool canCreateSidekickFromImpostor = true;
    public static bool hasImpostorVision;
    public static bool CanImpostorFindSidekick;
    public static bool killFakeImpostor;
    public static bool wasTeamRed;
    public static bool canSabotage;
    public static bool wasImpostor;
    public static bool wasSpy;


    public static Sprite getSidekickButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SidekickButton.png", 115f);
        return buttonSprite;
    }
    public static void removeCurrentJackal()
    {
        if (!formerJackals.Any(x => x.PlayerId == jackal.PlayerId)) formerJackals.Add(jackal);
        jackal = null;
        currentTarget = null;
        fakeSidekick = null;
        cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
        createSidekickCooldown = CustomOptionHolder.jackalCreateSidekickCooldown.getFloat();
    }

    public static void clearAndReload()
    {
        jackal = null;
        currentTarget = null;
        fakeSidekick = null;
        //isInvisable = false;
        cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
        createSidekickCooldown = CustomOptionHolder.jackalCreateSidekickCooldown.getFloat();
        canUseVents = CustomOptionHolder.jackalCanUseVents.getBool();
        canSabotage = CustomOptionHolder.jackalCanUseSabo.getBool();
        CanImpostorFindSidekick = CustomOptionHolder.jackalCanImpostorFindSidekick.getBool();
        canCreateSidekick = CustomOptionHolder.jackalCanCreateSidekick.getBool();
        jackalPromotedFromSidekickCanCreateSidekick = CustomOptionHolder.jackalPromotedFromSidekickCanCreateSidekick.getBool();
        canCreateSidekickFromImpostor = CustomOptionHolder.jackalCanCreateSidekickFromImpostor.getBool();
        killFakeImpostor = CustomOptionHolder.jackalKillFakeImpostor.getBool();
        formerJackals.Clear();
        hasImpostorVision = CustomOptionHolder.jackalAndSidekickHaveImpostorVision.getBool();
        wasTeamRed = wasImpostor = wasSpy = false;
    }
}

public static class Sidekick
{
    public static PlayerControl sidekick;
    public static Color color = new Color32(0, 180, 235, byte.MaxValue);

    public static PlayerControl currentTarget;

    public static bool wasTeamRed;
    public static bool wasImpostor;
    public static bool wasSpy;

    public static float cooldown = 30f;
    public static bool canUseVents = true;
    public static bool canKill = true;
    public static bool promotesToJackal = true;
    public static bool hasImpostorVision;

    public static void clearAndReload()
    {
        sidekick = null;
        currentTarget = null;
        cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
        canUseVents = CustomOptionHolder.sidekickCanUseVents.getBool();
        canKill = CustomOptionHolder.sidekickCanKill.getBool();
        promotesToJackal = CustomOptionHolder.sidekickPromotesToJackal.getBool();
        hasImpostorVision = CustomOptionHolder.jackalAndSidekickHaveImpostorVision.getBool();
        wasTeamRed = wasImpostor = wasSpy = false;
    }
}

public static class Guesser
{
    public static PlayerControl niceGuesser;

    public static List<PlayerControl> evilGuesser = new();
    public static Color color = new Color32(255, 255, 0, byte.MaxValue);

    public static int remainingShotsEvilGuesser = 2;
    public static int remainingShotsNiceGuesser = 2;
    public static bool hasMultipleShotsPerMeeting;
    public static bool assassinMultipleShotsPerMeeting;
    public static bool showInfoInGhostChat = true;
    public static bool killsThroughShield = true;
    public static bool assassinKillsThroughShield = true;
    public static bool evilGuesserCanGuessSpy = true;
    public static bool guesserCantGuessSnitch;
    public static bool evilGuesserCanGuessCrewmate = true;


    public static bool isGuesser(byte playerId)
    {
        if (evilGuesser.Any(item => item.PlayerId == playerId && evilGuesser != null)) return true;

        return niceGuesser != null && niceGuesser.PlayerId == playerId;
    }

    public static void clear(byte playerId)
    {
        if (niceGuesser != null && niceGuesser.PlayerId == playerId) niceGuesser = null;
        foreach (var item in evilGuesser.Where(item => item.PlayerId == playerId && evilGuesser != null))
            evilGuesser = null;
    }

    public static int remainingShots(byte playerId, bool shoot = false)
    {
        var result = remainingShotsEvilGuesser;
        if (niceGuesser != null && niceGuesser.PlayerId == playerId)
        {
            result = remainingShotsNiceGuesser;
            if (shoot) remainingShotsNiceGuesser = Mathf.Max(0, remainingShotsNiceGuesser - 1);
        }
        else if (shoot)
        {
            remainingShotsEvilGuesser = Mathf.Max(0, remainingShotsEvilGuesser - 1);
        }
        return result;
    }

    public static void clearAndReload()
    {
        niceGuesser = null;
        evilGuesser = new List<PlayerControl>();

        guesserCantGuessSnitch = CustomOptionHolder.guesserCantGuessSnitchIfTaksDone.getBool();
        remainingShotsEvilGuesser = Mathf.RoundToInt(CustomOptionHolder.modifierAssassinNumberOfShots.getFloat() + 1);
        remainingShotsNiceGuesser = Mathf.RoundToInt(CustomOptionHolder.guesserNumberOfShots.getFloat() + 1);
        hasMultipleShotsPerMeeting = CustomOptionHolder.guesserHasMultipleShotsPerMeeting.getBool();
        assassinMultipleShotsPerMeeting = CustomOptionHolder.modifierAssassinMultipleShotsPerMeeting.getBool();
        showInfoInGhostChat = CustomOptionHolder.guesserShowInfoInGhostChat.getBool();
        killsThroughShield = CustomOptionHolder.guesserKillsThroughShield.getBool();
        assassinKillsThroughShield = CustomOptionHolder.modifierAssassinKillsThroughShield.getBool();
        evilGuesserCanGuessSpy = CustomOptionHolder.guesserEvilCanKillSpy.getBool();
        evilGuesserCanGuessCrewmate = CustomOptionHolder.guesserEvilCanKillCrewmate.getBool();
    }
}

public static class Lawyer
{
    public static PlayerControl lawyer;
    public static PlayerControl target;
    public static Color color = new Color32(134, 153, 25, byte.MaxValue);
    public static Sprite targetSprite;
    public static bool triggerProsecutorWin;
    public static bool isProsecutor;
    public static bool canCallEmergency = true;
    public static bool targetKnows;

    public static float vision = 1f;
    public static bool lawyerKnowsRole;
    public static bool targetCanBeJester;
    public static bool targetWasGuessed;

    public static Sprite getTargetSprite()
    {
        if (targetSprite) return targetSprite;
        targetSprite = Helpers.loadSpriteFromResources("", 150f);
        return targetSprite;
    }

    public static void clearAndReload(bool clearTarget = true)
    {
        lawyer = null;
        if (clearTarget)
        {
            target = null;
            targetWasGuessed = false;
        }

        isProsecutor = false;
        triggerProsecutorWin = false;
        vision = CustomOptionHolder.lawyerVision.getFloat();
        targetKnows = CustomOptionHolder.lawyerTargetKnows.getBool();
        lawyerKnowsRole = CustomOptionHolder.lawyerKnowsRole.getBool();
        targetCanBeJester = CustomOptionHolder.lawyerTargetCanBeJester.getBool();
        canCallEmergency = CustomOptionHolder.jesterCanCallEmergency.getBool();
    }
}

public static class Pursuer
{
    public static PlayerControl pursuer;
    public static PlayerControl target;
    public static Color color = Lawyer.color;
    public static List<PlayerControl> blankedList = new();
    public static int blanks;
    public static Sprite blank;
    public static bool notAckedExiled;

    public static float cooldown = 30f;
    public static int blanksNumber = 5;

    public static Sprite getTargetSprite()
    {
        if (blank) return blank;
        blank = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PursuerButton.png", 115f);
        return blank;
    }

    public static void clearAndReload()
    {
        pursuer = null;
        target = null;
        blankedList = new List<PlayerControl>();
        blanks = 0;
        notAckedExiled = false;

        cooldown = CustomOptionHolder.pursuerCooldown.getFloat();
        blanksNumber = Mathf.RoundToInt(CustomOptionHolder.pursuerBlanksNumber.getFloat());
    }
}
