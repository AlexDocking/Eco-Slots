using Eco.Gameplay.Components.Auth;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eco.Shared.Localization;
using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Shared.Items;
using Eco.Gameplay.Systems.NewTooltip;
using Parts.Migration;
using Parts.UI;

namespace Parts.Kitchen
{
    [NoIcon]
    [Serialized]
    [RequireComponent(typeof(PropertyAuthComponent))]
    [RequireComponent(typeof(PartsContainerComponent))]
    [RequireComponent(typeof(ModelPartColourComponent))]
    [RequireComponent(typeof(PartColoursUIComponent))]
    [RequireComponent(typeof(SlotsUIComponent))]
    [RequireComponent(typeof(ModelReplacerComponent))]
    public class KitchenCupboardObject : WorldObject, IRepresentsItem, IThreadSafeSubscriptions
    {
        public override LocString DisplayName => Localizer.DoStr("Kitchen Base Cabinet");

        public Type RepresentedItemType => typeof(KitchenCupboardItem);
    }
    [Serialized]
    [LocDisplayName("Kitchen Base Cabinet")]
    [LocDescription("A kitchen cabinet that sits on the floor.")]
    public class KitchenCupboardItem : WorldObjectItem<KitchenCupboardObject>, IPersistentData, IPartsContainerWorldObjectItem
    {
        [Serialized, SyncToView, NewTooltipChildren(CacheAs.Instance, flags: TTFlags.AllowNonControllerTypeForChildren)]
        public object PersistentData { get; set; }
        public IPartsContainerMigrator GetPartsContainerMigrator() => new KitchenBaseCabinetMigrator();
    }

    [Serialized]
    [LocDisplayName("Kitchen Base Cabinet Box")]
    [LocDescription("The sides, base and shelves of a kitchen cabinet that sits on the floor.")]
    public class KitchenBaseCabinetBoxItem : Item, IPart, IColouredPart
    {
        public KitchenBaseCabinetBoxItem() : base()
        {
            ColourData = colourData;
        }
        private ModelPartColourData colourData = new ModelPartColourData();
        [Serialized]
        public ModelPartColourData ColourData
        {
            get => colourData; set
            {
                ModelPartColourData newData = value;
                newData.ModelName = "Unit";
                this.SetProperty(newData, ref colourData);
            }
        }
        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();
        string IPart.DisplayName => "Cabinet Box";
    }
    [Serialized]
    [LocDisplayName("Kitchen Cabinet Flat Door")]
    [LocDescription("Completely flat on all sides, for a modern feel.")]
    public class KitchenCabinetFlatDoorItem : Item, IPart, IColouredPart
    {
        public KitchenCabinetFlatDoorItem() : base()
        {
            ColourData = colourData;
        }
        private ModelPartColourData colourData = new ModelPartColourData();
        [Serialized]
        public ModelPartColourData ColourData
        {
            get => colourData; set
            {
                ModelPartColourData newData = value;
                newData.ModelName = "Door";
                this.SetProperty(newData, ref colourData);
            }
        }
        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();
        string IPart.DisplayName => "Flat Door";
    }
    [Serialized]
    [LocDisplayName("Kitchen Cabinet Raised Panel Door")]
    [LocDescription("A cabinet door with a raised panel in the centre.")]
    public class KitchenCupboardRaisedPanelDoorItem : Item, IPart, IColouredPart
    {
        public KitchenCupboardRaisedPanelDoorItem() : base()
        {
            ColourData = colourData;
        }
        private ModelPartColourData colourData = new ModelPartColourData();
        [Serialized]
        public ModelPartColourData ColourData
        {
            get => colourData; set
            {
                ModelPartColourData newData = value;
                newData.ModelName = "Door";
                this.SetProperty(newData, ref colourData);
            }
        }
        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();
        string IPart.DisplayName => "Raised Panel Door";
    }
    [Serialized]
    [LocDisplayName("Kitchen Cabinet Worktop")]
    [LocDescription("A surface to prepare meals on.")]
    public class KitchenCupboardWorktopItem : Item, IPart, IColouredPart
    {
        public KitchenCupboardWorktopItem() : base()
        {
            ColourData = colourData;
        }
        private ModelPartColourData colourData = new ModelPartColourData();
        [Serialized]
        public ModelPartColourData ColourData
        {
            get => colourData; set
            {
                ModelPartColourData newData = value;
                newData.ModelName = "Worktop";
                this.SetProperty(newData, ref colourData);
            }
        }
        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();
        string IPart.DisplayName => "Worktop";
    }
}
