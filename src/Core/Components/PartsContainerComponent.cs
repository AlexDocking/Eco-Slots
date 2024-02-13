using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Shared.Serialization;
using Parts.Migration;

namespace Parts
{
    /// <summary>
    /// Allows a world object to have parts installed.
    /// No UI or specific functionality of what those parts can do is defined here.
    /// Its only responsibility is to store a reference to the parts container and ensure it is migrated and initialized.
    /// The parts container is our model in the MVC architecture.
    /// UI components and other types of view, such as disabling the object if the correct part is not installed, are kept separate to promote loose coupling and mod extensibility.
    /// </summary>
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
        public IPartsContainerMigrator Migrator { get; set; }
        public override void OnCreate()
        {
            base.OnCreate();
            //Get the migrator/schema from the world object's item. Setting this in OnCreate would allow a mod to override it before Initialize is called
            Migrator = (Parent.CreatingItem as IPartsContainerWorldObjectItem)?.GetPartsContainerMigrator();
        }
        public override void Initialize()
        {
            base.Initialize();
            EnsureInitialized();
        }
        private bool initialized = false;
        /// <summary>
        /// Migrate and initialize the parts container.
        /// </summary>
        public void EnsureInitialized()
        {
            if (initialized) return;
            partsContainer ??= PartsContainerFactory.Create();
            partsContainer = Migrator?.Migrate(Parent, partsContainer) ?? partsContainer;
            partsContainer.Initialize(Parent);

            initialized = true;
        }
        public object PersistentData
        {
            get => PartsContainer; set => PartsContainer ??= value as IPartsContainer;
        }
    }
}
