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
        public override LocString DisplayName => Localizer.DoStr("Kitchen Base Cabinet");

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
    [LocDisplayName("Kitchen Base Cabinet")]
    [LocDescription("A kitchen cabinet that sits on the floor.")]
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
    [LocDescription("Completely flat on all sides, for a modern feel.")]
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
    [LocDisplayName("Kitchen Cabinet Raised Panel Door")]
    [LocDescription("A cabinet door with a raised panel in the centre.")]
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
    [LocDisplayName("Kitchen Cabinet Worktop")]
    [LocDescription("A surface to prepare meals on.")]
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
        string IPart.DisplayName => "Worktop";
    }
}
