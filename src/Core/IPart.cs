using Eco.Core.Utils;
using Eco.Shared.Serialization;

namespace Parts
{
    [Serialized]
    public interface IPart
    {
        public string DisplayName { get; }
        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; }
    }
    public static class PartNotifications
    {
        public static ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEventGlobal { get; } = new ThreadSafeAction<IPart, IPartProperty>();
    }
}
