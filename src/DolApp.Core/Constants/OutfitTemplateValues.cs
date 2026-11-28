namespace DolApp.Core.Constants;

/// outfit_templates tablosundaki value kodları (002_seed_lookups.sql). Kod içinde metin yerine bu sabitler kullanılır.
/// Python'a giden geçmişteki "template" alanı da bu kodlardır: kombinde tek parça varsa tek_parca, yoksa ust_alt.

public static class OutfitTemplateValues
{
    public const string TopAndBottom = "ust_alt";
    public const string OnePiece = "tek_parca";
}