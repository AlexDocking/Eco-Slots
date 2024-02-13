using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Objects;
using Eco.Shared.Serialization;
using Parts.Effects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parts
{
    /// <summary>
    /// WorldObjectComponent to send all part colours to the client.
    /// TODO: likely refactor this with <see cref="ModelReplacerComponent"/>, and possibly <see cref="PartColoursUIComponent"/> and <see cref="Parts.UI.SlotsUIComponent"/>
    /// because they are all very similar: 'create an object for each slot that is responsible for interacting with the world in some way'.
    /// </summary>
    [Serialized]
    [NoIcon]
    [RequireComponent(typeof(PartsContainerComponent))]
    public class ModelPartColourComponent : WorldObjectComponent
    {
        private IDictionary<ISlot, ModelColourSetter> partViews = new ThreadSafeDictionary<ISlot, ModelColourSetter>();
        private IPartsContainer PartsContainer { get; set; }

        public override void Initialize()
        {
            base.Initialize();
            PartsContainer = Parent.GetComponent<PartsContainerComponent>().PartsContainer;
        }
        public override void PostInitialize()
        {
            base.PostInitialize();
            BuildViews();
            PartsContainer.NewPartInSlotEvent.Add(OnPartChanged);
        }
        private void OnPartChanged(ISlot slot)
        {
            if (!partViews.TryGetValue(slot, out ModelColourSetter viewForSlot)) return;

            IColouredPart colourComponent = slot.Part as IColouredPart;
            viewForSlot.SetModel(Parent, colourComponent);
            
        }
        private void BuildViews()
        {
            partViews.Clear();
            IReadOnlyList<ISlot> slots = PartsContainer.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                ISlot slot = slots[i];
                IPart part = slot.Part;
                IColouredPart partColourComponent = part as IColouredPart;

                ModelColourSetter partView = new ModelColourSetter();
                partView.SetModel(Parent, partColourComponent);
                partViews.Add(slot, partView);
            }
        }
    }
}
