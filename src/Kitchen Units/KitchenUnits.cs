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
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;

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
            }
            EnsureSlotsHaveCorrectParts(partsContainer);

            IReadOnlyList<Slot> slots = partsContainer.Slots;
            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();
            slotRestrictionManager.SetTypeRestriction(slots[0], new[] { typeof(KitchenBaseCabinetBoxItem) });
            slotRestrictionManager.SetOptional(slots[0], false);

            slotRestrictionManager.SetTypeRestriction(slots[1], new[] { typeof(KitchenCupboardWorktopItem) });
            slotRestrictionManager.SetOptional(slots[1], false);

            slotRestrictionManager.SetTypeRestriction(slots[2], new[] { typeof(KitchenCabinetFlatDoorItem), typeof(KitchenCupboardRaisedPanelDoorItem) });
            slotRestrictionManager.SetOptional(slots[2], true);

            partsContainer.SlotRestrictionManager = slotRestrictionManager;
            partsContainer.Initialize(this);

            ModTooltipLibrary.CurrentPartsListDescription(partsContainer);
        }

        private static void EnsureSlotsHaveCorrectParts(PartsContainer partsContainer)
        {
            IReadOnlyList<Slot> slots = partsContainer.Slots;
            for (int i = 0; i < 3 - slots.Count; i++)
            {
                partsContainer.AddPart(new Slot(), null);
            }
            IPart preexistingPart0 = slots[0].Inventory.Stacks?.FirstOrDefault()?.Item as IPart;
            IPart preexistingPart1 = slots[1].Inventory.Stacks?.FirstOrDefault()?.Item as IPart;

            if (preexistingPart0 is not KitchenBaseCabinetBoxItem)
            {
                KitchenBaseCabinetBoxItem newBox = new KitchenBaseCabinetBoxItem();
                if (slots[0].Part is IHasModelPartColourComponent colourComponent) newBox.ColourData.Colour = colourComponent.ColourData.Colour;
                slots[0].Inventory.Stacks.First().Item = newBox;
            }

            if (preexistingPart1 is not KitchenCupboardWorktopItem)
            {
                KitchenCupboardWorktopItem newWorktop = new KitchenCupboardWorktopItem();
                if (preexistingPart1 is IHasModelPartColourComponent colourComponent) newWorktop.ColourData.Colour = colourComponent.ColourData.Colour;
                slots[1].Inventory.Stacks.First().Item = newWorktop;
            }
            Log.WriteLine(Localizer.DoStr($"Existing 0:{preexistingPart0?.GetType()} {slots[0].Part?.GetType()}"));
            Log.WriteLine(Localizer.DoStr($"Existing 1:{preexistingPart1?.GetType()} {slots[1].Part?.GetType()}"));

            slots[0].Name = "Unit";
            slots[1].Name = "Worktop";
            slots[2].Name = "Door";
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
    [LocDisplayName("Kitchen Base Cabinet Box")]
    [LocDescription("The sides, base and shelves of a kitchen cabinet that sits on the floor.")]
    public class KitchenBaseCabinetBoxItem : Item, IPart, IHasModelPartColourComponent
    {
        public KitchenBaseCabinetBoxItem() : base()
        {
            ColourData = colourData;
        }
        private ModelPartColouring colourData = new ModelPartColouring();
        [Serialized] public ModelPartColouring ColourData
        {
            get => colourData; set
            {
                if (colourData != null) colourData.PropertyChanged -= OnModelPartColouringChanged;
                colourData = value;
                colourData.ModelName = "Unit";
                if (colourData != null) colourData.PropertyChanged += OnModelPartColouringChanged;
            }
        }
        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();
        private void OnModelPartColouringChanged(object sender, PropertyChangedEventArgs args)
        {
            PartPropertyChangedEvent.Invoke(this, ColourData);
            PartNotifications.PartPropertyChangedEventGlobal.Invoke(this, ColourData);
        }
        string IPart.DisplayName => "Cabinet Box";
    }
    [Serialized]
    [LocDisplayName("Kitchen Cabinet Flat Door")]
    [LocDescription("Completely flat.")]
    public class KitchenCabinetFlatDoorItem : Item, IPart, IHasModelPartColourComponent
    {
        public KitchenCabinetFlatDoorItem() : base()
        {
            ColourData = colourData;
        }
        private ModelPartColouring colourData = new ModelPartColouring();
        [Serialized]
        public ModelPartColouring ColourData
        {
            get => colourData; set
            {
                if (colourData != null) colourData.PropertyChanged -= OnModelPartColouringChanged;
                colourData = value;
                colourData.ModelName = "Door";
                if (colourData != null) colourData.PropertyChanged += OnModelPartColouringChanged;
            }
        }
        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();
        private void OnModelPartColouringChanged(object sender, PropertyChangedEventArgs args)
        {
            PartPropertyChangedEvent.Invoke(this, ColourData);
            PartNotifications.PartPropertyChangedEventGlobal.Invoke(this, ColourData);
        }

        string IPart.DisplayName => "Flat Door";
    }
    [Serialized]
    [LocDisplayName("Kitchen Cupboard Raised Panel Door")]
    [LocDescription("A cupboard door with a raised panel in the centre.")]
    public class KitchenCupboardRaisedPanelDoorItem : Item, IPart, IHasModelPartColourComponent
    {
        public KitchenCupboardRaisedPanelDoorItem() : base()
        {
            ColourData = colourData;
        }
        private ModelPartColouring colourData = new ModelPartColouring();
        [Serialized]
        public ModelPartColouring ColourData
        {
            get => colourData; set
            {
                if (colourData != null) colourData.PropertyChanged -= OnModelPartColouringChanged;
                colourData = value;
                colourData.ModelName = "Door";
                if (colourData != null) colourData.PropertyChanged += OnModelPartColouringChanged;
            }
        }
        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();
        private void OnModelPartColouringChanged(object sender, PropertyChangedEventArgs args)
        {
            PartPropertyChangedEvent.Invoke(this, ColourData);
            PartNotifications.PartPropertyChangedEventGlobal.Invoke(this, ColourData);
        }
        string IPart.DisplayName => "Raised Panel Door";
    }
    [Serialized]
    [LocDisplayName("Kitchen Cupboard Worktop")]
    [LocDescription("A fancy ashlar stone chair that has been adorned with gold. A throne fit for a king.")]
    public class KitchenCupboardWorktopItem : Item, IPart, IHasModelPartColourComponent
    {
        public KitchenCupboardWorktopItem() : base()
        {
            ColourData = colourData;
        }
        private ModelPartColouring colourData = new ModelPartColouring();
        [Serialized]
        public ModelPartColouring ColourData
        {
            get => colourData; set
            {
                if (colourData != null) colourData.PropertyChanged -= OnModelPartColouringChanged;
                colourData = value;
                colourData.ModelName = "Worktop";
                if (colourData != null) colourData.PropertyChanged += OnModelPartColouringChanged;
            }
        }
        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();
        private void OnModelPartColouringChanged(object sender, PropertyChangedEventArgs args)
        {
            PartPropertyChangedEvent.Invoke(this, ColourData);
            PartNotifications.PartPropertyChangedEventGlobal.Invoke(this, ColourData);
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
