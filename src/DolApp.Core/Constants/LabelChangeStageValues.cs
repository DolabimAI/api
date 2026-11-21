namespace DolApp.Core.Constants;

/// label_change_stages tablosundaki value kodları: düzeltmenin yapıldığı ekran.
/// Onay: fotoğraftan sonraki etiket onayı (C3 approve). Sonradan: parça detayında düzenleme (C3 PATCH).

public static class LabelChangeStageValues
{
    public const string Approval = "onay";
    public const string Later = "sonradan";
}