using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using Reactor.Utilities.Extensions;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using TheOtherRoles.Roles.Crewmate;
using TheOtherRoles.Roles.Impostor;
using TheOtherRoles.Roles.Modifier;
using TheOtherRoles.Roles.Neutral;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Helper;

public enum MurderAttemptResult
{
    ReverseKill,
    PerformKill,
    SuppressKill,
    BlankKill,
    BodyGuardKill,
    DelayVampireKill
}

public enum SabatageTypes
{
    Comms,
    O2,
    Reactor,
    OxyMask,
    Lights,
    None
}

public enum CustomGamemodes
{
    Classic,
    Guesser,
    HideNSeek,
    PropHunt
}

public enum LogLevel
{
    Message,
    Error,
    Warning,
    Fatal,
    Info,
    Debug
}

public static class Helpers
{
    public static Dictionary<string, Sprite> CachedSprites = new();
    public static Sprite teamCultistChat = null;
    public static Sprite teamLoverChat = null;

    public static bool zoomOutStatus;

    /*
            public static Sprite getTeamCultistChatButtonSprite()
        {
            if (teamCultistChat != null)
            {
                return teamCultistChat;
            }
            teamCultistChat = loadSpriteFromResources("TheOtherRoles.Resources.TeamJackalChat.png", 115f);
            return teamCultistChat;
        }

                public static Sprite getLoversChatButtonSprite() {
            if (teamLoverChat != null)
            {
                return teamLoverChat;
            }
            teamLoverChat = loadSpriteFromResources("TheOtherRoles.Resources.LoversChat.png", 150f);
            return teamLoverChat;
        }
        */

    public static bool gameStarted => AmongUsClient.Instance != null &&
                                      AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started; //new

    public static bool ShowButtons =>
        !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
        !MeetingHud.Instance &&
        !ExileController.Instance;

    public static bool canAlwaysBeGuessed(RoleId roleId)
    {
        var guessable = false;
        if (roleId == RoleId.Cursed) guessable = true;
        return guessable;
    }

    public static bool flipBitwise(bool bit)
    {
        if (!bit) return true;
        return false;
    }

