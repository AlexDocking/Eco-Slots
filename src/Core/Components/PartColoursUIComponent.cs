using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using Eco.Shared.Pools;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    [Serialized]
    [AutogenClass]
    [CreateComponentTabLoc("Part Colours")]
    [NoIcon]
    [RequireComponent(typeof(ModelPartColourComponent))]
    public class PartColoursUIComponent : WorldObjectComponent, IHasClientControlledContainers, INotifyPropertyChanged
    {
        private IList<(ISlot, ColouredPartViewController)> partViews = new ThreadSafeList<(ISlot, ColouredPartViewController)>();
        private IEnumerable<ColouredPartViewController> Viewers => partViews.SelectNonNull(pair => pair.Item2.Model != null ? pair.Item2 : null);
        [Autogen, SyncToView, HideRoot, HideRootListEntry]
        public ControllerList<ColouredPartViewController> PartsUI { get; private set; }
        
        private IPartsContainer PartsContainer { get; set; }
        public PartColoursUIComponent()
        {
            PartsUI = new ControllerList<ColouredPartViewController>(this, nameof(PartsUI), Array.Empty<ColouredPartViewController>());
        }
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
            PartsUI.Callbacks.OnAdd.Add(ResetList);
            PartsUI.Callbacks.OnRemove.Add(ResetList);
        }
        private void OnPartChanged(ISlot slot)
        {
            foreach(var (_, viewForSlot) in partViews.Where(s => s.Item1 == slot))
            {
                IHasModelPartColour colourComponent = slot.Part as IHasModelPartColour;
                viewForSlot.SetModel(colourComponent);
            }
            PartsUI.Set(Viewers);
            PartsUI.NotifyChanged();
        }
        private void BuildViews()
        {
            partViews.Clear();
            IReadOnlyList<ISlot> slots = PartsContainer.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                ISlot slot = slots[i];
                IPart part = slot.Part;
                IHasModelPartColour partColourComponent = (part as IHasModelPartColour);
                ColouredPartViewController partView = new ColouredPartViewController();
                partView.SetModel(partColourComponent);
                partViews.Add((slot, partView));

            }
            PartsUI.Set(Viewers);
            PartsUI.NotifyChanged();
        }
        private void ResetList(INetObject sender, object obj)
        {
            PartsUI.Set(Viewers);
            PartsUI.NotifyChanged();
        }
    }
}
