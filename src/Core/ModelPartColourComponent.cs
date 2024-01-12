using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Objects;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using Eco.Shared.View;
using System.Collections.Generic;
using System.Linq;

namespace Parts
{
    [Serialized]
    [NoIcon]
    public class ModelPartColourComponent : WorldObjectComponent
    {
        private PartsContainer PartsContainer => Parent.GetComponent<PartsContainerComponent>().PartsContainer;

        private ThreadSafeList<IHasModelPartColourComponent> currentColouredParts { get; set; } = new ThreadSafeList<IHasModelPartColourComponent>();
        public override void Initialize()
        {
            base.Initialize();
            UpdateWatchedParts();
            SetModelColours();
        }
        private void UpdateWatchedParts()
        {
            lock (currentColouredParts)
            {
                IEnumerable<IHasModelPartColourComponent> newColouredParts = PartsContainer.Parts.OfType<IHasModelPartColourComponent>().ToList();

                IEnumerable<IHasModelPartColourComponent> addedParts = newColouredParts.Except(currentColouredParts);
                IEnumerable<IHasModelPartColourComponent> removedParts = currentColouredParts.Except(newColouredParts);
                foreach (IHasModelPartColourComponent part in addedParts)
                {
                    //part.ColourData.SubscribeAndCall(nameof(ModelPartColouring.Colour), SetModelColours);
                    Log.WriteLine(Localizer.DoStr("Subscribing to " + part.DisplayName));
                    part.ColourData.SubscribeAndCall(nameof(ModelPartColouring.Colour), SetModelColours);
                }
                foreach(IHasModelPartColourComponent part in removedParts)
                {
                    part.ColourData.Unsubscribe(nameof(ModelPartColouring.Colour), SetModelColours);
                }
                currentColouredParts.Set(newColouredParts);
            }

        }
        private void SetModelColours()
        {
            foreach (IHasModelPartColourComponent colouredPart in currentColouredParts)
            {
                ModelPartColouring partColouring = colouredPart.ColourData;
                Color colour = partColouring.Colour;
                Log.WriteLine(Localizer.DoStr("Send colour " + colour + " to model " + partColouring.ModelName));
                Parent.SetAnimatedState(partColouring.ModelName + "-Red", colour.R);
                Parent.SetAnimatedState(partColouring.ModelName + "-Green", colour.G);
                Parent.SetAnimatedState(partColouring.ModelName + "-Blue", colour.B);
            }
        }
    }
}
