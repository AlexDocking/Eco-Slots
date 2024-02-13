using System.ComponentModel;

namespace Parts
{
    /// <summary>
    /// Defines how many extra storage slots to add to the world object's storage.
    /// </summary>
    public class CustomPublicStorageSize : ICustomStorageSize, IPartProperty
    {
        public int NumberOfAdditionalSlots { get; init; }

        #region IController
        private int id;
        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
