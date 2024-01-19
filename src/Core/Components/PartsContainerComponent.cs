using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using Parts.Migration;
using System.Collections.Generic;
using System.Linq;

namespace Parts
{
    [Serialized]
    [NoIcon]
    [Priority(PriorityAttribute.High)]
    public class PartsContainerComponent : WorldObjectComponent, IPersistentData
    {
        [Serialized, SyncToView, NewTooltipChildren(Eco.Shared.Items.CacheAs.Instance)]
        public IPartsContainer PartsContainer
        {
            get
            {
                return partsContainer;
            }
            private set
            {
                partsContainer = value;
            }
        }

        private IPartsContainer partsContainer;
        public override WorldObjectComponentClientAvailability Availability => WorldObjectComponentClientAvailability.Always;
        private IPartsContainerSchema Schema => (Parent as IPartsContainerWorldObject)?.GetPartsContainerSchema();

        public override void Initialize()
        {
            base.Initialize();
            EnsureInitialized();
        }
        private bool initialized = false;
        /// <summary>
        /// Initialize the parts container.
        /// </summary>
        public void EnsureInitialized()
        {
            if (initialized) return;
            partsContainer ??= PartsContainerFactory.Create();
            partsContainer = Schema?.Migrate(Parent, partsContainer) ?? partsContainer;
            partsContainer.Initialize(Parent);

            initialized = true;
        }
        public object PersistentData
        {
            get => PartsContainer; set => PartsContainer ??= value as IPartsContainer;
        }
    }
}
