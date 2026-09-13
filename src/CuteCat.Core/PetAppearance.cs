using System.Globalization;

namespace CuteCat.Core;

public enum CatHat { None, Beanie, Beret, Sunhat, PartyHat, Flower }
public enum CatNeckwear { None, Bandana, BowTie, Scarf }
public enum CatCollar { None, Classic, Bell, Heart }
public sealed record PetAppearance
{
    public const string Oat="#FCF0D5",Sage="#95B99B";
    public string CoatColor { get; init; }=Oat;
    public CatHat Hat { get; init; }
    public string HatColor { get; init; }=Sage;
    public CatNeckwear Neckwear { get; init; }
    public string NeckwearColor { get; init; }=Sage;
    public CatCollar Collar { get; init; }
    public string CollarColor { get; init; }=Sage;
    public static PetAppearance Default { get; }=new();
    public PetAppearance Normalize()=>this with{CoatColor=Color(CoatColor,Oat),HatColor=Color(HatColor,Sage),NeckwearColor=Color(NeckwearColor,Sage),CollarColor=Color(CollarColor,Sage),
        Hat=Enum.IsDefined(Hat)?Hat:CatHat.None,Neckwear=Enum.IsDefined(Neckwear)?Neckwear:CatNeckwear.None,Collar=Enum.IsDefined(Collar)?Collar:CatCollar.None};
    public static bool IsColor(string? value)=>value is {Length:7}&&value[0]=='#'&&uint.TryParse(value.AsSpan(1),NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture,out _);
    public static string Color(string? value,string fallback)=>IsColor(value)?value!.ToUpperInvariant():fallback;
    public static string LegacyColor(string? name)=>name switch{"Rose"=>"#D794A5","Sky"=>"#8FBBD1","Plum"=>"#A795BE","Honey"=>"#DBB76D",_=>Sage};
    public static PetAppearance FromLegacy(PetAccessory accessory,string? color)
    {
        string hex=LegacyColor(color);
        return new(){Hat=accessory==PetAccessory.Flower?CatHat.Flower:CatHat.None,HatColor=hex,
            Neckwear=accessory==PetAccessory.Bandana?CatNeckwear.Bandana:accessory==PetAccessory.BowTie?CatNeckwear.BowTie:CatNeckwear.None,NeckwearColor=hex,
            Collar=accessory==PetAccessory.BellCollar?CatCollar.Bell:CatCollar.None,CollarColor=hex};
    }
}
