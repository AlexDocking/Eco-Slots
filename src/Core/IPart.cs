using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Shared.Serialization;
using System;
using System.ComponentModel;

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
    /// <summary>
    /// 
    /// </summary>
    public static class PartExtensions
    {
        private static WeakKeyConcurrentDictionary<IPart, PropertyChangedEventHandler> Listeners { get; } = new WeakKeyConcurrentDictionary<IPart, PropertyChangedEventHandler>();
        /// <summary>
        /// Set up a subscription for the property to call the necessary events when it changes.
        /// This elimates the need for each part to define a PropertyChangedEventHandler to call PartPropertyChangedEvent(/Global), and to change over the subscription to the new object when the property changes, such as when loading persistent data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="part"></param>
        /// <param name="newProperty"></param>
        /// <param name="existingProperty"></param>
        public static void SetProperty<T>(this IPart part, T newProperty, ref T existingProperty) where T : IPartProperty
        {
            if (!Listeners.TryGetValue(part, out var propertyChangedEventHandler))
            {
                propertyChangedEventHandler = (object sender, PropertyChangedEventArgs args) =>
                {
                    if (sender is not IPartProperty partProperty) return;
                    part.PartPropertyChangedEvent.Invoke(part, partProperty);
                    PartNotifications.PartPropertyChangedEventGlobal.Invoke(part, partProperty);
                };
                Listeners[part] = propertyChangedEventHandler;
            }
            if (existingProperty != null) existingProperty.PropertyChanged -= propertyChangedEventHandler;
            existingProperty = newProperty;
            if (newProperty != null) newProperty.PropertyChanged += propertyChangedEventHandler;
        }
    }
}
