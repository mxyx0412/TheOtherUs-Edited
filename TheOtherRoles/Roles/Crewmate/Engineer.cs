using Hazel;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Roles.Crewmate;

public class Engineer
{
    public static PlayerControl engineer;
    public static Color color = new Color32(0, 40, 245, byte.MaxValue);
    public static bool resetFixAfterMeeting;
    public static bool remoteFix = true;
    public static int remainingFixes = 1;
    //public static bool expertRepairs = false;
    public static bool highlightForImpostors = true;
    public static bool highlightForTeamJackal = true;
    public static bool usedFix;

    public static ResourceSprite buttonSprite = new("RepairButton.png");
    public static CustomButton engineerRepairButton;

    public static void resetFixes()
    {
        remainingFixes = Mathf.RoundToInt(CustomOptionHolder.engineerNumberOfFixes.getFloat());
        usedFix = false;
    }

    public virtual void ButtonCreate(HudManager _hudManager)
    {
        // Engineer Repair
        engineerRepairButton = new CustomButton(
            () =>
            {
                engineerRepairButton.Timer = 0f;
                var usedRepairWriter = AmongUsClient.Instance.StartRpcImmediately(
                    CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.EngineerUsedRepair,
                    SendOption.Reliable);
                AmongUsClient.Instance.FinishRpcImmediately(usedRepairWriter);
                RPCProcedure.engineerUsedRepair();
                SoundEffectsManager.play("engineerRepair");
                foreach (var task in CachedPlayer.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
                    if (task.TaskType == TaskTypes.FixLights)
                    {
                        var writer = AmongUsClient.Instance.StartRpcImmediately(
                            CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.EngineerFixLights,
                            SendOption.Reliable);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.engineerFixLights();
                    }
                    else if (task.TaskType == TaskTypes.RestoreOxy)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                    }
                    else if (task.TaskType == TaskTypes.ResetReactor)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
                    }
                    else if (task.TaskType == TaskTypes.ResetSeismic)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
                    }
                    else if (task.TaskType == TaskTypes.FixComms)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                    }
                    else if (task.TaskType == TaskTypes.StopCharles)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                    }
                    else if (SubmergedCompatibility.IsSubmerged &&
                             task.TaskType == SubmergedCompatibility.RetrieveOxygenMask)
                    {
                        var writer = AmongUsClient.Instance.StartRpcImmediately(
                            CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.EngineerFixSubmergedOxygen,
                            SendOption.Reliable);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.engineerFixSubmergedOxygen();
                    }
            },
            () =>
            {
                return engineer != null && engineer == CachedPlayer.LocalPlayer.PlayerControl &&
                       remainingFixes > 0 && remoteFix && !CachedPlayer.LocalPlayer.Data.IsDead;
            },
            () =>
            {
                var sabotageActive = false;
                foreach (var task in CachedPlayer.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
                    if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy ||
                        task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic ||
                        task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles
                        || (SubmergedCompatibility.IsSubmerged &&
                            task.TaskType == SubmergedCompatibility.RetrieveOxygenMask))
                        sabotageActive = true;
                return sabotageActive && remainingFixes > 0 &&
                       CachedPlayer.LocalPlayer.PlayerControl.CanMove && !usedFix;
            },
            () =>
            {
                if (resetFixAfterMeeting) resetFixes();
            },
            buttonSprite,
            CustomButton.ButtonPositions.upperRowRight,
            _hudManager,
            KeyCode.F,
            buttonText: getString("RepairText")
        );
    }

    public static void clearAndReload()
    {
        engineer = null;
        resetFixes();
        remoteFix = CustomOptionHolder.engineerRemoteFix.getBool();
        //expertRepairs = CustomOptionHolder.engineerExpertRepairs.getBool();
        resetFixAfterMeeting = CustomOptionHolder.engineerResetFixAfterMeeting.getBool();
        remainingFixes = Mathf.RoundToInt(CustomOptionHolder.engineerNumberOfFixes.getFloat());
        highlightForImpostors = CustomOptionHolder.engineerHighlightForImpostors.getBool();
        highlightForTeamJackal = CustomOptionHolder.engineerHighlightForTeamJackal.getBool();
        usedFix = false;
    }

    public virtual void ResetCustomButton()
    {
        engineerRepairButton.MaxTimer = 0f;
    }
}
