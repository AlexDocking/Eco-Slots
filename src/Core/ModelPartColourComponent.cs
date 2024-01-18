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
        private IDictionary<Slot, ModelColourSetterViewController> partViews = new ThreadSafeDictionary<Slot, ModelColourSetterViewController>();
        private PartsContainer PartsContainer { get; set; }

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
        private void OnPartChanged(Slot slot)
        {
            if (!partViews.TryGetValue(slot, out ModelColourSetterViewController viewForSlot)) return;

            IHasModelPartColourComponent colourComponent = slot.Part as IHasModelPartColourComponent;
            viewForSlot.SetModel(Parent, colourComponent);
            
        }
        private void BuildViews()
        {
            partViews.Clear();
            IReadOnlyList<Slot> slots = PartsContainer.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                Slot slot = slots[i];
                IPart part = slot.Part;
                IHasModelPartColourComponent partColourComponent = part as IHasModelPartColourComponent;

                ModelColourSetterViewController partView = new ModelColourSetterViewController();
                partView.SetModel(Parent, partColourComponent);
                partViews.Add(slot, partView);
            }
        }
    }
}
