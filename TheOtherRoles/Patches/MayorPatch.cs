using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using Hazel;
using InnerNet;
using static TheOtherRoles.TheOtherRoles;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Patches;


public static class AddAbstain
{
    public static void UpdateButton(PlayerControl p, MeetingHud __instance)
    {
        if (CachedPlayer.LocalPlayer.PlayerControl != Mayor.mayor) return;
        var skip = __instance.SkipVoteButton;
        Mayor.Abstain.gameObject.SetActive(skip.gameObject.active && !Mayor.VotedOnce);
        Mayor.Abstain.voteComplete = skip.voteComplete;
        Mayor.Abstain.GetComponent<SpriteRenderer>().enabled = skip.GetComponent<SpriteRenderer>().enabled;
        Mayor.Abstain.GetComponentsInChildren<TextMeshPro>()[0].text = "存票";
    }


    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public class MeetingHudStart
    {
        public static void GenButton(PlayerControl p, MeetingHud __instance)
        {
            if (CachedPlayer.LocalPlayer.PlayerControl != Mayor.mayor) return;
            var skip = __instance.SkipVoteButton;
            Mayor.Abstain = Object.Instantiate(skip, skip.transform.parent);
            Mayor.Abstain.Parent = __instance;
            Mayor.Abstain.SetTargetPlayerId(251);
            Mayor.Abstain.transform.localPosition = skip.transform.localPosition +
                                                   new Vector3(0f, -0.17f, 0f);
            skip.transform.localPosition += new Vector3(0f, 0.20f, 0f);
            UpdateButton(p, __instance);
        }

        public static void Postfix(MeetingHud __instance)
        {
            if (!CachedPlayer.LocalPlayer.PlayerControl == Mayor.mayor) return;
            GenButton(Mayor.mayor, __instance);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ClearVote))]
    public class MeetingHudClearVote
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (CachedPlayer.LocalPlayer.PlayerControl == Mayor.mayor) UpdateButton(Mayor.mayor, __instance);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Confirm))]
    public class MeetingHudConfirm
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (!CachedPlayer.LocalPlayer.PlayerControl == Mayor.mayor) return;
            UpdateButton(Mayor.mayor, __instance);
            Mayor.Abstain.ClearButtons();
            UpdateButton(Mayor.mayor, __instance);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Select))]
    public class MeetingHudSelect
    {
        public static void Postfix(MeetingHud __instance, int __0)
        {
            if (!CachedPlayer.LocalPlayer.PlayerControl == Mayor.mayor) return;
            UpdateButton(Mayor.mayor, __instance);
            if (__0 != 251) Mayor.Abstain.ClearButtons();

            UpdateButton(Mayor.mayor, __instance);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    public class MeetingHudVotingComplete
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (!CachedPlayer.LocalPlayer.PlayerControl == Mayor.mayor) return;
            UpdateButton(Mayor.mayor, __instance);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    public class MeetingHudUpdate
    {
        public static void Postfix(MeetingHud __instance)
        {/*
            if (!CachedPlayer.LocalPlayer.PlayerControl == Mayor.mayor) return;
            switch (__instance.state)
            {
                case MeetingHud.VoteStates.Discussion:
                    if (__instance.discussionTimer < PlayerControl.GameOptions.DiscussionTime)
                    {
                        Mayor.Abstain.SetDisabled();
                        break;
                    }


                    Mayor.Abstain.SetEnabled();
                    break;
            }
            */
            UpdateButton(Mayor.mayor, __instance);
        }
    }
}




[HarmonyPatch(typeof(PlayerVoteArea))]
public class AllowExtraVotes
{
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
    public static class Select
    {
        public static bool Prefix(PlayerVoteArea __instance)
        {
            if (CachedPlayer.LocalPlayer.PlayerControl != Mayor.mayor) return true;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (__instance.AmDead) return false;
            if (!Mayor.CanVote || !__instance.Parent.Select(__instance.TargetPlayerId)) return false;
            __instance.Buttons.SetActive(true);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.VoteForMe))]
    public static class VoteForMe
    {
        public static bool Prefix(PlayerVoteArea __instance)
        {
            if (CachedPlayer.LocalPlayer.PlayerControl != Mayor.mayor) return true;
            if (__instance.Parent.state == MeetingHud.VoteStates.Proceeding ||
                __instance.Parent.state == MeetingHud.VoteStates.Results)
                return false;

            if (!Mayor.CanVote) return false;
            if (__instance != Mayor.Abstain)
            {
                Mayor.VoteBank--;
                Mayor.VotedOnce = true;
            }
            else
            {
                Mayor.SelfVote = true;
            }

            __instance.Parent.Confirm(__instance.TargetPlayerId);
            return false;
        }
    }
}



