using Eco.Gameplay.Components.Auth;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parts;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Eco.Core.Controller;
using Eco.Core.Utils;
using System.ComponentModel;
using Eco.Shared.Items;
using Eco.Gameplay.Systems.NewTooltip;
using System.IO;
using Eco.Gameplay.Housing.PropertyValues;
using Eco.Mods.TechTree;
using Eco.Core.Systems;
using Eco.Core.PropertyHandling;

namespace KitchenUnits
{
    [NoIcon]
    [Serialized]
    [RequireComponent(typeof(PropertyAuthComponent))]
    [RequireComponent(typeof(PartsContainerComponent))]
    [RequireComponent(typeof(ModelPartColourComponent))]
    [RequireComponent(typeof(PartColoursUIComponent))]
    [RequireComponent(typeof(PartSlotsUIComponent))]
    [RequireComponent(typeof(ModelReplacerComponent))]
    public class KitchenCupboardObject : WorldObject, IRepresentsItem, IThreadSafeSubscriptions
    {
        public override LocString DisplayName => Localizer.DoStr("Kitchen Cupboard");

        public Type RepresentedItemType => typeof(KitchenCupboardItem);
        protected override void Initialize()
        {
            base.Initialize();
            bool isNewWorldObject = GetComponent<PartsContainerComponent>().PartsContainer == null;
            PartsContainer partsContainer = GetComponent<PartsContainerComponent>().PartsContainer;
            if (isNewWorldObject)
            {
                partsContainer = new PartsContainer();
                GetComponent<PartsContainerComponent>().PartsContainer = partsContainer;
                partsContainer.AddPart(new Slot(), new KitchenCupboardUnitItem());
                partsContainer.AddPart(new Slot(), new KitchenCupboardWorktopItem());
                partsContainer.AddPart(new Slot(), null);
            }
            IReadOnlyList<Slot> slots = partsContainer.Slots;
            slots[0].Name = "Unit";
            slots[1].Name = "Worktop";
            slots[2].Name = "Door";
            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();
            slotRestrictionManager.SetTypeRestriction(slots[0], new[] { typeof(KitchenCupboardUnitItem) });
            slotRestrictionManager.SetOptional(slots[0], false);

            slotRestrictionManager.SetTypeRestriction(slots[1], new[] { typeof(KitchenCupboardWorktopItem) });
            slotRestrictionManager.SetOptional(slots[1], false);

            slotRestrictionManager.SetTypeRestriction(slots[2], new[] { typeof(KitchenCupboardFlatDoorItem), typeof(KitchenCupboardShakerDoorItem) });
            slotRestrictionManager.SetOptional(slots[2], true);

            partsContainer.SlotRestrictionManager = slotRestrictionManager;
            partsContainer.Initialize(this);

            ModTooltipLibrary.CurrentPartsListDescription(partsContainer);
        }
    }

