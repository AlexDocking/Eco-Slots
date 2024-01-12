using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System.ComponentModel;

namespace Parts
{
    [Serialized]
    public class ModelPartColouring : IController, INotifyPropertyChanged
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
            }
        }

        public ModelPartColouring()
        {
            PropertyChanged += OnPropertyChanged;
        }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            Log.WriteLine(Localizer.DoStr("Detected change in " + args.PropertyName));
        }
        #region IController
        private int id;
        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        public ref ThreadSafeSubscriptions Subscriptions => ref this.subscriptions; ThreadSafeSubscriptions subscriptions;
        private Color colour;
    }
}
