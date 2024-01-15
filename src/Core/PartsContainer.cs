using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    [Serialized]
    public class PartsContainer : IController, INotifyPropertyChanged, IClearRequestHandler
    {
        [Serialized] public string Name { get; set; } = "Serialized Name";
        //[SyncToView, NewTooltipChildren(Eco.Shared.Items.CacheAs.Instance)]
        public IReadOnlyList<IPart> Parts => Slots.SelectNonNull(slot => slot.Inventory.Stacks.FirstOrDefault()?.Item).OfType<IPart>().ToList();
        [Serialized]
        private ThreadSafeList<Slot> slots = new ThreadSafeList<Slot> ();
        [SyncToView, NewTooltip(Eco.Shared.Items.CacheAs.Instance)]//, TooltipAffectedBy(nameof(OnPartChanged))]
        public LocString CurrentPartsListDescription()
        {
            LocString partsListDescription = Parts.Select(part =>
            {
                return (part as ILinkable)?.UILink() ?? part.UILinkGeneric();
            }).NewlineList();
            return Localizer.DoStr("Contains parts:").AppendLine(partsListDescription);
        }

        public IReadOnlyList<Slot> Slots => slots.Snapshot.AsReadOnly();
        [Notify] public ThreadSafeAction<Slot> OnPartChanged { get; } = new ThreadSafeAction<Slot> ();
        public ISlotRestrictionManager SlotRestrictionManager { get; set; }
        public void AddPart(Slot slot, IPart part)
        {
            slot.TryAddPart(part);
            slots.Add(slot);
        }
        public void RemovePart(Slot slot)
        {
            int index = slots.IndexOf(slot);
            if (index >= 0)
            {
                slots.RemoveAt(index);
            }
        }

        public void Initialize(WorldObject worldObject)
        {
            Log.WriteLine(Localizer.DoStr($"Initialize parts container {Name} with object {worldObject?.Name}"));

            foreach (Slot slot in Slots)
            {
                slot.Initialize(worldObject, this);
                slot.OnPartChanged.Add(() => OnPartChanged.Invoke(slot));
            }
        }

        public Result TryHandleClearRequest(Player player)
        {
            return Result.Fail(Localizer.DoStr("Cannot delete slots"));
        }
        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
