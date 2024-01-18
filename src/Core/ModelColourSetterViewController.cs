using Eco.Core.Controller;
using Eco.Gameplay.Objects;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Eco.Shared.View;
using System.ComponentModel;

namespace Parts
{
    /// <summary>
    /// Send colours to the client to change the colour of the world object model
    /// </summary>
    public class ModelColourSetterViewController : IController, INotifyPropertyChanged
    {
        public WorldObject WorldObject { get; private set; }
        public IHasModelPartColourComponent Model { get; private set; }

        public void SetModel(WorldObject worldObject, IHasModelPartColourComponent model)
        {
            WorldObject = worldObject;
            Model?.ColourData.Unsubscribe(nameof(ModelPartColouring.Colour), OnModelChanged);
            Model = model;
            Model?.ColourData.SubscribeAndCall(nameof(ModelPartColouring.Colour), OnModelChanged);
        }
        /// <summary>
        /// Update the view with the model colour
        /// </summary>
        private void OnModelChanged()
        {
            ModelPartColouring partColouring = Model?.ColourData;
            if (partColouring == null) return;

            SetColour(partColouring.ModelName, partColouring.Colour);
        }
        private void SetColour(string modelName, Color colour)
        {
            WorldObject.SetAnimatedState(modelName + "-Red", colour.R);
            WorldObject.SetAnimatedState(modelName + "-Green", colour.G);
            WorldObject.SetAnimatedState(modelName + "-Blue", colour.B);
        }

        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
