using Eco.Core.Controller;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Utils;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using Parts.Kitchen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    /// <summary>
    /// Send model part names to the client to change the configuration the world object model.
    /// TODO: remove coupling with kitchen cupboards so that other world objects can define their own set of mesh replacements.
    /// </summary>
    public class ModelReplacer : IController, INotifyPropertyChanged
    {
        KitchenCupboardModelReplacements ModelReplacements { get; } = new KitchenCupboardModelReplacements();
        public WorldObject WorldObject { get; private set; }
        public IPartsContainer Model { get; private set; }
        public void SetModel(WorldObject worldObject, IPartsContainer model)
        {
            WorldObject = worldObject;
            Model?.NewPartInSlotEvent.Remove(OnPartChanged);
            Model = model;
            Model?.NewPartInSlotEvent.Add(OnPartChanged);
            OnModelChanged();
        }
        private void OnPartChanged(ISlot slot) => OnModelChanged();
        /// <summary>
        /// Update the view with the model name
        /// </summary>
        private void OnModelChanged()
        {
            ModelReplacements.SetEnabledParts(WorldObject, Model);
        }

        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
    /// <summary>
    /// Sets the bool visibility states of different parts of the GameObject for a kitchen cupboard with interchangeable doors.
    /// TODO: refactor into interface.
    /// </summary>
    public class KitchenCupboardModelReplacements 
    {
        public void SetEnabledParts(WorldObject worldObject, IPartsContainer container)
        {
            IReadOnlyList<IPart> parts = container.Parts;

            worldObject.SetAnimatedState("Flat Door", parts.Any(part => part is KitchenCabinetFlatDoorItem));
            worldObject.SetAnimatedState("Shaker Door", parts.Any(part => part is KitchenCupboardRaisedPanelDoorItem));
            worldObject.SetAnimatedState("No Door", parts.None(part => part is KitchenCabinetFlatDoorItem || part is KitchenCupboardRaisedPanelDoorItem));
        }
    }
    /// <summary>
    /// Listen for changes in the parts container so that the game object can be kept in sync with which parts are installed.
    /// The object may have different models or parts of models depending on the state of the installed parts, such as replacing one style of cabinet door for another.
    /// </summary>
    [Serialized]
    [NoIcon]
    [RequireComponent(typeof(PartsContainerComponent))]
    public class ModelReplacerComponent : WorldObjectComponent, IHasClientControlledContainers, INotifyPropertyChanged
    {
        ModelReplacer view = null;
        private IPartsContainer PartsContainer { get; set; }
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
            view = new ModelReplacer();
            view.SetModel(Parent, PartsContainer);
        }
    }
}
