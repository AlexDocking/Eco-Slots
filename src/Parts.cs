using Eco.Core.Controller;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Properties;
using Eco.Core.PropertyHandling;
using Eco.Core.Serialization.Migrations;
using Eco.Core.Serialization.Migrations.Attributes;
using Eco.Core.Systems;
using Eco.Core.Utils;
using Eco.Gameplay.Economy;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Gameplay.Utils;
using Eco.Mods.TechTree;
using Eco.Shared.Gameplay;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using Eco.Shared.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    public static class ColorUtility
    {
        public static string RGBHex(string hex)
        {
            if (!IsValidColourHex(hex))
            {
                return "#000000";
            }
            if (!hex.StartsWith('#'))
            {
                hex = "#" + hex;
            }
            hex = hex.PadRight(9);
            hex = hex.Substring(0, 7).ToUpper();
            return hex;
        }
        public static bool IsValidColourHex(string colourHex)
        {
            if (string.IsNullOrEmpty(colourHex)) return false;

            colourHex = colourHex.TrimStart('#');
            if (colourHex.Length != 6 && colourHex.Length != 8) return false;

            colourHex = colourHex.ToLower();
            return colourHex.All(c => (c >= 'a' && c <= 'f') || (c >= '0' && c <= '9'));
        }
        public static Color? FromHex(string hex)
        {
            if (IsValidColourHex(hex))
            {
                return new Color(hex);
            }
            return null;
        }
    }
    [Serialized]
    public interface IPart
    {
        public string DisplayName { get; }
    }
    
    [Serialized]
    public class Slot
    {
        [SyncToView] public string Name { get; set; }
        [Serialized, SyncToView, NewTooltip(Eco.Shared.Items.CacheAs.Disabled)] public Inventory Inventory { get; set; } = new AuthorizationInventory(1);
    }
    [Serialized]
    public class PartsContainer : IController, INotifyPropertyChanged
    {
        [Serialized, SyncToView, NewTooltip(Eco.Shared.Items.CacheAs.Disabled)] public string Name { get; set; } = "Serialized Name";
        public IReadOnlyList<IPart> Parts => Slots.SelectNonNull(slot => slot.Inventory.Stacks.FirstOrDefault()?.Item).OfType<IPart>().ToList();
        [Serialized]
        private ThreadSafeList<Slot> slots = new ThreadSafeList<Slot> ();
        [SyncToView, NewTooltipChildren(Eco.Shared.Items.CacheAs.Disabled)]
        public LocString CurrentPartsListDescription => Localizer.DoStr("Contains parts:").AppendLine(Parts.Select(part => part.UILinkGeneric()).NewlineList());
        public IReadOnlyList<Slot> Slots => slots.Snapshot.AsReadOnly();
        public void AddPart(Slot slot, IPart part)
        {
            if (part is not Item partItem) return;
            slot.Inventory.AddItem(partItem);
            slots.Add(slot);
        }
        public void RemovePart(Slot slot)
        {
            int index = slots.IndexOf(slot);
            if (index >= 0)
            {
                slots.RemoveAt(index);
            }
        }
        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
    [Serialized]
    [NoIcon]
    public class PartsContainerComponent : WorldObjectComponent, IPersistentData
    {
        public override WorldObjectComponentClientAvailability Availability => WorldObjectComponentClientAvailability.Always;

        [Serialized, SyncToView, NewTooltipChildren(Eco.Shared.Items.CacheAs.Disabled)]
        public PartsContainer PartsContainer { get; set; } = new PartsContainer();
        public object PersistentData
        {
            get => PartsContainer; set
            {
                PartsContainer = value as PartsContainer ?? new PartsContainer();
                Log.WriteLine(Localizer.DoStr($"Deserialized persistent data. Null? {(value as PartsContainer) == null}"));
            }
        }
        public override void Initialize()
        {
            IReadOnlyList<Slot> slots = PartsContainer.Slots;
            IReadOnlyList<IPart> parts = PartsContainer.Parts;
            Log.WriteLine(Localizer.DoStr($"Slots {slots.Count}, parts {parts.Count}"));
            for (int i = 0; i < parts.Count; i++)
            {
                Log.WriteLine(Localizer.DoStr($"Slot {i}: {slots[i].Inventory.NonEmptyStacks.FirstOrDefault()?.Item.Name}"));
                Log.WriteLine(Localizer.DoStr($"Part {i}: {parts[i].DisplayName}"));
            }
        }
    }
    [Serialized]
    public class ModelPartColouring : IController, INotifyPropertyChanged
    {
        [Serialized]
        public string ModelName { get; set; }
        [Serialized]
        public Color Colour
        {
            get => colour; set
            {
                colour = value;
                this.Changed(nameof(Colour));
            }
        }

        public ModelPartColouring()
        {
            PropertyChanged += OnPropertyChanged;
        }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            Log.WriteLine(Localizer.DoStr("Detected change in " + args.PropertyName));
        }
        #region IController
        private int id;
        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        public ref ThreadSafeSubscriptions Subscriptions => ref this.subscriptions; ThreadSafeSubscriptions subscriptions;
        private Color colour;
    }
    [Serialized, AutogenClass]
    [UITypeName("PropertyPage")]
    public class ColouredPartView : IController, INotifyPropertyChanged
    {
        [SyncToView, Autogen]
        [UITypeName("GeneralHeader")]
        public string NameDisplay => Component.DisplayName;

        [SyncToView, Autogen, AutoRPC]
        public string ColourHex
        {
            get => ToHex(); set
            {
                Color colour = new Color(ColorUtility.RGBHex(value));
                r = colour.R;
                g = colour.G;
                b = colour.B;
                SyncToModel();
            }
        }

        [LocDisplayName("Red")]
        [SyncToView, Autogen, AutoRPC]
        [Range(0, 1)]
        public float R
        {
            get => r; set
            {
                float newValue = Math.Clamp(value, 0, 1);
                if (r == newValue) return;
                r = newValue;
                SyncToModel();
            }
        }
        [LocDisplayName("Green")]
        [SyncToView, Autogen, AutoRPC]
        [Range(0, 1)]
        public float G
        {
            get => g; set
            {
                float newValue = Math.Clamp(value, 0, 1);
                if (g == newValue) return;
                g = newValue;
                SyncToModel();
            }
        }
        [LocDisplayName("Blue")]
        [SyncToView, Autogen, AutoRPC]
        [Range(0, 1)]
        public float B
        {
            get => b; set
            {
                float newValue = Math.Clamp(value, 0, 1);
                if (b == newValue) return;
                b = newValue;
                SyncToModel();
            }
        }
        private string ToHex()
        {
            return ColorUtility.RGBHex(new Color(R, G, B).HexRGBA);
        }

        private float r = 1;
        private float g = 1;
        private float b = 1;

        public IHasModelPartColourComponent Component { get; }

        public ColouredPartView(IHasModelPartColourComponent component)
        {
            Component = component;
            r = Component.ColourData.Colour.R;
            g = Component.ColourData.Colour.G;
            b = Component.ColourData.Colour.B;
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            Log.WriteLine(Localizer.DoStr("Property Changed " + Component.ToString()));
            SetColour(new Color(R, G, B));
        }
        /// <summary>
        /// Update the model with the colour values in the view
        /// </summary>
        public void SyncToModel()
        {
            SetColour(new Color(R, G, B));

            this.Changed(nameof(ColourHex));
            this.Changed(nameof(R));
            this.Changed(nameof(G));
            this.Changed(nameof(B));
        }
        private void SetColour(Color colour)
        {
            ModelPartColouring partColouring = Component.ColourData;
            if (partColouring == null) return;
            partColouring.Colour = colour;
        }

        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
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
    [Serialized]
    [NoIcon]
    public class ModelPartColourComponent : WorldObjectComponent
    {
        private PartsContainer PartsContainer => Parent.GetComponent<PartsContainerComponent>().PartsContainer;

        private ThreadSafeList<IHasModelPartColourComponent> currentColouredParts { get; set; } = new ThreadSafeList<IHasModelPartColourComponent>();
        public override void Initialize()
        {
            base.Initialize();
            UpdateWatchedParts();
            SetModelColours();
        }
        private void UpdateWatchedParts()
        {
            lock (currentColouredParts)
            {
                IEnumerable<IHasModelPartColourComponent> newColouredParts = PartsContainer.Parts.OfType<IHasModelPartColourComponent>().ToList();

                IEnumerable<IHasModelPartColourComponent> addedParts = newColouredParts.Except(currentColouredParts);
                IEnumerable<IHasModelPartColourComponent> removedParts = currentColouredParts.Except(newColouredParts);
                foreach (IHasModelPartColourComponent part in addedParts)
                {
                    //part.ColourData.SubscribeAndCall(nameof(ModelPartColouring.Colour), SetModelColours);
                    Log.WriteLine(Localizer.DoStr("Subscribing to " + part.DisplayName));
                    part.ColourData.SubscribeAndCall(nameof(ModelPartColouring.Colour), SetModelColours);
                }
                foreach(IHasModelPartColourComponent part in removedParts)
                {
                    part.ColourData.Unsubscribe(nameof(ModelPartColouring.Colour), SetModelColours);
                }
                currentColouredParts.Set(newColouredParts);
            }

        }
        private void SetModelColours()
        {
            foreach (IHasModelPartColourComponent colouredPart in currentColouredParts)
            {
                ModelPartColouring partColouring = colouredPart.ColourData;
                Color colour = partColouring.Colour;
                Log.WriteLine(Localizer.DoStr("Send colour " + colour + " to model " + partColouring.ModelName));
                Parent.SetAnimatedState(partColouring.ModelName + "-Red", colour.R);
                Parent.SetAnimatedState(partColouring.ModelName + "-Green", colour.G);
                Parent.SetAnimatedState(partColouring.ModelName + "-Blue", colour.B);
            }
        }
    }

    public interface IHasModelPartColourComponent : IPart
    {
        public ModelPartColouring ColourData { get; }
    }
}
