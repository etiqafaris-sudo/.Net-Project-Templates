using System.Text.Json.Serialization;
using CompanyName.Application.Features.Uam.CreateStaff;
using CompanyName.Application.Features.Uam.UpdateUserStatus;

namespace CompanyName.Infrastructure.Outbox;

[JsonSerializable(typeof(StaffCreatedEvent))]
[JsonSerializable(typeof(UpdatedUserStatusEvent))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class OutboxJsonContext : JsonSerializerContext;