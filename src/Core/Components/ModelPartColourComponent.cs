using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Objects;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parts
{
    /// <summary>
    /// WorldObjectComponent to send all part colours to the client
    /// </summary>
    [Serialized]
    [NoIcon]
    [RequireComponent(typeof(PartsContainerComponent))]
    public class ModelPartColourComponent : WorldObjectComponent
    {
        private IDictionary<ISlot, ModelColourSetterViewController> partViews = new ThreadSafeDictionary<ISlot, ModelColourSetterViewController>();
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
            if (!partViews.TryGetValue(slot, out ModelColourSetterViewController viewForSlot)) return;

            IHasModelPartColour colourComponent = slot.Part as IHasModelPartColour;
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
                IHasModelPartColour partColourComponent = part as IHasModelPartColour;

                ModelColourSetterViewController partView = new ModelColourSetterViewController();
                partView.SetModel(Parent, partColourComponent);
                partViews.Add(slot, partView);
            }
        }
    }
}
