using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System;
using System.ComponentModel;

namespace Parts
{
    [Serialized]
    public class ModelPartColouring : IController, INotifyPropertyChanged, IPartProperty
    {
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
        public static ThreadSafeAction<ModelPartColouring> OnColourChangedGlobal { get; } = new ThreadSafeAction<ModelPartColouring>();
        #region IController
        private int id;
        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        private Color colour = Color.White;
    }
}