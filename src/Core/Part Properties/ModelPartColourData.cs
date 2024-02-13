using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System;
using System.ComponentModel;

namespace Parts
{
    /// <summary>
    /// Colour data for part of a world object's model, such as door colour.
    /// </summary>
    [Serialized]
    public class ModelPartColourData : IController, INotifyPropertyChanged, IPartProperty
    {
        /// <summary>
        /// The name of the animator state variable that will change the colour of some aspect of the world object's model on the client.
        /// Colours are sent to the client in separate channels via 'SetAnimatorState' as 'ModelName-Red', 'ModelName-Green' and 'ModelName-Blue',
        /// and these are picked up by float events on the client which set corresponding floats on the animator, which then uses blend trees to set the material's colour channels.
        /// </summary>
        [Serialized]
        public string ModelName { get; set; }
        [Serialized]
        public Color Colour
        {
            get => colour; set
            {
                colour = value;
                this.Changed(nameof(Colour));
                OnColourChangedGlobal.Invoke(this);
            }
        }
        /// <summary>
        /// Called whenever any part changes colour. Used to update the part's and container's tooltips since I believe 'TooltipAffectedBy' uses Fody as it does not work for mods.
        /// </summary>
        public static ThreadSafeAction<ModelPartColourData> OnColourChangedGlobal { get; } = new ThreadSafeAction<ModelPartColourData>();
        #region IController
        private int id;
        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        private Color colour = Color.White;
    }
}