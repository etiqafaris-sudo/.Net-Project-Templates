using CompanyName.Application.Features.Uam.CreateStaff;
using CompanyName.Application.Features.Uam.UpdateUserStatus;

namespace CompanyName.Infrastructure.Outbox;

internal static class OutboxMessageTypeRegistry
{
    private static readonly Dictionary<string, Type> TypesByName = new(StringComparer.Ordinal);
    static OutboxMessageTypeRegistry()
    {
        Register<StaffCreatedEvent>();
        Register<UpdatedUserStatusEvent>();
    }

    public static void Register<T>()
        where T : class
    {
        var name = GetTypeName<T>();
        TypesByName[name] = typeof(T);
    }

    public static string GetTypeName<T>()
        where T : class =>
        typeof(T).FullName ?? throw new InvalidOperationException($"Cannot resolve type name for {typeof(T).Name}.");

    public static Type Resolve(string storedType)
    {
        if (TypesByName.TryGetValue(storedType, out var registered))
        {
            return registered;
        }

        var legacy = Type.GetType(storedType);
        if (legacy is not null)
        {
            return legacy;
        }

        throw new InvalidOperationException($"Unknown outbox message type: {storedType}.");
    }
}
