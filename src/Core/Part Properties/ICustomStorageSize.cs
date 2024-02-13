using Eco.Core.Controller;
using System.ComponentModel;

namespace Parts
{
    /// <summary>
    /// Defines how many extra storage slots to add to the world object's storage.
    /// </summary>
    public interface ICustomStorageSize : IController, INotifyPropertyChanged, IPartProperty
    {
        public int NumberOfAdditionalSlots { get; }
    }
}