    public static void enableCursor(bool initalSetCursor)
    {
        if (initalSetCursor)
        {
            var sprite = loadSpriteFromResources("TheOtherRoles.Resources.Cursor.png", 115f);
            Cursor.SetCursor(sprite.texture, Vector2.zero, CursorMode.Auto);
            return;
        }

        if (TheOtherRolesPlugin.ToggleCursor.Value)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            var sprite = loadSpriteFromResources("TheOtherRoles.Resources.Cursor.png", 115f);
            Cursor.SetCursor(sprite.texture, Vector2.zero, CursorMode.Auto);
        }
    }

    public static PlayerControl getCultistPartner(this PlayerControl player)
    {
        if (player == null) return null;
        if (Cultist.cultist == player) return Follower.follower;
        if (Follower.follower == player) return Cultist.cultist;
        return null;
    }

    public static bool roleCanSabotage(this PlayerControl player)
    {
        var roleCouldUse = false;
        if (Jackal.canSabotage)
            if (player == Jackal.jackal || player == Sidekick.sidekick || Jackal.formerJackals.Contains(player))
                roleCouldUse = true;
        if (player.Data?.Role != null && player.Data.Role.IsImpostor)
            roleCouldUse = true;
        return roleCouldUse;
    }


    public static SabatageTypes getActiveSabo()
    {
        foreach (var task in CachedPlayer.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
            if (task.TaskType == TaskTypes.FixLights)
                return SabatageTypes.Lights;
            else if (task.TaskType == TaskTypes.RestoreOxy)
                return SabatageTypes.O2;
            else if (task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.StopCharles ||
                     task.TaskType == TaskTypes.StopCharles)
                return SabatageTypes.Reactor;
            else if (task.TaskType == TaskTypes.FixComms)
                return SabatageTypes.Comms;
            else if (SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask)
                return SabatageTypes.OxyMask;
        return SabatageTypes.None;
    }

    public static bool isLightsActive()
    {
        return getActiveSabo() == SabatageTypes.Lights;
    }

    public static bool isCommsActive()
    {
        return getActiveSabo() == SabatageTypes.Comms;
    }


    public static bool isCamoComms()
    {
        return isCommsActive() && MapOptions.camoComms;
    }

    public static bool isActiveCamoComms()
    {
        return isCamoComms() && Camouflager.camoComms;
    }

    public static bool wasActiveCamoComms()
    {
        return !isCamoComms() && Camouflager.camoComms;
    }

    public static void camoReset()
    {
        Camouflager.resetCamouflage();
        if (Morphling.morphTimer > 0f && Morphling.morphling != null && Morphling.morphTarget != null)
        {
            var target = Morphling.morphTarget;
            Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId,
                target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId,
                target.Data.DefaultOutfit.PetId);
        }
    }

    public static IEnumerator BlackmailShhh()
    {
        //Helpers.showFlash(new Color32(49, 28, 69, byte.MinValue), 3f, "Blackmail", false, 0.75f);
        yield return HudManager.Instance.CoFadeFullScreen(Color.clear, new Color(0f, 0f, 0f, 0.98f));
        var TempPosition = HudManager.Instance.shhhEmblem.transform.localPosition;
        var TempDuration = HudManager.Instance.shhhEmblem.HoldDuration;
        HudManager.Instance.shhhEmblem.transform.localPosition = new Vector3(
            HudManager.Instance.shhhEmblem.transform.localPosition.x,
            HudManager.Instance.shhhEmblem.transform.localPosition.y,
            HudManager.Instance.FullScreen.transform.position.z + 1f);
        HudManager.Instance.shhhEmblem.TextImage.text = getString("BlackmailShhhText");
        HudManager.Instance.shhhEmblem.HoldDuration = 3f;
        yield return HudManager.Instance.ShowEmblem(true);
        HudManager.Instance.shhhEmblem.transform.localPosition = TempPosition;
        HudManager.Instance.shhhEmblem.HoldDuration = TempDuration;
        yield return HudManager.Instance.CoFadeFullScreen(new Color(0f, 0f, 0f, 0.98f), Color.clear);
        yield return null;
    }

    public static int getAvailableId()
    {
        var id = 0;
        while (true)
        {
            if (ShipStatus.Instance.AllVents.All(v => v.Id != id)) return id;
            id++;
        }
    }

    public static void turnToCrewmate(PlayerControl player)
    {
        var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
            (byte)CustomRPC.TurnToCrewmate, SendOption.Reliable);
        writer.Write(player.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.turnToCrewmate(player.PlayerId);
        foreach (var player2 in PlayerControl.AllPlayerControls)
            if (player2.Data.Role.IsImpostor && CachedPlayer.LocalPlayer.PlayerControl.Data.Role.IsImpostor)
                player.cosmetics.nameText.color = Palette.White;
    }

    public static void turnToCrewmate(List<PlayerControl> players, PlayerControl player)
    {
        foreach (var p in players)
        {
            if (p == player) continue;
            turnToCrewmate(p);
        }
    }

    public static void turnToImpostorRPC(PlayerControl player)
    {
        var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
            (byte)CustomRPC.TurnToImpostor, SendOption.Reliable);
        writer.Write(player.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.turnToImpostor(player.PlayerId);
    }

    public static void turnToImpostor(PlayerControl player)
    {
        player.Data.Role.TeamType = RoleTeamTypes.Impostor;
        RoleManager.Instance.SetRole(player, RoleTypes.Impostor);
        player.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);

        System.Console.WriteLine("PROOF I AM IMP VANILLA ROLE: " + player.Data.Role.IsImpostor);

        foreach (var player2 in PlayerControl.AllPlayerControls)
            if (player2.Data.Role.IsImpostor && CachedPlayer.LocalPlayer.PlayerControl.Data.Role.IsImpostor)
                player.cosmetics.nameText.color = Palette.ImpostorRed;
    }

    public static void showTargetNameOnButton(PlayerControl target, CustomButton button, string defaultText)
    {
        if (CustomOptionHolder.showButtonTarget.getBool())
        {
            // Should the button show the target name option
            var text = "";
            if (Camouflager.camouflageTimer >= 0.1f || isCamoComms())
                text = defaultText; // set text to default if camo is on
            else if (isLightsActive()) text = defaultText; // set to default if lights are out
            else if (Trickster.trickster != null && Trickster.lightsOutTimer > 0f)
                text = defaultText; // set to default if trickster ability is active
            else if (Morphling.morphling != null && Morphling.morphTarget != null && target == Morphling.morphling &&
                     Morphling.morphTimer > 0) text = Morphling.morphTarget.Data.PlayerName; // set to morphed player
            else if (target == Swooper.swooper && Swooper.isInvisable) text = defaultText;
            //else if (target == PhantomRole.phantomRole) text = defaultText;
            else if (target == null) text = defaultText; // Set text to defaultText if no target
            else text = target.Data.PlayerName; // Set text to playername
            showTargetNameOnButtonExplicit(null, button, text);
        }
    }


    public static void showTargetNameOnButtonExplicit(PlayerControl target, CustomButton button, string defaultText)
    {
        var text = defaultText;
        if (target == null) text = defaultText; // Set text to defaultText if no target
        else text = target.Data.PlayerName; // Set text to playername
        button.actionButton.OverrideText(text);
        button.showButtonText = true;
    }

    public static bool isInvisible(PlayerControl player)
    {
        if (Swooper.swooper != null && Swooper.swooper == player && Swooper.isInvisable) return true;
        return false;
    }

    public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit, bool cache = true)
    {
        try
        {
            if (cache && CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            var texture = loadTextureFromResources(path);
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
            if (cache) sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            if (!cache) return sprite;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            System.Console.WriteLine("Error loading sprite from path: " + path);
        }

        return null;
    }

    public static unsafe Texture2D loadTextureFromResources(string path)
    {
        try
        {
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(path);
            var length = stream.Length;
            var byteTexture = new Il2CppStructArray<byte>(length);
            stream.Read(new Span<byte>(IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(), (int)length));
            if (path.Contains("HorseHats")) byteTexture = new Il2CppStructArray<byte>(byteTexture.Reverse().ToArray());
            ImageConversion.LoadImage(texture, byteTexture, false);
            return texture;
        }
        catch
        {
            //System.Console.WriteLine("Error loading texture from resources: " + path);
        }

        return null;
    }

    public static Texture2D loadTextureFromDisk(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                var byteTexture = Il2CppSystem.IO.File.ReadAllBytes(path);
                ImageConversion.LoadImage(texture, byteTexture, false);
                return texture;
            }
        }
        catch
        {
            Error("Error loading texture from disk: " + path);
        }

        return null;
    }

    public static AudioClip loadAudioClipFromResources(string path, string clipName = "UNNAMED_TOR_AUDIO_CLIP")
    {
        // must be "raw (headerless) 2-channel signed 32 bit pcm (le)" (can e.g. use Audacity® to export)
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(path);
            var byteAudio = new byte[stream.Length];
            _ = stream.Read(byteAudio, 0, (int)stream.Length);
            var samples = new float[byteAudio.Length / 4]; // 4 bytes per sample
            int offset;
            for (var i = 0; i < samples.Length; i++)
            {
                offset = i * 4;
                samples[i] = (float)BitConverter.ToInt32(byteAudio, offset) / int.MaxValue;
            }

            var channels = 2;
            var sampleRate = 48000;
            var audioClip = AudioClip.Create(clipName, samples.Length / 2, channels, sampleRate, false);
            audioClip.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            audioClip.SetData(samples, 0);
            return audioClip;
        }
        catch
        {
            Error("Error loading AudioClip from resources: " + path);
        }

        return null;

        /* Usage example:
        AudioClip exampleClip = Helpers.loadAudioClipFromResources("TheOtherRoles.Resources.exampleClip.raw");
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(exampleClip, false, 0.8f);
        */
    }

    public static string readTextFromResources(string path)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(path);
        var textStreamReader = new StreamReader(stream);
        return textStreamReader.ReadToEnd();
    }

    public static string readTextFromFile(string path)
    {
        Stream stream = File.OpenRead(path);
        var textStreamReader = new StreamReader(stream);
        return textStreamReader.ReadToEnd();
    }

    public static List<RoleInfo> allRoleInfos()
    {
        var allRoleInfo = new List<RoleInfo>();
        foreach (var player in RoleInfo.allRoleInfos)
        {
            if (player.isModifier) continue;
            allRoleInfo.Add(player);
        }

        return allRoleInfo;
    }

    public static List<RoleInfo> onlineRoleInfos()
    {
        var roleInfos = new List<RoleInfo>();
        roleInfos.AddRange(CachedPlayer.AllPlayers.Select(
            n => RoleInfo.getRoleInfoForPlayer(n, false)).SelectMany(n => n));
        return roleInfos;
    }

    public static PlayerControl playerById(byte id)
    {
        foreach (PlayerControl player in CachedPlayer.AllPlayers)
            if (player.PlayerId == id)
                return player;
        return null;
    }

    public static Dictionary<byte, PlayerControl> allPlayersById()
    {
        var res = new Dictionary<byte, PlayerControl>();
        foreach (PlayerControl player in CachedPlayer.AllPlayers)
            res.Add(player.PlayerId, player);
        return res;
    }

    public static void handleVampireBiteOnBodyReport()
    {
        // Murder the bitten player and reset bitten (regardless whether the kill was successful or not)
        checkMurderAttemptAndKill(Vampire.vampire, Vampire.bitten, true, false);
        var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
            (byte)CustomRPC.VampireSetBitten, SendOption.Reliable);
        writer.Write(byte.MaxValue);
        writer.Write(byte.MaxValue);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.vampireSetBitten(byte.MaxValue, byte.MaxValue);
    }

    public static void handleBomberExplodeOnBodyReport()
    {
        // Murder the bitten player and reset bitten (regardless whether the kill was successful or not)
        checkMuderAttemptAndKill(Bomber.bomber, Bomber.hasBomb, true, false);
        var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
            (byte)CustomRPC.GiveBomb, SendOption.Reliable);
        writer.Write(byte.MaxValue);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.giveBomb(byte.MaxValue);
    }

    public static void refreshRoleDescription(PlayerControl player)
    {
        var infos = RoleInfo.getRoleInfoForPlayer(player);
        List<string> taskTexts = new(infos.Count);

        foreach (var roleInfo in infos) taskTexts.Add(getRoleString(roleInfo));

        var toRemove = new List<PlayerTask>();
        foreach (var t in player.myTasks.GetFastEnumerator())
        {
            var textTask = t.TryCast<ImportantTextTask>();
            if (textTask == null) continue;

            var currentText = textTask.Text;

            if (taskTexts.Contains(currentText))
                taskTexts.Remove(
                    currentText); // TextTask for this RoleInfo does not have to be added, as it already exists
            else toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
        }

        foreach (var t in toRemove)
        {
            t.OnRemove();
            player.myTasks.Remove(t);
            Object.Destroy(t.gameObject);
        }

        // Add TextTask for remaining RoleInfos
        foreach (var title in taskTexts)
        {
            var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
            task.transform.SetParent(player.transform, false);
            task.Text = title;
            player.myTasks.Insert(0, task);
        }
    }

    internal static string getRoleString(RoleInfo roleInfo)
    {
        if (roleInfo.name == "Jackal")
        {
            var getSidekickText = Jackal.canCreateSidekick ? " and recruit a Sidekick" : "";
            return cs(roleInfo.color, $"{roleInfo.name}: Kill everyone{getSidekickText}");
        }

        if (roleInfo.name == "Invert")
            return cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription} ({Invert.meetings})");

        return cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription}");
    }

    public static bool isD(byte playerId)
    {
        return playerId % 2 == 0;
    }

    public static bool isLighterColor(PlayerControl target)
    {
        return isD(target.PlayerId);
    }

    public static bool isLighterColor2(int colorId)
    {
        return CustomColors.lighterColors.Contains(colorId);
    }

    public static bool isCustomServer()
    {
        if (FastDestroyableSingleton<ServerManager>.Instance == null) return false;
        var n = FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
        return n != StringNames.ServerNA && n != StringNames.ServerEU && n != StringNames.ServerAS;
    }

    public static bool isDead(this PlayerControl player)
    {
        return player == null || player?.Data?.IsDead == true || player?.Data?.Disconnected == true;
    }

    public static void setInvisable(PlayerControl player)
    {
        var invisibleWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
            (byte)CustomRPC.SetInvisibleGen, SendOption.Reliable);
        invisibleWriter.Write(player.PlayerId);
        invisibleWriter.Write(byte.MinValue);
        AmongUsClient.Instance.FinishRpcImmediately(invisibleWriter);
        RPCProcedure.setInvisibleGen(player.PlayerId, byte.MinValue);
    }

    public static bool isAlive(this PlayerControl player)
    {
        return !isDead(player);
    }

    //假任务
    public static bool hasFakeTasks(this PlayerControl player)
    {
        return player == Werewolf.werewolf ||
            player == Doomsayer.doomsayer ||
            player == Juggernaut.juggernaut ||
               player == Jester.jester ||
               player == Arsonist.arsonist ||
               player == Jackal.jackal ||
               player == Sidekick.sidekick ||
               player == Doomsayer.doomsayer ||
               player == Pursuer.pursuer ||
               player == Akujo.akujo ||
               player == Swooper.swooper ||
               player == Lawyer.lawyer ||
               player == Vulture.vulture ||
               Jackal.formerJackals.Any(x => x == player);
    }

    public static bool canBeErased(this PlayerControl player)
    {
        return player != Jackal.jackal && player != Juggernaut.juggernaut && player != Swooper.swooper && player != Sidekick.sidekick &&
               !Jackal.formerJackals.Any(x => x == player) && player != Werewolf.werewolf;
    }

    public static bool shouldShowGhostInfo()
    {
        return (CachedPlayer.LocalPlayer.PlayerControl != null && CachedPlayer.LocalPlayer.PlayerControl.Data.IsDead &&
                MapOptions.ghostsSeeInformation) ||
               AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended;
    }

    public static void clearAllTasks(this PlayerControl player)
    {
        if (player == null) return;
        foreach (var playerTask in player.myTasks.GetFastEnumerator())
        {
            playerTask.OnRemove();
            Object.Destroy(playerTask.gameObject);
        }

        player.myTasks.Clear();

        if (player.Data != null && player.Data.Tasks != null)
            player.Data.Tasks.Clear();
    }

    public static void MurderPlayer(this PlayerControl player, PlayerControl target)
    {
        player.MurderPlayer(target, MurderResultFlags.Succeeded);
    }

    public static void RpcRepairSystem(this ShipStatus shipStatus, SystemTypes systemType, byte amount)
    {
        shipStatus.RpcUpdateSystem(systemType, amount);
    }

    public static bool isMira()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 1;
    }

    public static bool isAirship()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 4;
    }

    public static bool isSkeld()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 0;
    }

    public static bool isPolus()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 2;
    }

    public static bool isFungle()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 5;
    }


    public static bool IsCN()
    {
        return (int)AmongUs.Data.DataManager.Settings.Language.CurrentLanguage == 13;
    }

    public static string GithubUrl(this string url)
    {
        if (IsCN() && !url.Contains("github.moeyy.xyz"))
        {
            if (url.Contains("github.com"))
            {
                return url.Replace("https://github.com", "https://github.moeyy.xyz/https://github.com");
            }

            if (url.Contains("raw.githubusercontent.com"))
            {
                return url.Replace("https://raw.githubusercontent.com", "https://github.moeyy.xyz/https://raw.githubusercontent.com");
            }
        }
        //Info("Rewrite URL: " + url);
        return url;
    }

    public static bool MushroomSabotageActive()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.myTasks.ToArray()
            .Any(x => x.TaskType == TaskTypes.MushroomMixupSabotage);
    }

    public static void setSemiTransparent(this PoolablePlayer player, bool value, float alpha = 0.25f)
    {
        alpha = value ? alpha : 1f;
        foreach (var r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
            r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
        player.cosmetics.nameText.color = new Color(player.cosmetics.nameText.color.r,
            player.cosmetics.nameText.color.g, player.cosmetics.nameText.color.b, alpha);
    }

    public static string GetString(this TranslationController t, StringNames key, params Il2CppSystem.Object[] parts)
    {
        return t.GetString(key, parts);
    }

    public static string cs(Color c, string s)
    {
        return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b),
            ToByte(c.a), s);
    }

    public static int lineCount(string text)
    {
        return text.Count(c => c == '\n');
    }

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }

    public static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie)
    {
        tie = true;
        var result = new KeyValuePair<byte, int>(byte.MaxValue, int.MinValue);
        foreach (var keyValuePair in self)
            if (keyValuePair.Value > result.Value)
            {
                result = keyValuePair;
                tie = false;
            }
            else if (keyValuePair.Value == result.Value)
            {
                tie = true;
            }

        return result;
    }

    public static bool hidePlayerName(PlayerControl source, PlayerControl target)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (Camouflager.camouflageTimer > 0f || MushroomSabotageActive() || isCamoComms())
            return true; // No names are visible
        if (SurveillanceMinigamePatch.nightVisionIsActive) return true;
        if (Ninja.isInvisble && Ninja.ninja == target) return true;
        if (Swooper.isInvisable && Swooper.swooper == target) return true;
        if (MapOptions.hideOutOfSightNametags && gameStarted && !source.Data.IsDead && GameOptionsManager.Instance.currentNormalGameOptions.MapId != 5 &&
            PhysicsHelpers.AnythingBetween(localPlayer.GetTruePosition(), target.GetTruePosition(),
                Constants.ShadowMask, false)) return true;
        /*
        {
            float num = (isLightsActive() ? 2f : 1.25f);
            float num2 = Vector3.Distance(source.transform.position, target.transform.position);
            if (PhysicsHelpers.AnythingBetween(source.GetTruePosition(), target.GetTruePosition(), Constants.ShadowMask, useTriggers: false))
            {
                return true;
            }
        }
        */
        if (!MapOptions.hidePlayerNames) return false; // All names are visible
        if (source == null || target == null) return true;
        if (source == target) return false; // Player sees his own name
        if (source.Data.Role.IsImpostor && (target.Data.Role.IsImpostor || target == Spy.spy ||
                                            (target == Sidekick.sidekick && Sidekick.wasTeamRed) ||
                                            (target == Jackal.jackal && Jackal.wasTeamRed)))
            return false; // Members of team Impostors see the names of Impostors/Spies
        if ((source == Lovers.lover1 || source == Lovers.lover2) &&
            (target == Lovers.lover1 || target == Lovers.lover2))
            return false; // Members of team Lovers see the names of each other
        if ((source == Jackal.jackal || source == Sidekick.sidekick) && (target == Jackal.jackal ||
                                                                         target == Sidekick.sidekick ||
                                                                         target == Jackal.fakeSidekick))
            return false; // Members of team Jackal see the names of each other
        if (Deputy.knowsSheriff && (source == Sheriff.sheriff || source == Deputy.deputy) &&
            (target == Sheriff.sheriff || target == Deputy.deputy))
            return false; // Sheriff & Deputy see the names of each other
        return true;
    }

    public static void setDefaultLook(this PlayerControl target, bool enforceNightVisionUpdate = true)
    {
        if (MushroomSabotageActive())
        {
            var instance = ShipStatus.Instance.CastFast<FungleShipStatus>().specialSabotage;
            var condensedOutfit = instance.currentMixups[target.PlayerId];
            var playerOutfit = instance.ConvertToPlayerOutfit(condensedOutfit);
            target.MixUpOutfit(playerOutfit);
        }
        else
        {
            target.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId,
                target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId,
                enforceNightVisionUpdate);
        }
    }

    public static void setLook(this PlayerControl target, string playerName, int colorId, string hatId, string visorId,
        string skinId, string petId, bool enforceNightVisionUpdate = true)
    {
        target.RawSetColor(colorId);
        target.RawSetVisor(visorId, colorId);
        target.RawSetHat(hatId, colorId);
        target.RawSetName(hidePlayerName(CachedPlayer.LocalPlayer.PlayerControl, target) ? "" : playerName);


        SkinViewData nextSkin = null;
        try
        {
            nextSkin = ShipStatus.Instance.CosmeticsCache.GetSkin(skinId);
        }
        catch
        {
            return;
        }

        ;

        var playerPhysics = target.MyPhysics;
        AnimationClip clip = null;
        var spriteAnim = playerPhysics.myPlayer.cosmetics.skin.animator;
        var currentPhysicsAnim = playerPhysics.Animations.Animator.GetCurrentAnimation();


        if (currentPhysicsAnim == playerPhysics.Animations.group.RunAnim) clip = nextSkin.RunAnim;
        else if (currentPhysicsAnim == playerPhysics.Animations.group.SpawnAnim) clip = nextSkin.SpawnAnim;
        else if (currentPhysicsAnim == playerPhysics.Animations.group.EnterVentAnim) clip = nextSkin.EnterVentAnim;
        else if (currentPhysicsAnim == playerPhysics.Animations.group.ExitVentAnim) clip = nextSkin.ExitVentAnim;
        else if (currentPhysicsAnim == playerPhysics.Animations.group.IdleAnim) clip = nextSkin.IdleAnim;
        else clip = nextSkin.IdleAnim;
        var progress = playerPhysics.Animations.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        playerPhysics.myPlayer.cosmetics.skin.skin = nextSkin;
        playerPhysics.myPlayer.cosmetics.skin.UpdateMaterial();

        spriteAnim.Play(clip);
        spriteAnim.m_animator.Play("a", 0, progress % 1);
        spriteAnim.m_animator.Update(0f);

        target.RawSetPet(petId, colorId);

        if (enforceNightVisionUpdate) SurveillanceMinigamePatch.enforceNightVision(target);
        Chameleon.update(); // so that morphling and camo wont make the chameleons visible
    }

    public static void showFlash(Color color, float duration = 1f, string message = "", bool fade = true,
        float opacity = 100f)
    {
        if (FastDestroyableSingleton<HudManager>.Instance == null ||
            FastDestroyableSingleton<HudManager>.Instance.FullScreen == null) return;
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
        // Message Text
        var messageText = Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
            FastDestroyableSingleton<HudManager>.Instance.transform);
        messageText.text = message;
        messageText.enableWordWrapping = false;
        messageText.transform.localScale = Vector3.one * 0.5f;
        messageText.transform.localPosition += new Vector3(0f, 2f, -69f);
        messageText.gameObject.SetActive(true);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>(p =>
        {
            var renderer = FastDestroyableSingleton<HudManager>.Instance.FullScreen;

            if (p < 0.5)
            {
                if (renderer != null)
                    renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
            }
            else
            {
                if (renderer != null)
                    renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
            }

            if (p == 1f && renderer != null) renderer.enabled = false;
            if (p == 1f) messageText.gameObject.Destroy();
        })));
    }

    public static void checkWatchFlash(PlayerControl target)
    {
        if (CachedPlayer.LocalPlayer.PlayerControl == PrivateInvestigator.watching)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.PrivateInvestigatorWatchFlash, SendOption.Reliable);
            writer.Write(target.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.privateInvestigatorWatchFlash(target.PlayerId);
        }
    }

    public static bool roleCanUseVents(this PlayerControl player)
    {
        var roleCouldUse = false;
        if (player.inVent) //test
            return true;
        if (Engineer.engineer != null && Engineer.engineer == player)
        {
            roleCouldUse = true;
        }
        else if (Werewolf.canUseVents && Werewolf.werewolf != null && Werewolf.werewolf == player)
        {
            roleCouldUse = true;
        }
        else if (Jackal.canUseVents && Jackal.jackal != null && Jackal.jackal == player)
        {
            roleCouldUse = true;
        }
        else if (Sidekick.canUseVents && Sidekick.sidekick != null && Sidekick.sidekick == player)
        {
            roleCouldUse = true;
        }
        else if (Spy.canEnterVents && Spy.spy != null && Spy.spy == player)
        {
            roleCouldUse = true;
        }
        else if (Vulture.canUseVents && Vulture.vulture != null && Vulture.vulture == player)
        {
            roleCouldUse = true;
        }
        else if (Undertaker.deadBodyDraged != null && !Undertaker.canDragAndVent && Undertaker.undertaker == player)
        {
            roleCouldUse = false;
        }
        else if (Thief.canUseVents && Thief.thief != null && Thief.thief == player)
        {
            roleCouldUse = true;
        }
        else if (player.Data?.Role != null && player.Data.Role.CanVent)
        {
            if (Janitor.janitor != null && Janitor.janitor == CachedPlayer.LocalPlayer.PlayerControl)
                roleCouldUse = false;
            else if (Mafioso.mafioso != null && Mafioso.mafioso == CachedPlayer.LocalPlayer.PlayerControl &&
                     Godfather.godfather != null && !Godfather.godfather.Data.IsDead)
                roleCouldUse = false;
            else
                roleCouldUse = true;
        }
        else if (Jester.jester != null && Jester.jester == player && Jester.canVent)
        {
            roleCouldUse = true;
        }
        //天启跳洞添加
        else if (Juggernaut.juggernaut != null && Juggernaut.juggernaut == player)
        {
            roleCouldUse = true;
        }
        else if (Swooper.swooper != null && Swooper.swooper == player)
        {
            roleCouldUse = true;
        }
        if (Tunneler.tunneler != null && Tunneler.tunneler == player)
        {
            var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Tunneler.tunneler.Data);
            var numberOfTasks = playerTotal - playerCompleted;
            if (numberOfTasks == 0) roleCouldUse = true;
        }

        return roleCouldUse;
    }

    public static MurderAttemptResult checkMuderAttempt(PlayerControl killer, PlayerControl target,
        bool blockRewind = false, bool ignoreBlank = false, bool ignoreIfKillerIsDead = false)
    {
        var targetRole = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault();

        // Modified vanilla checks
        if (AmongUsClient.Instance.IsGameOver) return MurderAttemptResult.SuppressKill;
        if (killer == null || killer.Data == null || (killer.Data.IsDead && !ignoreIfKillerIsDead) ||
            killer.Data.Disconnected)
            return MurderAttemptResult.SuppressKill; // Allow non Impostor kills compared to vanilla code
        if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
            return MurderAttemptResult.SuppressKill; // Allow killing players in vents compared to vanilla code
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek || PropHunt.isPropHuntGM)
            return MurderAttemptResult.PerformKill;

        // Handle first kill attempt
        if (MapOptions.shieldFirstKill && MapOptions.firstKillPlayer == target)
            return MurderAttemptResult.SuppressKill;

        // Handle blank shot
        if (!ignoreBlank && Pursuer.blankedList.Any(x => x.PlayerId == killer.PlayerId))
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.SetBlanked, SendOption.Reliable);
            writer.Write(killer.PlayerId);
            writer.Write((byte)0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setBlanked(killer.PlayerId, 0);

            return MurderAttemptResult.BlankKill;
        }

        // Kill the killer if the Veteren is on alert

        if (Veteren.veteren != null && target == Veteren.veteren && Veteren.alertActive)
        {
            if (Medic.shielded != null && Medic.shielded == target)
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId,
                    (byte)CustomRPC.ShieldedMurderAttempt, SendOption.Reliable);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shieldedMurderAttempt(killer.PlayerId);
            }

            return MurderAttemptResult.ReverseKill;
        } // Kill the killer if the Veteren is on alert

        // Kill the Body Guard and the killer if the target is guarded

        if (BodyGuard.bodyguard != null && target == BodyGuard.guarded && isAlive(BodyGuard.bodyguard))
        {
            if (Medic.shielded != null && Medic.shielded == target)
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId,
                    (byte)CustomRPC.ShieldedMurderAttempt, SendOption.Reliable);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shieldedMurderAttempt(killer.PlayerId);
            }

            return MurderAttemptResult.BodyGuardKill;
        }

        // Block impostor shielded kill
        if (!Medic.unbreakableShield && Medic.shielded != null && Medic.shielded == target)
        {
            var write = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.SetBlanked, SendOption.Reliable);
            write.Write(killer.PlayerId);
            write.Write((byte)0);
            AmongUsClient.Instance.FinishRpcImmediately(write);
            RPCProcedure.setBlanked(killer.PlayerId, 0);
            Medic.shielded = null;

            return MurderAttemptResult.BlankKill;
        }

        if (Medic.shielded != null && Medic.shielded == target)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt,
                SendOption.Reliable);
            writer.Write(killer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.shieldedMurderAttempt(killer.PlayerId);
            SoundEffectsManager.play("fail");
            return MurderAttemptResult.SuppressKill;
        }

        // Block impostor not fully grown mini kill

        if (Mini.mini != null && target == Mini.mini && !Mini.isGrownUp()) return MurderAttemptResult.SuppressKill;
        // Block Time Master with time shield kill
        if (TimeMaster.shieldActive && TimeMaster.timeMaster != null && TimeMaster.timeMaster == target)
        {
            if (!blockRewind)
            {
                // Only rewind the attempt was not called because a meeting startet 
                var writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId,
                    (byte)CustomRPC.TimeMasterRewindTime, SendOption.Reliable);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.timeMasterRewindTime();
            }

            return MurderAttemptResult.SuppressKill;
        }

        if (Cursed.cursed != null && Cursed.cursed == target && killer.Data.Role.IsImpostor)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.SetBlanked, SendOption.Reliable);
            writer.Write(killer.PlayerId);
            writer.Write((byte)0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setBlanked(killer.PlayerId, 0);

            turnToImpostorRPC(target);

            return MurderAttemptResult.BlankKill;
        }

        if (Cultist.cultist != null && !target.Data.Role.IsImpostor && killer == Cultist.cultist)
        {
            var writer3 = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.ShowCultistFlash, SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(writer3);
            RPCProcedure.showCultistFlash();
        }

        else if (Follower.follower != null && !target.Data.Role.IsImpostor && killer == Follower.follower)
        {
            var writer3 = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.ShowFollowerFlash, SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(writer3);
            RPCProcedure.showFollowerFlash();
        }

        // Thief if hit crew only kill if setting says so, but also kill the thief.
        else if (Thief.isFailedThiefKill(target, killer, targetRole))
        {
            Thief.suicideFlag = true;
            return MurderAttemptResult.SuppressKill;
        }

        // Block hunted with time shield kill
        else if (Hunted.timeshieldActive.Contains(target.PlayerId))
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.HuntedRewindTime,
                SendOption.Reliable);
            writer.Write(target.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.huntedRewindTime(target.PlayerId);

            return MurderAttemptResult.SuppressKill;
        }

        if (TransportationToolPatches.isUsingTransportation(target) && !blockRewind && killer == Vampire.vampire)
            return MurderAttemptResult.DelayVampireKill;
        if (TransportationToolPatches.isUsingTransportation(target))
            return MurderAttemptResult.SuppressKill;
        return MurderAttemptResult.PerformKill;
    }

    public static void MurderPlayer(PlayerControl killer, PlayerControl target, bool showAnimation)
    {
        var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
            (byte)CustomRPC.UncheckedMurderPlayer, SendOption.Reliable);
        writer.Write(killer.PlayerId);
        writer.Write(target.PlayerId);
        writer.Write(showAnimation ? byte.MaxValue : 0);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.uncheckedMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? byte.MaxValue : (byte)0);
    }

    public static MurderAttemptResult checkMuderAttemptAndKill(PlayerControl killer, PlayerControl target,
        bool isMeetingStart = false, bool showAnimation = true)
    {
        return checkMurderAttemptAndKill(killer, target, isMeetingStart, showAnimation);
    }

    public static MurderAttemptResult checkMurderAttemptAndKill(PlayerControl killer, PlayerControl target,
        bool isMeetingStart = false, bool showAnimation = true, bool ignoreBlank = false,
        bool ignoreIfKillerIsDead = false)
    {
        // The local player checks for the validity of the kill and performs it afterwards (different to vanilla, where the host performs all the checks)
        // The kill attempt will be shared using a custom RPC, hence combining modded and unmodded versions is impossible
        var murder = checkMuderAttempt(killer, target, isMeetingStart, ignoreBlank, ignoreIfKillerIsDead);

        if (murder == MurderAttemptResult.PerformKill)
        {
            if (killer == Poucher.poucher) Poucher.killed.Add(target);
            if (Mimic.mimic != null && killer == Mimic.mimic && !Mimic.hasMimic)
            {
                var writerMimic = AmongUsClient.Instance.StartRpcImmediately(
                    CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.MimicMimicRole, SendOption.Reliable);
                writerMimic.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writerMimic);
                RPCProcedure.mimicMimicRole(target.PlayerId);
            }

            MurderPlayer(killer, target, showAnimation);
        }
        else if (murder == MurderAttemptResult.DelayVampireKill)
        {
            HudManager.Instance.StartCoroutine(Effects.Lerp(10f, new Action<float>(p =>
            {
                if (!TransportationToolPatches.isUsingTransportation(target) && Vampire.bitten != null)
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(
                        CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.VampireSetBitten,
                        SendOption.Reliable);
                    writer.Write(byte.MaxValue);
                    writer.Write(byte.MaxValue);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.vampireSetBitten(byte.MaxValue, byte.MaxValue);
                    MurderPlayer(killer, target, showAnimation);
                }
            })));
        }

        if (murder == MurderAttemptResult.BodyGuardKill)
        {
            // Kill the Killer
            var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.UncheckedMurderPlayer, SendOption.Reliable);
            writer.Write(killer.PlayerId);
            writer.Write(killer.PlayerId);
            writer.Write(showAnimation ? byte.MaxValue : 0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.uncheckedMurderPlayer(BodyGuard.bodyguard.PlayerId, killer.PlayerId, 0);

            // Kill the BodyGuard
            var writer2 = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.UncheckedMurderPlayer, SendOption.Reliable);
            writer2.Write(BodyGuard.bodyguard.PlayerId);
            writer2.Write(BodyGuard.bodyguard.PlayerId);
            writer2.Write(showAnimation ? byte.MaxValue : 0);
            AmongUsClient.Instance.FinishRpcImmediately(writer2);
            RPCProcedure.uncheckedMurderPlayer(BodyGuard.bodyguard.PlayerId, BodyGuard.bodyguard.PlayerId, 0);


            var writer3 = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.ShowBodyGuardFlash, SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(writer3);
            RPCProcedure.showBodyGuardFlash();
        }

        if (murder == MurderAttemptResult.ReverseKill) checkMuderAttemptAndKill(target, killer, isMeetingStart);

        return murder;
    }

    public static bool checkAndDoVetKill(PlayerControl target)
    {
        var shouldVetKill = Veteren.veteren == target && Veteren.alertActive;
        if (shouldVetKill)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.VeterenKill, SendOption.Reliable);
            writer.Write(CachedPlayer.LocalPlayer.PlayerControl.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.veterenKill(CachedPlayer.LocalPlayer.PlayerControl.PlayerId);
        }

        return shouldVetKill;
    }

    public static List<PlayerControl> getKillerTeamMembers(PlayerControl player)
    {
        var team = new List<PlayerControl>();
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
            if (player.Data.Role.IsImpostor && p.Data.Role.IsImpostor && player.PlayerId != p.PlayerId &&
                team.All(x => x.PlayerId != p.PlayerId)) team.Add(p);
            else if (player == Jackal.jackal && p == Sidekick.sidekick) team.Add(p);
            else if (player == Sidekick.sidekick && p == Jackal.jackal) team.Add(p);

        return team;
    }

    public static bool isRoleAlive(PlayerControl role)
    {
        if (Mimic.mimic != null)
            if (role == Mimic.mimic)
                return false;
        return role != null && isAlive(role);
    }

    //强力船员判定
    public static bool killingCrewAlive()
    {
        var powerCrewAlive = false;
        // This functions blocks the game from ending if specified crewmate roles are alive
        if (!CustomOptionHolder.blockGameEnd.getBool()) return false;

        if (isRoleAlive(Sheriff.sheriff)) powerCrewAlive = true;
        if (isRoleAlive(Deputy.deputy)) powerCrewAlive = true;
        if (isRoleAlive(Veteren.veteren)) powerCrewAlive = true;
        if (isRoleAlive(Mayor.mayor)) powerCrewAlive = true;
        if (isRoleAlive(Swapper.swapper)) powerCrewAlive = true;
        if (isRoleAlive(Guesser.niceGuesser)) powerCrewAlive = true;

        return powerCrewAlive;
    }

    public static bool isPlayerLover(PlayerControl player)
    {
        return !(player == null) && (player == Lovers.lover1 || player == Lovers.lover2);
    }

    public static bool isTeamCultist(PlayerControl player)
    {
        return !(player == null) && (player == Cultist.cultist || player == Follower.follower);
    }

    public static PlayerControl getChatPartner(this PlayerControl player)
    {
        //     if (!Jackal.hasChat || Sidekick.sidekick == null) return Lovers.getPartner(player);

        //     if (isPlayerLover(player) && !isTeamJackal(player))
        //         return Lovers.getPartner(player);
        //     if (isTeamJackal(player) && !isPlayerLover(player)) {
        //       if (Jackal.jackal == player) return Sidekick.sidekick;
        //       if (Sidekick.sidekick == player) return Jackal.jackal;
        //     }
        //     if (isPlayerLover(player) && isTeamJackal(player)) {
        //       if (Jackal.jackal == player) {
        //         if (Jackal.chatTarget == 1) return Sidekick.sidekick;
        //         else return Lovers.getPartner(player);
        //       }

        //       if (Sidekick.sidekick == player) {
        //         if (Sidekick.chatTarget == 1) return Jackal.jackal;
        //         else return Lovers.getPartner(player);
        //       }
        //     } 
        //     return null;
        // }

        if (!player.isLover()) return player.getCultistPartner();
        if (!player.isTeamCultist()) return player.getPartner();
        if (player == Cultist.cultist)
        {
            if (Cultist.chatTarget) return Follower.follower;
            if (!Cultist.chatTarget) return player.getPartner();
        }

        if (player == Follower.follower)
        {
            if (Follower.chatTarget2) return Cultist.cultist;
            if (!Follower.chatTarget2) return player.getPartner();
        }

        return null;
    }

    public static PlayerControl getChatPartnerSwitch(this PlayerControl player)
    {
        //     if (!Jackal.hasChat || Sidekick.sidekick == null) return Lovers.getPartner(player);

        //     if (isPlayerLover(player) && !isTeamJackal(player))
        //         return Lovers.getPartner(player);
        //     if (isTeamJackal(player) && !isPlayerLover(player)) {
        //       if (Jackal.jackal == player) return Sidekick.sidekick;
        //       if (Sidekick.sidekick == player) return Jackal.jackal;
        //     }
        //     if (isPlayerLover(player) && isTeamJackal(player)) {
        //       if (Jackal.jackal == player) {
        //         if (Jackal.chatTarget == 1) return Sidekick.sidekick;
        //         else return Lovers.getPartner(player);
        //       }

        //       if (Sidekick.sidekick == player) {
        //         if (Sidekick.chatTarget == 1) return Jackal.jackal;
        //         else return Lovers.getPartner(player);
        //       }
        //     } 
        //     return null;
        // }


        if (Follower.chatTarget2) return Cultist.cultist;
        if (!Follower.chatTarget2) return player.getPartner();

        return null;
    }

    public static bool isNeutral(PlayerControl player)
    {
        var roleInfo = RoleInfo.getRoleInfoForPlayer(player, false).FirstOrDefault();
        if (roleInfo != null)
            return roleInfo.isNeutral;
        return false;
    }

    public static bool isShiftNeutral(PlayerControl player)
    {
        if (CustomOptionHolder.modifierShiftNeutral.getBool())
        {
            if (player != null)
                return player == Jackal.jackal ||
                       player == Sidekick.sidekick ||
                       player == Werewolf.werewolf ||
                       player == Akujo.akujo ||
                       player == Juggernaut.juggernaut ||
                       player == Swooper.swooper ||
                       player == Arsonist.arsonist;
            return false;
        }
        else
        {
            var roleInfo = RoleInfo.getRoleInfoForPlayer(player, false).FirstOrDefault();
            if (roleInfo != null)
                return roleInfo.isNeutral;
            return false;
        }

    }

    public static bool isKiller(PlayerControl player)
    {
        return isNeutral(player) && (
                player == Juggernaut.juggernaut ||
                player == Werewolf.werewolf ||
                player == Swooper.swooper ||
                player == Arsonist.arsonist ||
                player == Jackal.jackal ||
                player == Sidekick.sidekick);
    }

    public static bool isEvil(PlayerControl player)
    {
        return isNeutral(player) &&
                player != Amnisiac.amnisiac &&
                player != Pursuer.pursuer;
    }

    public static void toggleZoom(bool reset = false)
    {
        var orthographicSize = reset || zoomOutStatus ? 3f : 12f;

        zoomOutStatus = !zoomOutStatus && !reset;
        Camera.main.orthographicSize = orthographicSize;
        foreach (var cam in Camera.allCameras)
            if (cam != null && cam.gameObject.name == "UI Camera")
                cam.orthographicSize =
                    orthographicSize; // The UI is scaled too, else we cant click the buttons. Downside: map is super small.

        if (HudManagerStartPatch.zoomOutButton != null)
        {
            HudManagerStartPatch.zoomOutButton.Sprite = zoomOutStatus
                ? loadSpriteFromResources("TheOtherRoles.Resources.PlusButton.png", 75f)
                : loadSpriteFromResources("TheOtherRoles.Resources.MinusButton.png", 150f);
            HudManagerStartPatch.zoomOutButton.PositionOffset =
                zoomOutStatus ? new Vector3(0f, 3f, 0) : new Vector3(0.4f, 2.8f, 0);
        }

        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height,
            Screen.fullScreen); // This will move button positions to the correct position.
    }

    private static long GetBuiltInTicks()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var builtin = assembly.GetType("Builtin");
        if (builtin == null) return 0;
        var field = builtin.GetField("CompileTime");
        if (field == null) return 0;
        var value = field.GetValue(null);
        if (value == null) return 0;
        return (long)value;
    }

    //红狼视野
    public static bool hasImpVision(GameData.PlayerInfo player)
    {
        return player.Role.IsImpostor
               || (((Jackal.jackal != null && Jackal.jackal.PlayerId == player.PlayerId) ||
                    Jackal.formerJackals.Any(x => x.PlayerId == player.PlayerId)) && Jackal.hasImpostorVision)
               || (Sidekick.sidekick != null && Sidekick.sidekick.PlayerId == player.PlayerId &&
                   Sidekick.hasImpostorVision)
               || (Spy.spy != null && Spy.spy.PlayerId == player.PlayerId && Spy.hasImpostorVision)
               || (Juggernaut.juggernaut != null && Juggernaut.juggernaut.PlayerId == player.PlayerId &&
                   Spy.hasImpostorVision)
               || (Jester.jester != null && Jester.jester.PlayerId == player.PlayerId && Jester.hasImpostorVision)
               || (Thief.thief != null && Thief.thief.PlayerId == player.PlayerId && Thief.hasImpostorVision)
               || (Swooper.swooper != null && Swooper.swooper.PlayerId == player.PlayerId && Swooper.hasImpVision)
               || (Werewolf.werewolf != null && Werewolf.werewolf.PlayerId == player.PlayerId &&
                   Werewolf.hasImpostorVision);
    }

    public static object TryCast(this Il2CppObjectBase self, Type type)
    {
        return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type)
            .Invoke(self, Array.Empty<object>());
    }

    internal static int flipBitwise(int chatTarget)
    {
        throw new NotImplementedException();
    }
}