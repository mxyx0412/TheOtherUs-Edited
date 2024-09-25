using System.Text.Json.Serialization;
using System;

namespace TheOtherRoles.CustomCosmetics;

public class CosmeticsManagerConfig
{
    public string ConfigName = "None";
    public string RootUrl { get; set; }
    public CustomCosmeticsFlags hasCosmetics { get; set; }
    public string HatDirName { get; set; } = "hats";
    public string VisorDirName { get; set; } = "Visors";
    public string NamePlateDirName { get; set; } = "NamePlates";
    public string HatFileName { get; set; } = "CustomHats.json";
    public string VisorFileName { get; set; } = "CustomVisors.json";
    public string NamePlateFileName { get; set; } = "CustomNamePlates.json";
    public string HatPropertyName { get; set; } = "hats";
    public string VisorPropertyName { get; set; } = "Visors";
    public string NamePlatePropertyName { get; set; } = "nameplates";
}

[Flags, JsonConverter(typeof(JsonStringEnumConverter))]
public enum CustomCosmeticsFlags
{
    Hat = 1,
    Skin,
    Visor,
    NamePlate,
}