[HarmonyPatch(typeof(MeetingHud))]
public class RegisterExtraVotes
{
    [HarmonyPatch(nameof(MeetingHud.Update))]
    public static void Postfix(MeetingHud __instance)
    {
        if (!CachedPlayer.LocalPlayer.PlayerControl == Mayor.mayor) return;
        if (PlayerControl.LocalPlayer.Data.IsDead) return;
        if (__instance.TimerText.text.Contains("Can Vote")) return;
        __instance.TimerText.text = "Can Vote: " + Mayor.VoteBank + " time(s) | " + __instance.TimerText.text;
    }

    public static Dictionary<byte, int> CalculateAllVotes(MeetingHud __instance)
    {
        var dictionary = new Dictionary<byte, int>();
        for (var i = 0; i < __instance.playerStates.Length; i++)
        {
            var playerVoteArea = __instance.playerStates[i];
            if (!playerVoteArea.DidVote
                || playerVoteArea.AmDead
                || playerVoteArea.VotedFor == PlayerVoteArea.MissedVote
                || playerVoteArea.VotedFor == PlayerVoteArea.DeadVote) continue;

            if (dictionary.TryGetValue(playerVoteArea.VotedFor, out var num))
                dictionary[playerVoteArea.VotedFor] = num + 1;
            else
                dictionary[playerVoteArea.VotedFor] = 1;
        }

        foreach (var role in RoleInfo.getRoleInfoForPlayer(Mayor.mayor))
            foreach (var number in Mayor.ExtraVotes)
                if (dictionary.TryGetValue(number, out var num))
                    dictionary[number] = num + 1;
                else
                    dictionary[number] = 1;

        dictionary.MaxPair(out var tie);

        if (tie)
            foreach (var player in __instance.playerStates)
            {
                if (!player.DidVote
                    || player.AmDead
                    || player.VotedFor == PlayerVoteArea.MissedVote
                    || player.VotedFor == PlayerVoteArea.DeadVote) continue;
            }

        return dictionary;
    }

    [HarmonyPatch(nameof(MeetingHud.Start))]
    public static void Prefix()
    {
        foreach (var role in RoleInfo.getRoleInfoForPlayer(Mayor.mayor))
        {
            Mayor.ExtraVotes.Clear();
            if (Mayor.VoteBank < 0)
                Mayor.VoteBank = 0;

            Mayor.VoteBank++;
            Mayor.SelfVote = false;
            Mayor.VotedOnce = false;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(
        nameof(MeetingHud.HandleDisconnect),
        typeof(PlayerControl), typeof(DisconnectReasons)
    )]
    public static void Prefix(
        MeetingHud __instance, [HarmonyArgument(0)] PlayerControl player)
    {
        if (AmongUsClient.Instance.AmHost && MeetingHud.Instance)
        {
            foreach (var role in RoleInfo.getRoleInfoForPlayer(Mayor.mayor))
            {
                if (CachedPlayer.LocalPlayer.PlayerControl == Mayor.mayor)
                {
                    if (Mayor.VotedOnce)
                    {
                        var votesRegained = Mayor.ExtraVotes.RemoveAll(x => x == player.PlayerId);

                        if (Mayor.mayor == PlayerControl.LocalPlayer)
                            Mayor.VoteBank += votesRegained;

                        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte)CustomRPC.AddMayorVoteBank, SendOption.Reliable, -1);
                        writer.Write(Mayor.mayor.PlayerId);
                        writer.Write(Mayor.VoteBank);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Confirm))]
    public static class Confirm
    {
        public static bool Prefix(MeetingHud __instance)
        {
            if (CachedPlayer.LocalPlayer.PlayerControl != Mayor.mayor) return true;
            if (__instance.state != MeetingHud.VoteStates.Voted) return true;
            __instance.state = MeetingHud.VoteStates.NotVoted;
            return true;
        }

        [HarmonyPriority(Priority.First)]
        public static void Postfix(MeetingHud __instance)
        {
            if (CachedPlayer.LocalPlayer.PlayerControl != Mayor.mayor) return;
            if (Mayor.CanVote) __instance.SkipVoteButton.gameObject.SetActive(true);
        }
    }


    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
    public static class CastVote
    {
        public static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] byte srcPlayerId,
            [HarmonyArgument(1)] byte suspectPlayerId)
        {
            var player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == srcPlayerId);
            if (CachedPlayer.LocalPlayer.PlayerControl != Mayor.mayor) return true;

