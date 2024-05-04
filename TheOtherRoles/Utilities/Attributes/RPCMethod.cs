using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;

namespace TheOtherRoles.Utilities.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RPCMethod(CustomRPC rpc) : Attribute
{
    public CustomRPC RPC = rpc;

    public Action<object[]> Start;

    public static readonly List<RPCMethod> _AllRPCMethod =
        [];
    
    public static void Register(Assembly assembly)
    {
        var types = assembly.GetTypes().SelectMany(n => n.GetMethods(BindingFlags.Static)).Where(n => n.IsDefined(typeof(RPCMethod)));
        types.Do(n =>
        {
            var method = n.GetCustomAttribute<RPCMethod>();
            if (method == null) return;
            method.Start = objs => n.Invoke(null, objs);
            _AllRPCMethod.Add(method);
        });
    }

    public static void StartRPCMethod(CustomRPC rpc, params object[] objects)
    {
        _AllRPCMethod.Where(n => n.RPC == rpc).Do(n => n.Start(objects));
    }
}