using System.Text.Json;

namespace CompanyName.Infrastructure.Outbox;

internal static class OutboxJson
{
    public static JsonSerializerOptions SerializerOptions => OutboxJsonContext.Default.Options;
}