            var playerVoteArea = __instance.playerStates.ToArray().First(pv => pv.TargetPlayerId == srcPlayerId);

            if (playerVoteArea.AmDead)
                return false;

            if (PlayerControl.LocalPlayer.PlayerId == srcPlayerId)
            {
                SoundManager.Instance.PlaySound(__instance.VoteLockinSound, false, 1f);
            }

            if (playerVoteArea.DidVote)
            {
                Mayor.ExtraVotes.Add(suspectPlayerId);
            }
            else
            {
                playerVoteArea.SetVote(suspectPlayerId);
                playerVoteArea.Flag.enabled = true;
                PlayerControl.LocalPlayer.RpcSendChatNote(srcPlayerId, ChatNoteTypes.DidVote);
            }
            __instance.Cast<InnerNetObject>().SetDirtyBit(1U);
            __instance.CheckForEndVoting();

            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    public static class VotingComplete
    {
        public static void Postfix(MeetingHud __instance,
            [HarmonyArgument(0)] Il2CppStructArray<MeetingHud.VoterState> states,
            [HarmonyArgument(1)] GameData.PlayerInfo exiled,
            [HarmonyArgument(2)] bool tie)
        {
            // __instance.exiledPlayer = __instance.wasTie ? null : __instance.exiledPlayer;
            var exiledString = exiled == null ? "null" : exiled.PlayerName;
            Message($"Exiled PlayerName = {exiledString}");
            Message($"Was a tie = {tie}");
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
    public static class PopulateResults
    {
        public static bool Prefix(MeetingHud __instance,
            [HarmonyArgument(0)] Il2CppStructArray<MeetingHud.VoterState> statess)
        {
            // var joined = string.Join(",", statess);
            // var arr = joined.Split(',');
            // var states = arr.Select(byte.Parse).ToArray();

            // var allnums = new int[__instance.playerStates.Length];

            var allNums = new Dictionary<int, int>();


            __instance.TitleText.text = Object.FindObjectOfType<TranslationController>()
                .GetString(StringNames.MeetingVotingResults, Array.Empty<Il2CppSystem.Object>());
            var amountOfSkippedVoters = 0;
            for (var i = 0; i < __instance.playerStates.Length; i++)
            {
                var playerVoteArea = __instance.playerStates[i];
                playerVoteArea.ClearForResults();
                allNums.Add(i, 0);

                for (var stateIdx = 0; stateIdx < statess.Length; stateIdx++)
                {
                    var voteState = statess[stateIdx];
                    var playerInfo = GameData.Instance.GetPlayerById(voteState.VoterId);
                    if (playerInfo == null)
                    {
                        Error(string.Format("Couldn't find player info for voter: {0}", voteState.VoterId));
                    }
                    else if (i == 0 && voteState.SkippedVote)
                    {
                        __instance.BloopAVoteIcon(playerInfo, amountOfSkippedVoters, __instance.SkippedVoting.transform);
                        amountOfSkippedVoters++;
                    }
                    else if (voteState.VotedForId == playerVoteArea.TargetPlayerId)
                    {
                        __instance.BloopAVoteIcon(playerInfo, allNums[i], playerVoteArea.transform);
                        allNums[i]++;
                    }
                }
            }

            foreach (var role in RoleInfo.getRoleInfoForPlayer(Mayor.mayor))
            {
                var playerInfo = GameData.Instance.GetPlayerById(Mayor.mayor.PlayerId);

                foreach (var extraVote in Mayor.ExtraVotes)
                {
                    if (extraVote == PlayerVoteArea.HasNotVoted ||
                        extraVote == PlayerVoteArea.MissedVote ||
                        extraVote == PlayerVoteArea.DeadVote)
                    {
                        continue;
                    }
                    if (extraVote == PlayerVoteArea.SkippedVote)
                    {

                        __instance.BloopAVoteIcon(playerInfo, amountOfSkippedVoters, __instance.SkippedVoting.transform);
                        amountOfSkippedVoters++;
                    }
                    else
                    {
                        for (var i = 0; i < __instance.playerStates.Length; i++)
                        {
                            var area = __instance.playerStates[i];
                            if (extraVote != area.TargetPlayerId) continue;
                            __instance.BloopAVoteIcon(playerInfo, allNums[i], area.transform);
                            allNums[i]++;
                        }
                    }
                }
            }

            return false;
        }
    }
}