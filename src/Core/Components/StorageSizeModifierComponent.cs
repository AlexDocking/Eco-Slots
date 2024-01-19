using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Parts
{
    [Serialized]
    [NoIcon]
    [RequireComponent(typeof(PartsContainerComponent))]
    [PriorityAfter(typeof(PublicStorageComponent), typeof(PartsContainerComponent))]
    public class StorageSizeModifierComponent : WorldObjectComponent
    {
        private StorageSizeSetterViewController View { get; set; }
        private IPartsContainer PartsContainer { get; set; }

        [Serialized] private int baseNumSlots;
        [Serialized] private int baseWeightLimit;
        [Serialized] private bool isNewObject = true;
        private WeightComponent WeightComponent { get; set; }
        public override void Initialize()
        {
            base.Initialize();
            PartsContainer = Parent.GetComponent<PartsContainerComponent>().PartsContainer;

            Inventory publicStorageInventory = Parent.GetComponent<PublicStorageComponent>().Storage;
            if (isNewObject) baseNumSlots = publicStorageInventory.Stacks.Count();
            IEnumerable<InventoryComponent> components = typeof(Inventory).GetProperty("Components", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(publicStorageInventory) as IEnumerable<InventoryComponent>;
            WeightComponent = components?.OfType<WeightComponent>().FirstOrDefault();
            baseWeightLimit = WeightComponent?.MaxWeight ?? -1;
            isNewObject = false;
            Log.WriteLine(Localizer.DoStr($"Storage modifier:{baseNumSlots}, {baseWeightLimit}, {components?.Count()}"));
            Log.WriteLine(Localizer.DoStr("parts:" + PartsContainer.Parts.Count()));
            Log.WriteLine(Localizer.DoStr("Modifers:" + PartsContainer.Parts.OfType<IHasCustomStorageSize>().Count()));

        }
        public override void PostInitialize()
        {
            base.PostInitialize();
            BuildViews();
        }
        private void BuildViews()
        {
            StorageSizeSetterViewController partView = new StorageSizeSetterViewController();
            View = partView;
            partView.SetModel(Parent, PartsContainer, baseNumSlots, baseWeightLimit);
        }
    }
}
