using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Utils;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using Parts.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    /// <summary>
    /// Autogen UI component for displaying and setting the colour on any parts which has colour data.
    /// TODO: refactor this and <see cref="SlotsUIComponent"/> since they are practically identical in how they work.
    /// </summary>
    [Serialized]
    [AutogenClass]
    [CreateComponentTabLoc("Part Colours")]
    [NoIcon]
    [RequireComponent(typeof(ModelPartColourComponent))]
    public class PartColoursUIComponent : WorldObjectComponent, IHasClientControlledContainers, INotifyPropertyChanged
    {
        private IList<(ISlot, ColouredPartController)> partViews = new ThreadSafeList<(ISlot, ColouredPartController)>();
        private IEnumerable<ColouredPartController> Viewers => partViews.SelectNonNull(pair => pair.Item2.Model != null ? pair.Item2 : null);
        [Autogen, SyncToView, HideRoot, HideRootListEntry]
        public ControllerList<ColouredPartController> PartsUI { get; private set; }
        
        private IPartsContainer PartsContainer { get; set; }
        public PartColoursUIComponent()
        {
            PartsUI = new ControllerList<ColouredPartController>(this, nameof(PartsUI), Array.Empty<ColouredPartController>());
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
                IColouredPart colourComponent = slot.Part as IColouredPart;
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
                IColouredPart partColourComponent = part as IColouredPart;
                ColouredPartController partView = new ColouredPartController();
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
