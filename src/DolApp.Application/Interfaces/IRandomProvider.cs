namespace DolApp.Application.Interfaces;

/// Rastgele seçimler. C5 ve C6'da kural kartının solda mı sağda mı duracağı buradan seçilir.
/// Arayüz olmasının sebebi IDateTimeProvider'la aynı: testlerde sonucu sabit bir sahte verilebilsin.

public interface IRandomProvider
{
    /// Yarı yarıya true ya da false.
    bool NextBool();
}
