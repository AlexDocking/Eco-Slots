using System.ComponentModel;

namespace Parts
{
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
