using System.Collections.Generic;
using TheOtherRoles.CustomCosmetics.CustomHats;

namespace TheOtherRoles.CustomCosmetics;
public class CosmeticsManager
{

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
