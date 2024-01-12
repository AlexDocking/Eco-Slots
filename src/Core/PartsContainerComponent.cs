using Eco.Core.Controller;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Parts
{
    [Serialized]
    [NoIcon]
    public class PartsContainerComponent : WorldObjectComponent, IPersistentData
    {
        public override WorldObjectComponentClientAvailability Availability => WorldObjectComponentClientAvailability.Always;

        [Serialized, SyncToView, NewTooltipChildren(Eco.Shared.Items.CacheAs.Disabled)]
        public PartsContainer PartsContainer { get; set; } = new PartsContainer();
        public object PersistentData
        {
            get => PartsContainer; set
            {
                PartsContainer = value as PartsContainer ?? new PartsContainer();
                Log.WriteLine(Localizer.DoStr($"Deserialized persistent data. Null? {(value as PartsContainer) == null}"));
            }
        }
        public override void Initialize()
        {
            IReadOnlyList<Slot> slots = PartsContainer.Slots;
            IReadOnlyList<IPart> parts = PartsContainer.Parts;
            Log.WriteLine(Localizer.DoStr($"Slots {slots.Count}, parts {parts.Count}"));
            for (int i = 0; i < parts.Count; i++)
            {
                Log.WriteLine(Localizer.DoStr($"Slot {i}: {slots[i].Inventory.NonEmptyStacks.FirstOrDefault()?.Item.Name}"));
                Log.WriteLine(Localizer.DoStr($"Part {i}: {parts[i].DisplayName}"));
            }
        }
    }
}
