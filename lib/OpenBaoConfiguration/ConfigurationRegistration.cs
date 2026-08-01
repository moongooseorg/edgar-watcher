using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace OpenBaoConfiguration;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigurationSectionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

public static class ConfigurationRegistration
{
    public static IHostApplicationBuilder AddOpenBaoConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Configuration.AddOpenBao(builder.Configuration);

        foreach (Type type in Assembly.GetEntryAssembly()!.GetTypes())
        {
            if (type.GetCustomAttribute<ConfigurationSectionAttribute>() is not { } attribute)
                continue;

            IConfigurationSection section = builder.Configuration.GetSection(attribute.Name);

            RequireAllKeys(type, section);

            builder.Services.AddSingleton(
                typeof(IOptions<>).MakeGenericType(type),
                Activator.CreateInstance(
                    typeof(OptionsWrapper<>).MakeGenericType(type),
                    section.Get(type) ?? Activator.CreateInstance(type)!)!);
        }

        return builder;
    }

    private static void RequireAllKeys(Type type, IConfigurationSection section)
    {
        HashSet<string> present = section
            .GetChildren()
            .Select(child => child.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        string[] missing = type
            .GetProperties()
            .Select(property => property.Name)
            .Where(name => !present.Contains(name))
            .ToArray();

        if (missing.Length > 0)
            throw new InvalidOperationException(
                $"Configuration section '{section.Key}' is missing key(s): {string.Join(", ", missing)}");
    }
}
