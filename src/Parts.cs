using Eco.Core.Controller;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Serialization.Migrations;
using Eco.Core.Serialization.Migrations.Attributes;
using Eco.Core.Systems;
using Eco.Core.Utils;
using Eco.Gameplay.Economy;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Utils;
using Eco.Mods.TechTree;
using Eco.Shared.Gameplay;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
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
    public class PartAttributeChangeArgs
    {
        public Part Part { get; init; }
        public string Attribute { get; init; }
        public object OldValue { get; init; }
        public object NewValue { get; init; }
    }
    [Serialized]
    public interface IPart
    {
        public string Name { get; }
        public string DisplayName { get; }
        T GetAttribute<T>(string attributeName);
        object GetAttribute(string attributeName);
        bool HasAttribute(string attributeName);
        void SetAttribute(string attributeName, object value);
        public ThreadSafeAction<PartAttributeChangeArgs> OnChanged { get; }
    }
    [Serialized]
    public class Part : IPart
    {
        [Serialized]
        public string Name { get; init; }
        [Serialized]
        public string DisplayName { get; init; }
        [Serialized]
        private ThreadSafeDictionary<string, object> Attributes { get; set; } = new ThreadSafeDictionary<string, object>();
        public bool HasAttribute(string attributeName) => Attributes.ContainsKey(attributeName);
        public T GetAttribute<T>(string attributeName) => (Attributes.TryGetValue(attributeName, out object value) && value is T t) ? t : default;
        public object GetAttribute(string attributeName) => Attributes.TryGetValue(attributeName, out object value) ? value : null;
        public void SetAttribute(string attributeName, object value)
        {
            if (HasAttribute(attributeName))
            {
                object oldValue = GetAttribute(attributeName);
                bool changed = oldValue != value;
                Attributes[attributeName] = value;
                if (changed)
                {
                    OnChanged.Invoke(new PartAttributeChangeArgs()
                    {
                        Part = this,
                        Attribute = attributeName,
                        OldValue = oldValue,
                        NewValue = value
                    });
                }
            }
            else
            {
                Attributes.Add(attributeName, value);
            }
        }
        public ThreadSafeAction<PartAttributeChangeArgs> OnChanged { get; } = new ThreadSafeAction<PartAttributeChangeArgs>();
        
    }
    
    [Serialized]
    public class Slot
    {
        public string Name { get; set; }
    }
    [Serialized]
    public class PartsContainer
    {
        [Serialized]
        private ThreadSafeList<IPart> parts = new ThreadSafeList<IPart>();
        public IReadOnlyList<IPart> Parts => parts.AsReadOnly();
        [Serialized]
        private ThreadSafeList<Slot> slots = new ThreadSafeList<Slot> ();
        public IReadOnlyList<Slot> Slots => slots.AsReadOnly();
        public void AddPart(Slot slot, Part part)
        {
            parts.Add(part);
            slots.Add(slot);
        }
        public void RemovePart(Slot slot)
        {
            int index = slots.IndexOf(slot);
            if (index >= 0)
            {
                slots.RemoveAt(index);
                parts.RemoveAt(index);
            }
        }
    }
    [Serialized]
    [NoIcon]
    public class PartsContainerComponent : WorldObjectComponent, IPersistentData
    {
        [Serialized]
        public PartsContainer PartsContainer { get; private set; } = new PartsContainer();
        public object PersistentData
        {
            get => PartsContainer; set
            {
                Log.WriteLine(Localizer.DoStr($"Deserialized persistent data. Null? {(value as PartsContainer) == null}"));
                PartsContainer = value as PartsContainer ?? new PartsContainer();
            }
        }
    }
    [Serialized]
    public class ModelPartColouring : INotifyPropertyChanged
    {
        [Serialized]
        private string modelName = "";
        [Serialized]
        private Color colour;
        [Notify]
        public string ModelName
        {
            get => modelName; set
            {
                modelName = value;
            }
        }
        [Notify]
        public Color Colour
        {
            get => colour; set
            {
                colour = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Colour)));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    [Serialized, AutogenClass]
    [UITypeName("PropertyPage")]
    public class PartView : IController, INotifyPropertyChanged
    {
        [SyncToView, Autogen]
        [UITypeName("GeneralHeader")]
        public string NameDisplay => Name;
        public string Name { get; init; }

        [SyncToView, Autogen, AutoRPC]
        public string ColourHex
        {
            get => ToHex(); set
            {
                Color colour = new Color(ColorUtility.RGBHex(value));
                R = colour.R;
                G = colour.G;
                B = colour.B;
                this.Changed(nameof(ColourHex));
                this.Changed(nameof(R));
                this.Changed(nameof(G));
                this.Changed(nameof(B));
            }
        }

        [LocDisplayName("Red")]
        [SyncToView, Autogen, AutoRPC, Notify]
        [Range(0, 1)]
        public float R
        {
            get => r; set
            {
                r = Math.Clamp(value, 0, 1);
                this.Changed(nameof(R));
                this.Changed(nameof(ColourHex));
            }
        }
        [LocDisplayName("Green")]
        [SyncToView, Autogen, AutoRPC, Notify]
        [Range(0, 1)]
        public float G
        {
            get => g; set
            {
                g = Math.Clamp(value, 0, 1);
                this.Changed(nameof(G));
                this.Changed(nameof(ColourHex));
            }
        }
        [LocDisplayName("Blue")]
        [SyncToView, Autogen, AutoRPC, Notify]
        [Range(0, 1)]
        public float B
        {
            get => b; set
            {
                b = Math.Clamp(value, 0, 1);
                this.Changed(nameof(B));
                this.Changed(nameof(ColourHex));
            }
        }
        private string ToHex()
        {
            return ColorUtility.RGBHex(new Color(R, G, B).HexRGBA);
        }

        private float r = 1;
        private float g = 1;
        private float b = 1;

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
        private IDictionary<IPart, PartView> partViews = new ThreadSafeDictionary<IPart, PartView>();

        [Autogen, SyncToView, HideRoot, HideRootListEntry]
        public ControllerList<PartView> PartsUI { get; private set; }

        private PartsContainer PartsContainer { get; set; }
        public PartColoursUIComponent()
        {
            PartsUI = new ControllerList<PartView>(this, nameof(PartsUI), Array.Empty<PartView>());
        }
        public override void Initialize()
        {
            base.Initialize();
            PartsContainer = Parent.GetComponent<PartsContainerComponent>().PartsContainer;
        }
        public override void PostInitialize()
        {
            base.PostInitialize();
            foreach (IPart part in PartsContainer.Parts)
            {
                ModelPartColouring partColouring = part.GetAttribute<ModelPartColouring>("PartColouring");
                if (partColouring != null)
                {
                    partViews.Add(part, new PartView()
                    {
                        Name = part.DisplayName,
                        R = partColouring.Colour.R,
                        G = partColouring.Colour.G,
                        B = partColouring.Colour.B,
                    });
                }
            }
            PartsUI.Set(partViews.Values);
            foreach (PartView partView in PartsUI)
            {
                partView.PropertyChanged += OnPropertyChanged;
            }
            PartsUI.Callbacks.OnAdd.Add(ResetList);
            PartsUI.Callbacks.OnRemove.Add(ResetList);
            this.PropertyChanged += OnPropertyChanged;
        }
        private void ResetList(INetObject sender, object obj)
        {
            PartsUI.Set(partViews.Values);
            PartsUI.NotifyChanged();
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (sender is not PartView partView) return;
            IPart part = partViews.First(kpv => kpv.Value == partView).Key;
            SetColour(part, new Color(partView.R, partView.G, partView.B));
            PartsUI.NotifyChanged();
            this.Changed(nameof(PartsUI));
        }
        private void SetColour(IPart part, Color colour)
        {
            ModelPartColouring partColouring = part.GetAttribute<ModelPartColouring>("PartColouring");
            if (partColouring == null) return;
            partColouring.Colour = colour;
        }
    }
    [Serialized]
    [NoIcon]
    public class ModelPartColourComponent : WorldObjectComponent
    {
        public override void Initialize()
        {
            base.Initialize();
            PartsContainer partsContainer = Parent.GetComponent<PartsContainerComponent>().PartsContainer;
            foreach (IPart part in partsContainer.Parts)
            {
                ModelPartColouring partColouring = part.GetAttribute<ModelPartColouring>("PartColouring");
                partColouring.PropertyChanged += OnModelPartColouringChanged;
            }
        }
        private void OnModelPartColouringChanged(object sender, PropertyChangedEventArgs args)
        {
            if (sender is not ModelPartColouring partColouring) return;
            Color colour = partColouring.Colour;
            Parent.SetAnimatedState(partColouring.ModelName + "-Red", colour.R);
            Parent.SetAnimatedState(partColouring.ModelName + "-Green", colour.G);
            Parent.SetAnimatedState(partColouring.ModelName + "-Blue", colour.B);
        }
    }
}
