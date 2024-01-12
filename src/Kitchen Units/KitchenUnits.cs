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

namespace KitchenUnits
{
    [NoIcon]
    [Serialized]
    [RequireComponent(typeof(PropertyAuthComponent))]
    [RequireComponent(typeof(PartsContainerComponent))]
    [RequireComponent(typeof(ModelPartColourComponent))]
    [RequireComponent(typeof(PartColoursUIComponent))]
    public class KitchenCupboardObject : WorldObject, IRepresentsItem, IThreadSafeSubscriptions
    {
        public Type RepresentedItemType => typeof(KitchenCupboardItem);
        protected override void Initialize()
        {
            base.Initialize();
            PartsContainer partsContainer = GetComponent<PartsContainerComponent>().PartsContainer;
            if (partsContainer.Parts.Count != 3)
            {
                Log.WriteLine(Localizer.DoStr("Adding parts"));
                partsContainer.AddPart(new Slot(), new KitchenCupboardUnitItem());
                partsContainer.AddPart(new Slot(), new KitchenCupboardDoorItem());
                partsContainer.AddPart(new Slot(), new KitchenCupboardWorktopItem());
            }
        }
    }

    [Serialized]
    public class KitchenCupboardItem : WorldObjectItem<KitchenCupboardObject>, IPersistentData
    {
        [Serialized, NewTooltip(CacheAs.Instance)]
        public object PersistentData { get; set; }
    }

    [Serialized]
    [LocDisplayName("Kitchen Cupboard Unit")]
    public class KitchenCupboardUnitItem : Item, IUniqueStackable, IPart, IHasModelPartColourComponent
    {
        public KitchenCupboardUnitItem() : base()
        {
            ColourData.ModelName = "Unit";
        }
        [Serialized] public ModelPartColouring ColourData { get; set; } = new ModelPartColouring();
        [SyncToView] public string Colour => ColorUtility.RGBHex(ColourData.Colour.HexRGBA);

        string IPart.DisplayName => "Unit";

        public bool CanStack(Item stackingOntoItem)
        {
            if (stackingOntoItem?.GetType() != typeof(KitchenCupboardUnitItem)) return false;
            KitchenCupboardUnitItem kitchenCupboardItem = (KitchenCupboardUnitItem)stackingOntoItem;
            return kitchenCupboardItem.ColourData.Colour == ColourData.Colour;
        }
    }
    [Serialized]
    [LocDisplayName("Kitchen Cupboard Door")]
    public class KitchenCupboardDoorItem : Item, IPart, IHasModelPartColourComponent
    {
        public KitchenCupboardDoorItem() : base()
        {
            ColourData.ModelName = "Door";
        }

        [Serialized] public ModelPartColouring ColourData { get; private set; } = new ModelPartColouring();
        //[SyncToView, LocDisplayName("Colour"), DependsOnSubMember(nameof(ColourData), nameof(ModelPartColouring.Colour))] public string Colour => ColorUtility.RGBHex(ColourData.Colour.HexRGBA);

        string IPart.DisplayName => "Door";
    }
    [Serialized]
    [LocDisplayName("Kitchen Cupboard Worktop")]
    public class KitchenCupboardWorktopItem : Item, IPart, IHasModelPartColourComponent
    {
        public KitchenCupboardWorktopItem() : base()
        {
            ColourData.ModelName = "Worktop";
        }

        [Serialized] public ModelPartColouring ColourData { get; private set; } = new ModelPartColouring();
        //[SyncToView, LocDisplayName("Colour"), DependsOnSubMember(nameof(ColourData), nameof(ModelPartColouring.Colour))] public string Colour => ColorUtility.RGBHex(ColourData.Colour.HexRGBA);

        string IPart.DisplayName => "Worktop";
    }
}
