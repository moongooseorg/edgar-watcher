using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

namespace OpenBaoConfiguration;

public sealed class OpenBaoConfigurationSource : IConfigurationSource
{
    public required string Address { get; init; }
    public required string Path { get; init; }
    public string? RoleId { get; init; }
    public string? SecretId { get; init; }
    public string? Token { get; init; }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new OpenBaoConfigurationProvider(this);
}

public sealed class OpenBaoConfigurationProvider(OpenBaoConfigurationSource source) : ConfigurationProvider
{
    private const string MountPoint = "kv";

    public override void Load() => LoadAsync().GetAwaiter().GetResult();

    private async Task LoadAsync()
    {
        IAuthMethodInfo auth = source switch
        {
            { RoleId.Length: > 0, SecretId.Length: > 0 } => new AppRoleAuthMethodInfo(source.RoleId, source.SecretId),
            { Token.Length: > 0 } => new TokenAuthMethodInfo(source.Token),
            _ => throw new InvalidOperationException(
                "OpenBao is configured but no credentials were supplied. Set BAO_ROLE_ID and BAO_SECRET_ID, or BAO_TOKEN."),
        };

        VaultClientSettings settings = new(source.Address, auth);

#if DEBUG
        settings.PostProcessHttpClientHandlerAction = handler =>
        {
            if (handler is HttpClientHandler clientHandler)
                clientHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        };
#endif

        IVaultClient client = new VaultClient(settings);

        Secret<SecretData> secret = await client.V1.Secrets.KeyValue.V2
            .ReadSecretAsync(path: source.Path, mountPoint: MountPoint);

        byte[] json = JsonSerializer.SerializeToUtf8Bytes(secret.Data.Data);

        IConfigurationRoot parsed = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(json))
            .Build();

        Data = parsed.AsEnumerable()
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    }
}

public static class OpenBaoConfigurationExtensions
{
    public static IConfigurationBuilder AddOpenBao(this IConfigurationBuilder builder, IConfiguration bootstrap)
    {
        string? address = bootstrap["BAO_ADDR"];

        if (string.IsNullOrWhiteSpace(address))
            return builder;

        string path = Assembly.GetEntryAssembly()?.GetName().Name
            ?? throw new InvalidOperationException(
                "Unable to determine the application name for the OpenBao secret path.");

        return builder.Add(new OpenBaoConfigurationSource
        {
            Address = address,
            Path = path,
            RoleId = bootstrap["BAO_ROLE_ID"],
            SecretId = bootstrap["BAO_SECRET_ID"],
            Token = bootstrap["BAO_TOKEN"],
        });
    }
}
