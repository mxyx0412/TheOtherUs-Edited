using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using UnityEngine;

namespace TheOtherRoles.CustomCosmetics.CustomHats;

public class SkinsConfigFile
{
    [JsonPropertyName("hats")] public List<CustomHatConfig> Hats { get; set; }
}

public class CustomHatConfig
{
    [JsonPropertyName("author")] public string Author { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("package")] public string Package { get; set; }
    [JsonPropertyName("condition")] public string Condition { get; set; }
    [JsonPropertyName("adaptive")] public bool Adaptive { get; set; }
    [JsonPropertyName("bounce")] public bool Bounce { get; set; }
    [JsonPropertyName("behind")] public bool Behind { get; set; }
    [JsonPropertyName("resource")] public string Resource { get; set; }
    [JsonPropertyName("backresource")] public string BackResource { get; set; }
    [JsonPropertyName("climbresource")] public string ClimbResource { get; set; }
    [JsonPropertyName("flipresource")] public string FlipResource { get; set; }
    [JsonPropertyName("backflipresource")] public string BackFlipResource { get; set; }
    [JsonPropertyName("reshasha")] public string ResHashA { get; set; }
    [JsonPropertyName("reshashb")] public string ResHashB { get; set; }
    [JsonPropertyName("reshashc")] public string ResHashC { get; set; }
    [JsonPropertyName("reshashf")] public string ResHashF { get; set; }
    [JsonPropertyName("reshashbf")] public string ResHashBf { get; set; }
}

public class HatExtension
{
    public string Author { get; set; }
    public string Package { get; set; }
    public string Condition { get; set; }
    public Sprite FlipImage { get; set; }
    public Sprite BackFlipImage { get; set; }
    public bool Adaptive { get; set; }
}

[Flags, JsonConverter(typeof(JsonStringEnumConverter))]
public enum CustomCosmeticsFlags
{
    Hat = 1,
    Skin = 2,
    Visor = 8,
    NamePlate = 16,
}

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

internal static class HatDataExtensions
{
    public static HatExtension GetHatExtension(this HatData hat)
    {
        if (CustomHatManager.TestExtension != null && CustomHatManager.TestExtension.Condition.Equals(hat.name))
            return CustomHatManager.TestExtension;

        return CustomHatManager.ExtensionCache.TryGetValue(hat.name, out var extension) ? extension : null;
    }
}
