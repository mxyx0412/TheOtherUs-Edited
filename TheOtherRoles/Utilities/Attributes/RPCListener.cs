using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hazel;
using InnerNet;

namespace TheOtherRoles.Utilities.Attributes;

[Harmony]
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class RPCListener : Attribute
{
    private static readonly List<RPCListener> _allListeners = [];
    public Action<MessageReader> OnRPC = null!;
    private readonly CustomRPC RPCId;

    public RPCListener(CustomRPC rpc)
    {
        RPCId = rpc;
        OnRPC += n => Info($"{RPCId} {n.Tag} {n.Length}");
    }

    public static void Register(Assembly assembly)
    {
        var types = assembly.GetTypes().SelectMany(n => n.GetMethods())
            .Where(n => n.IsDefined(typeof(RPCListener)));
        types.Do(n =>
        {
            var listener = n.GetCustomAttribute<RPCListener>();
            if (listener == null) return;
            listener.OnRPC += reader => n.Invoke(null, [reader]);
            Info($"add listener {n.Name} {n.GetType().Name}");
            _allListeners.Add(listener);
        });
    }

    [HarmonyPatch(typeof(InnerNetClient._HandleGameDataInner_d__39), nameof(InnerNetClient._HandleGameDataInner_d__39.MoveNext))]
    [HarmonyPrefix]
    private static void InnerNet_ReaderPath(InnerNetClient._HandleGameDataInner_d__39 __instance)
    {
        if (_allListeners.Count <= 0) return;
        var innerNetClient = __instance.__4__this;
        var reader = __instance.reader;

        if (__instance.__1__state != 0) return;

        var HandleReader = MessageReader.Get(reader);
        HandleReader.Position = 0;
        var tag = reader.Tag;
        if (tag != 2)
            goto recycle;
        var objetId = HandleReader.ReadPackedUInt32();
        var callId = HandleReader.ReadByte();
        if (_allListeners.All(n => n.RPCId != (CustomRPC)callId))
            goto recycle;
        try
        {
            _allListeners.Where(n => n.RPCId == (CustomRPC)callId).Do(n => n.OnRPC.Invoke(HandleReader));
            Info("Listener");
        }
        catch (Exception e)
        {
            Exception(e);
        }

        finally
        {
            HandleReader.Recycle();
        }

        return;
    recycle:
        HandleReader.Recycle();
        return;
    }
}