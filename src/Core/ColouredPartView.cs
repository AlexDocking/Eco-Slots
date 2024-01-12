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
    public class ColouredPartView : IController, INotifyPropertyChanged
    {
        [SyncToView, Autogen]
        [UITypeName("GeneralHeader")]
        public string NameDisplay => Component.DisplayName;

        [SyncToView, Autogen, AutoRPC]
        public string ColourHex
        {
            get => ToHex(); set
            {
                Color colour = new Color(ColorUtility.RGBHex(value));
                r = colour.R;
                g = colour.G;
                b = colour.B;
                SyncToModel();
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
                SyncToModel();
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
                SyncToModel();
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
                SyncToModel();
            }
        }
        private string ToHex()
        {
            return ColorUtility.RGBHex(new Color(R, G, B).HexRGBA);
        }

        private float r = 1;
        private float g = 1;
        private float b = 1;

        public IHasModelPartColourComponent Component { get; }

        public ColouredPartView(IHasModelPartColourComponent component)
        {
            Component = component;
            r = Component.ColourData.Colour.R;
            g = Component.ColourData.Colour.G;
            b = Component.ColourData.Colour.B;
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            Log.WriteLine(Localizer.DoStr("Property Changed " + Component.ToString()));
            SetColour(new Color(R, G, B));
        }
        /// <summary>
        /// Update the model with the colour values in the view
        /// </summary>
        public void SyncToModel()
        {
            SetColour(new Color(R, G, B));

            this.Changed(nameof(ColourHex));
            this.Changed(nameof(R));
            this.Changed(nameof(G));
            this.Changed(nameof(B));
        }
        private void SetColour(Color colour)
        {
            ModelPartColouring partColouring = Component.ColourData;
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
