using System.Collections.Generic;
using System.IO;
using BepInEx;
using TheOtherRoles.CustomCosmetics.CustomHats;

namespace TheOtherRoles.CustomCosmetics;

public class CosmeticsManager : ManagerBase<CosmeticsManager>
{
    internal static string CosmeticDir = Path.Combine(Paths.GameRootPath, Main.ModName);
    internal static string CustomHatsDir => Path.Combine(CosmeticDir, "CustomHats");
    internal static string CustomVisorsDir => Path.Combine(CosmeticDir, "CustomVisors");
    internal static string CustomPlatesDir => Path.Combine(CosmeticDir, "CustomPlates");
    internal static string CosmeticsConfigDir => Path.Combine(CosmeticDir, "CosmeticsConfig");

    public readonly HashSet<CosmeticsManagerConfig> configs = [];
    public readonly CosmeticsManagerConfig DefConfig = new()
    {
        ConfigName = "TheOtherHats",
        HatDirName = "hats",
        HatFileName = "CustomHats.json",
        RootUrl = "https://raw.githubusercontent.com/TheOtherRolesAU/TheOtherHats/master".GithubUrl(),
        hasCosmetics = CustomCosmeticsFlags.Hat
    };

    internal static string RepositoryUrl => "https://raw.githubusercontent.com/TheOtherRolesAU/TheOtherHats/master".GithubUrl();

    public static void Load()
    {
        Instance.AddConfig();
        CustomHatManager.LoadHats();
        CustomColors.Load();
    }

    public void AddConfig()
    {
        configs.Add(DefConfig);
    }
}