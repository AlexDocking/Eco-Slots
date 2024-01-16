using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using Eco.Shared.Pools;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using KitchenUnits;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    /// <summary>
    /// Send model part names to the client to change the configuration the world object model
    /// </summary>
    public class ModelReplacementViewController : IController, INotifyPropertyChanged
    {
        KitchenCupboardModelReplacements ModelReplacements { get; } = new KitchenCupboardModelReplacements();
        public WorldObject WorldObject { get; private set; }
        public PartsContainer Model { get; private set; }

        public void SetModel(WorldObject worldObject, PartsContainer model)
        {
            WorldObject = worldObject;
            Model?.NewPartInSlotEvent.Remove(OnPartChanged);
            Model = model;
            Model?.NewPartInSlotEvent.Add(OnPartChanged);
            OnModelChanged();
        }
        private void OnPartChanged(Slot slot) => OnModelChanged();
        /// <summary>
        /// Update the view with the model name
        /// </summary>
        private void OnModelChanged()
        {
            Log.WriteLine(Localizer.DoStr("Send enabled parts model change"));

            ModelReplacements.SetEnabledParts(WorldObject, Model);
        }

        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
    public class KitchenCupboardModelReplacements 
    {
        public void SetEnabledParts(WorldObject worldObject, PartsContainer container)
        {
            IReadOnlyList<IPart> parts = container.Parts;

            worldObject.SetAnimatedState("Flat Door", parts.Any(part => part is KitchenCupboardFlatDoorItem));
            worldObject.SetAnimatedState("Shaker Door", parts.Any(part => part is KitchenCupboardShakerDoorItem));
            worldObject.SetAnimatedState("No Door", parts.None(part => part is KitchenCupboardFlatDoorItem || part is KitchenCupboardShakerDoorItem));

        }
    }
    [Serialized]
    [NoIcon]
    [RequireComponent(typeof(ModelPartColourComponent))]
    [RequireComponent(typeof(PartsContainerComponent))]
    public class ModelReplacerComponent : WorldObjectComponent, IHasClientControlledContainers, INotifyPropertyChanged
    {
        ModelReplacementViewController view = null;
        private PartsContainer PartsContainer { get; set; }
        public override void Initialize()
        {
            base.Initialize();
            PartsContainer = Parent.GetComponent<PartsContainerComponent>().PartsContainer;
        }
        public override void PostInitialize()
        {
            base.PostInitialize();
            BuildViews();
        }
        private void BuildViews()
        {
            view = new ModelReplacementViewController();
            view.SetModel(Parent, PartsContainer);
        }
    }
}
