using System.ComponentModel;

namespace Parts
{
    /// <summary>
    /// A property defined by a part such as colour, extra storage slots, oil well drill depth...
    /// It could be pretty much anything a modder could think up that is relevant to a part and that wants to have some effect in the world.
    /// WorldObjectComponents will use this data to know what changes to make in the world.
    /// This data will also be displayed in the part's tooltip if the tooltip library is made aware of a suitable tooltip method to display this property.
    /// </summary>
    public interface IPartProperty : INotifyPropertyChanged { }
}