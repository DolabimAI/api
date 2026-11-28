using DolApp.Application.Interfaces;

namespace DolApp.Infrastructure.Services;

/// IRandomProvider'ın gerçek implementasyonu. Random.Shared iş parçacıkları arasında güvenle paylaşılır,
/// bu yüzden sınıf tek örnek (singleton) olarak kaydedilir.

public class SystemRandomProvider : IRandomProvider
{
    public bool NextBool() => Random.Shared.Next(2) == 0;
}