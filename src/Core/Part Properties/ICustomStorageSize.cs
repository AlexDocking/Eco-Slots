using Eco.Core.Controller;
using System.ComponentModel;

namespace Parts
{
    public interface ICustomStorageSize : IController, INotifyPropertyChanged, IPartProperty
    {
        public int NumberOfAdditionalSlots { get; }
    }
}
