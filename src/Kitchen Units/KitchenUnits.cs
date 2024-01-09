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
using Eco.Gameplay.Utils;
using System.ComponentModel;
using Eco.Shared.Networking;
using Eco.Gameplay.Components;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Gameplay.Systems;
using Eco.Gameplay.Modules;
using Eco.Gameplay.DynamicValues;
using Eco.Core.Items;
using Eco.Mods.TechTree;
using Eco.Gameplay.Systems.TextLinks;

namespace KitchenUnits
{

    
    [NoIcon]
    [Serialized]
    [RequireComponent(typeof(PropertyAuthComponent))]
    [RequireComponent(typeof(PartsContainerComponent))]
    [RequireComponent(typeof(ModelPartColourComponent))]
    [RequireComponent(typeof(PartColoursUIComponent))]
    public class KitchenCupboardObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(KitchenCupboardItem);
        protected override void Initialize()
        {
            base.Initialize();
            PartsContainer partsContainer = GetComponent<PartsContainerComponent>().PartsContainer;
            if (partsContainer.Parts.Count != 3)
            {
                Log.WriteLine(Localizer.DoStr("Adding parts"));
                partsContainer.AddPart(new Slot(), new KitchenCupboardUnit());
                partsContainer.AddPart(new Slot(), new KitchenCupboardDoor());
                partsContainer.AddPart(new Slot(), new KitchenCupboardWorktop());
            }

        }
    }

    [Serialized]
    public class KitchenCupboardItem : WorldObjectItem<KitchenCupboardObject>
    {
    }

    [Serialized]
    public class KitchenCupboardUnit : Part
    {
        public KitchenCupboardUnit()
        {
            Name = "KitchenCupboardUnit";
            DisplayName = "Unit";
            SetAttribute("PartColouring", new ModelPartColouring()
            {
                ModelName = "Unit",
            });
        }
    }
    [Serialized]
    public class KitchenCupboardDoor : Part
    {
        public KitchenCupboardDoor()
        {
            Name = "KitchenCupboardDoor";
            DisplayName = "Door";
            SetAttribute("PartColouring", new ModelPartColouring()
            {
                ModelName = "Door",
            });
        }
    }
    [Serialized]
    public class KitchenCupboardWorktop : Part
    {
        public KitchenCupboardWorktop()
        {
            Name = "KitchenCupboardWorktop";
            DisplayName = "Worktop";
            SetAttribute("PartColouring", new ModelPartColouring()
            {
                ModelName = "Worktop",
            });
        }
    }
}
