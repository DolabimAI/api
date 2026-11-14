using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace DolApp.Api.Auth;

/// Supabase'in açık anahtarlarını (JWKS) doğrudan adresinden indirir.
/// .NET normalde anahtar adresini "openid-configuration" belgesinden bulur; burada adresi
/// kendimiz verdiğimiz için bu küçük sınıf, indirilen anahtarları .NET'in beklediği biçime çevirir.
/// İndirme ve önbellekleme işini .NET'in ConfigurationManager'ı yapar (Program.cs).

public class JwksRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
{
    private readonly string _issuer;

    public JwksRetriever(string issuer) => _issuer = issuer;

    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
        string address, IDocumentRetriever retriever, CancellationToken cancel)
    {
        var json = await retriever.GetDocumentAsync(address, cancel);
        var keySet = new JsonWebKeySet(json);

        var config = new OpenIdConnectConfiguration { Issuer = _issuer };
        foreach (var key in keySet.GetSigningKeys())
            config.SigningKeys.Add(key);

        return config;
    }
}