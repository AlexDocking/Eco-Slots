using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Utils;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    [Serialized]
    [AutogenClass]
    [CreateComponentTabLoc("Part Colours")]
    [NoIcon]
    [RequireComponent(typeof(ModelPartColourComponent))]
    public class PartColoursUIComponent : WorldObjectComponent, IHasClientControlledContainers, INotifyPropertyChanged
    {
        private IList<(IHasModelPartColourComponent, ColouredPartView)> partViews = new ThreadSafeList<(IHasModelPartColourComponent, ColouredPartView)>();
        private IEnumerable<ColouredPartView> Viewers => partViews.Select(pair => pair.Item2);
        [Autogen, SyncToView, HideRoot, HideRootListEntry]
        public ControllerList<ColouredPartView> PartsUI { get; private set; }

        private PartsContainer PartsContainer { get; set; }
        public PartColoursUIComponent()
        {
            PartsUI = new ControllerList<ColouredPartView>(this, nameof(PartsUI), Array.Empty<ColouredPartView>());
        }
        public override void Initialize()
        {
            base.Initialize();
            PartsContainer = Parent.GetComponent<PartsContainerComponent>().PartsContainer;
        }
        public override void PostInitialize()
        {
            base.PostInitialize();
            IReadOnlyList<IPart> parts = PartsContainer.Parts;
            for (int i = 0; i < parts.Count; i++)
            {
                IPart part = parts[i];
                IHasModelPartColourComponent partColourComponent = (part as IHasModelPartColourComponent);
                if (partColourComponent != null)
                {
                    ColouredPartView partView = new ColouredPartView(partColourComponent);
                    partViews.Add((partColourComponent, partView));
                }
            }
            PartsUI.Set(Viewers);
            PartsUI.Callbacks.OnAdd.Add(ResetList);
            PartsUI.Callbacks.OnRemove.Add(ResetList);
        }
        private void ResetList(INetObject sender, object obj)
        {
            PartsUI.Set(Viewers);
            PartsUI.NotifyChanged();
        }
    }
}
