using System.Collections.Concurrent;
using System.Reflection;
using Mediator;

namespace CompanyName.Infrastructure.Outbox;

internal static class MediatorNotificationPublisher
{
    private static readonly MethodInfo PublishMethodDefinition = typeof(IPublisher)
        .GetMethods(BindingFlags.Instance | BindingFlags.Public)
        .Single(m => m.Name == nameof(IPublisher.Publish)
            && m.IsGenericMethodDefinition
            && m.GetParameters().Length == 2);

    private static readonly ConcurrentDictionary<Type, PublishDelegate> PublishDelegates = new();

    private delegate ValueTask PublishDelegate(IPublisher publisher, object notification, CancellationToken cancellationToken);

    public static ValueTask PublishAsync(
        IPublisher publisher,
        object notification,
        CancellationToken cancellationToken)
    {
        var publish = PublishDelegates.GetOrAdd(notification.GetType(), CreatePublishDelegate);
        return publish(publisher, notification, cancellationToken);
    }

    private static PublishDelegate CreatePublishDelegate(Type notificationType)
    {
        var publish = PublishMethodDefinition.MakeGenericMethod(notificationType);
        return (publisher, notification, cancellationToken) =>
            (ValueTask)publish.Invoke(publisher, new object[] { notification, cancellationToken })!;
    }
}