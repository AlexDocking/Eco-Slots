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
        [Serialized, SyncToView, NewTooltipChildren(Eco.Shared.Items.CacheAs.Instance)]
        public IPartsContainer PartsContainer
        {
            get
            {
                return partsContainer;
            }
            set => partsContainer = value;
        }
        
        private IPartsContainer partsContainer;

        public override WorldObjectComponentClientAvailability Availability => WorldObjectComponentClientAvailability.Always;

        //Created by world object when first placed
        public object PersistentData
        {
            get => PartsContainer; set
            {
                PartsContainer = value as IPartsContainer ?? new PartsContainer();
            }
        }
    }
}