    [Serialized]
    [LocDisplayName("Kitchen Cupboard")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class KitchenCupboardItem : WorldObjectItem<KitchenCupboardObject>, IPersistentData
    {
        [Serialized, SyncToView, NewTooltipChildren(CacheAs.Instance, flags: TTFlags.AllowNonControllerTypeForChildren)]
        public object PersistentData { get; set; }
    }

    [Serialized]
    [LocDisplayName("Kitchen Cupboard Unit")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class KitchenCupboardUnitItem : Item, IPart, IHasModelPartColourComponent
    {
        public KitchenCupboardUnitItem() : base()
        {
            ColourData.ModelName = "Unit";
        }
        private ModelPartColouring colourData = new ModelPartColouring();
        [Serialized] public ModelPartColouring ColourData
        {
            get => colourData; set
            {
                colourData = value;
                colourData.ModelName = "Unit";
            }
        }

        string IPart.DisplayName => "Cupboard Unit";

        public bool CanStack(Item stackingOntoItem)
        {
            if (stackingOntoItem?.GetType() != typeof(KitchenCupboardUnitItem)) return false;
            KitchenCupboardUnitItem kitchenCupboardItem = (KitchenCupboardUnitItem)stackingOntoItem;
            return kitchenCupboardItem.ColourData.Colour == ColourData.Colour;
        }
    }
    [Serialized]
    [LocDisplayName("Kitchen Cupboard Flat Door")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class KitchenCupboardFlatDoorItem : Item, IPart, IHasModelPartColourComponent
    {
        public KitchenCupboardFlatDoorItem() : base()
        {
            ColourData.ModelName = "Door";
        }
        private ModelPartColouring colourData = new ModelPartColouring();
        [Serialized]
        public ModelPartColouring ColourData
        {
            get => colourData; set
            {
                colourData = value;
                colourData.ModelName = "Door";
            }
        }

        string IPart.DisplayName => "Flat Door";
    }
    [Serialized]
    [LocDisplayName("Kitchen Cupboard Shaker Door")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class KitchenCupboardShakerDoorItem : Item, IPart, IHasModelPartColourComponent
    {
        public KitchenCupboardShakerDoorItem() : base()
        {
            ColourData.ModelName = "Door";
        }
        private ModelPartColouring colourData = new ModelPartColouring();
        [Serialized]
        public ModelPartColouring ColourData
        {
            get => colourData; set
            {
                colourData = value;
                colourData.ModelName = "Door";
            }
        }
        string IPart.DisplayName => "Shaker Door";
    }
    [Serialized]
    [LocDisplayName("Kitchen Cupboard Worktop")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class KitchenCupboardWorktopItem : Item, IPart, IHasModelPartColourComponent
    {
        public KitchenCupboardWorktopItem() : base()
        {
            ColourData.ModelName = "Worktop";
        }
        private ModelPartColouring colourData = new ModelPartColouring();
        [Serialized]
        public ModelPartColouring ColourData
        {
            get => colourData; set
            {
                colourData = value;
                colourData.ModelName = "Worktop";
            }
        }
        string IPart.DisplayName => "Cupboard Worktop";
    }

    [Serialized]
    [LocDisplayName("Test 1")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class TestColourItem : Item, IHasSerializableID
    {
        public ModelPartColouring ColourData { get; set; } = new ModelPartColouring();
    }
    [Serialized]
    [LocDisplayName("Test 2")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class TestColour2Item : Item, IHasSerializableID
    {
        [SyncToView] public ModelPartColouring ColourData { get; set; } = new ModelPartColouring();
    }
    [Serialized]
    [LocDisplayName("Test 3")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class TestColour3Item : Item, IHasSerializableID
    {
        [Serialized, SyncToView, Notify] public ModelPartColouring ColourData { get; set; } = new ModelPartColouring();
        [NewTooltip(CacheAs.Instance), TooltipAffectedBy(nameof(ColourData), nameof(ModelPartColouring.Colour))] public string ColourTooltip() => ColorUtility.RGBHex(ColourData.Colour.HexRGBA);

    }
    [Serialized]
    [LocDisplayName("Test 4")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class TestColour4Item : Item, IHasSerializableID
    {
        public ModelPartColouring ColourData = new ModelPartColouring();
    }
    [Serialized]
    [LocDisplayName("Test 5")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class TestColour5Item : Item, IHasSerializableID
    {
    }
    [Serialized]
    [LocDisplayName("Test 6")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class TestColour6Item : Item, IHasSerializableID
    {
        public string Test { get; set; } = "Test String";
    }
    [Serialized]
    [LocDisplayName("Test 7")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class TestColour7Item : Item, IHasSerializableID
    {
        [SyncToView] public string Test { get; set; } = "Test String";

    }
    [Serialized]
    [LocDisplayName("Test 8")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class TestColour8Item : Item, IHasSerializableID
    {
        [Serialized] public string Test { get; set; } = "Test String";
    }
    [Serialized]
    [LocDisplayName("Test 9")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class TestColour9Item : Item, IHasSerializableID
    {
        public HomeFurnishingValue Test => Item.Get<LumberDresserItem>().HomeValue;
    }
    [Serialized]
    [LocDisplayName("Test 10")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class TestColour10Item : Item, IHasSerializableID
    {
        [SyncToView] public HomeFurnishingValue Test => Item.Get<LumberDresserItem>().HomeValue;
    }
    [Serialized]
    [LocDisplayName("Test 11")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class TestColour11Item : Item, IHasSerializableID
    {
        [NewTooltip(CacheAs.Instance)] public HomeFurnishingValue Test => Item.Get<LumberDresserItem>().HomeValue;
    }
}
