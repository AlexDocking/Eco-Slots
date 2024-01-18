using Eco.Core.Tests;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using KitchenUnits;
using Parts.Migration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts.Tests
{
    [ChatCommandHandler]
    public static class TestPartsContainer
    {
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldParentSlotToPartsContainer()
        {
            PartsContainer partsContainer = new PartsContainer();
            Slot slot = new Slot();
            WorldObject worldObject = new KitchenCupboardObject();
            partsContainer.AddPart(slot, null);
            partsContainer.Initialize(worldObject);

            DebugUtils.AssertEquals(partsContainer, slot.PartsContainer, $"Did not set parent slot to {nameof(PartsContainer)} correctly");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTriggerSlotChangedEvent()
        {
            PartsContainer partsContainer = new PartsContainer();
            Slot slot = new Slot();
            WorldObject worldObject = new KitchenCupboardObject();
            partsContainer.AddPart(slot, null);
            partsContainer.Initialize(worldObject);

            int calls = 0;
            Action<Slot> callback = s =>
                        {
                            calls += 1;
                            DebugUtils.AssertEquals(slot, s);
                        };
            partsContainer.NewPartInSlotEvent.Add(callback);

            KitchenBaseCabinetBoxItem box = new KitchenBaseCabinetBoxItem();
            slot.Inventory.AddItem(box);

            partsContainer.NewPartInSlotEvent.Remove(callback);
            DebugUtils.AssertEquals(1, calls, $"Did not trigger event on {nameof(PartsContainer)} when item was added to slot");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTriggerGlobalEventWhenSlotChanges()
        {
            PartsContainer partsContainer = new PartsContainer();
            Slot slot = new Slot();
            WorldObject worldObject = new KitchenCupboardObject();
            partsContainer.AddPart(slot, null);
            partsContainer.Initialize(worldObject);

            int calls = 0;
            Action<IPartsContainer> callback = container =>
                        {
                            calls += 1;
                            DebugUtils.AssertEquals(partsContainer, container);
                        };
            PartsContainer.PartsContainerChangedEventGlobal.Add(callback);

            KitchenBaseCabinetBoxItem box = new KitchenBaseCabinetBoxItem();
            slot.Inventory.AddItem(box);
            DebugUtils.AssertEquals(1, calls, $"Should have fired global event on {nameof(PartsContainer)} when item was added to slot");

            calls = 0;
            box.ColourData.Colour = Color.Orange;
            PartsContainer.PartsContainerChangedEventGlobal.Remove(callback);

            DebugUtils.AssertEquals(1, calls, $"Should have fired global event on {nameof(PartsContainer)} when item colour changed");
        }

        [Serialized]
        [LocCategory("Hidden")]
        public class TestWorldObject : WorldObject, IPartsContainerWorldObject
        {
            public IPartsContainerSchema Schema { get; set; }
            public IPartsContainerSchema GetPartsContainerSchema() => Schema;
        }
        public class TestPartsContainerSchema : IPartsContainerSchema
        {
            private readonly IPartsContainer subsitute;
            public TestPartsContainerSchema(IPartsContainer subsitute)
            {
                this.subsitute = subsitute;
            }
            public IPartsContainer Migrate(WorldObject worldObject, IPartsContainer existingContainer)
            {
                subsitute.AddPart(new Slot(), existingContainer.Parts.First());
                return subsitute;
            }
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldUseSchemaToMigratePartsContainer()
        {
            PartsContainer migratedPartsContainer = new PartsContainer();
            WorldObject worldObject = new TestWorldObject() { Schema = new TestPartsContainerSchema(migratedPartsContainer) };
            PartsContainerComponent partsContainerComponent = worldObject.GetOrCreateComponent<PartsContainerComponent>();
            
            IPartsContainer existingPartsContainer = new PartsContainer();
            Slot slot = new Slot() { Name = "Box" };
            KitchenBaseCabinetBoxItem part = new KitchenBaseCabinetBoxItem();
            existingPartsContainer.AddPart(slot, part);
            partsContainerComponent.PartsContainer = existingPartsContainer;
            partsContainerComponent.PartsContainer.Initialize(worldObject);

            DebugUtils.AssertEquals(migratedPartsContainer, partsContainerComponent.PartsContainer, "Should have used migration schema to set the new parts container");
            DebugUtils.AssertEquals(part, partsContainerComponent.PartsContainer.Parts.FirstOrDefault(), "Should have passed in existing parts container to migration schema");
        }
    }

}
