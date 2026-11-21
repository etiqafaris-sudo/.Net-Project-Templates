using CompanyName.Domain.Common;

namespace CompanyName.Infrastructure.Outbox;

internal static class OutboxDomainEventTypes
{
    private static readonly Lazy<IReadOnlyDictionary<string, Type>> TypesByFullName = new(Discover);

    private static IReadOnlyDictionary<string, Type> Discover()
    {
        var types = new Dictionary<string, Type>(StringComparer.Ordinal);

        foreach (var type in typeof(IDomainEvent).Assembly.GetTypes())
        {
            if (type is { IsAbstract: false, IsInterface: false } && typeof(IDomainEvent).IsAssignableFrom(type))
            {
                if (type.FullName is not null)
                {
                    types[type.FullName] = type;
                }
            }
        }

        return types;
    }

    public static Type Resolve(string storedType)
    {
        if (TypesByFullName.Value.TryGetValue(storedType, out var known))
        {
            return known;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
            {
                continue;
            }

            var resolved = assembly.GetType(storedType, throwOnError: false, ignoreCase: false);
            if (resolved is not null && typeof(IDomainEvent).IsAssignableFrom(resolved))
            {
                return resolved;
            }
        }

        throw new InvalidOperationException($"Unknown outbox message type: {storedType}.");
    }
}