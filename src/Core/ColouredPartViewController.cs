using Eco.Core.Controller;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System;
using System.ComponentModel;

namespace Parts
{
    [Serialized, AutogenClass]
    [UITypeName("PropertyPage")]
    public class ColouredPartViewController : IController, INotifyPropertyChanged
    {
        [SyncToView, Autogen]
        [UITypeName("GeneralHeader")]
        public string NameDisplay => Model?.DisplayName;

        [SyncToView, Autogen, AutoRPC]
        public string ColourHex
        {
            get => ToHex(); set
            {
                Color colour = new Color(ColorUtility.RGBHex(value));
                r = colour.R;
                g = colour.G;
                b = colour.B;
                OnUserInput();
            }
        }

        [LocDisplayName("Red")]
        [SyncToView, Autogen, AutoRPC]
        [Range(0, 1)]
        public float R
        {
            get => r; set
            {
                float newValue = Math.Clamp(value, 0, 1);
                if (r == newValue) return;
                r = newValue;
                OnUserInput();
            }
        }
        [LocDisplayName("Green")]
        [SyncToView, Autogen, AutoRPC]
        [Range(0, 1)]
        public float G
        {
            get => g; set
            {
                float newValue = Math.Clamp(value, 0, 1);
                if (g == newValue) return;
                g = newValue;
                OnUserInput();
            }
        }
        [LocDisplayName("Blue")]
        [SyncToView, Autogen, AutoRPC]
        [Range(0, 1)]
        public float B
        {
            get => b; set
            {
                float newValue = Math.Clamp(value, 0, 1);
                if (b == newValue) return;
                b = newValue;
                OnUserInput();
            }
        }
        private string ToHex()
        {
            return ColorUtility.RGBHex(new Color(R, G, B).HexRGBA);
        }

        private float r = 1;
        private float g = 1;
        private float b = 1;

        public IHasModelPartColourComponent Model { get; private set; }

        public void SetModel(IHasModelPartColourComponent component)
        {
            Model?.ColourData.Unsubscribe(nameof(ModelPartColouring.Colour), OnModelChanged);
            Model = component;
            Model?.ColourData.SubscribeAndCall(nameof(ModelPartColouring.Colour), OnModelChanged);
        }
        /// <summary>
        /// Update the model with the colour values in the view
        /// </summary>
        private void OnUserInput()
        {
            SetColour(new Color(R, G, B));
        }
        /// <summary>
        /// Update the view with the model colour
        /// </summary>
        private void OnModelChanged()
        {
            Log.WriteLine(Localizer.DoStr("ColouredPartViewController.OnModelChanged"));
            if (Model == null) return;
            r = Model.ColourData.Colour.R;
            g = Model.ColourData.Colour.G;
            b = Model.ColourData.Colour.B;
            this.Changed(nameof(NameDisplay));
            this.Changed(nameof(ColourHex));
            this.Changed(nameof(R));
            this.Changed(nameof(G));
            this.Changed(nameof(B));
        }
        private void SetColour(Color colour)
        {
            if (Model == null) return;
            ModelPartColouring partColouring = Model.ColourData;
            if (partColouring == null) return;
            partColouring.Colour = colour;
        }

        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
