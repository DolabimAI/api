namespace DolApp.Application.UseCases.Account.GetMe;

/// Şehrim'deki il ya da ilçe: { "id": 34, "name": "İstanbul" }.

public class MeLocationResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}