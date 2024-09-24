using System.Collections.Generic;
using System.IO;
using TheOtherRoles.CustomCosmetics.CustomHats;
using UnityEngine;

namespace TheOtherRoles.CustomCosmetics;

public class CosmeticsManager
{
    internal static string CustomHatsDir => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, Main.ModName + "/CustomHats");
    internal static string CustomVisorsDir => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, Main.ModName + "/CustomVisors");
    internal static string CustomPlatesDir => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, Main.ModName + "/CustomPlates");
    internal static string CosmeticsConfigDir => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, Main.ModName + "/CosmeticsConfig");

    public readonly HashSet<CosmeticsManagerConfig> configs = [];
    public static readonly CosmeticsManagerConfig DefConfig = new()
    {
        ConfigName = "TheOtherHats",
        HatDirName = "hats",
        HatFileName = "CustomHats.json",
        RootUrl = "https://raw.githubusercontent.com/TheOtherRolesAU/TheOtherHats/master".GithubUrl(),
        hasCosmetics = CustomCosmeticsFlags.Hat
    };

    public void AddConfig()
    {
        configs.Add(DefConfig);
    }
}