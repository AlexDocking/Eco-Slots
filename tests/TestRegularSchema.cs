using Eco.Core.Tests;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts.Tests
{
    [ChatCommandHandler]
    public static class TestRegularSchema
    {
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldCreateCorrectNumberOfSlotsInRegularSchema()
        {
            RegularSchema schema = new RegularSchema()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new RegularSlotDefinition()
                    {
                        Name = "First Slot"
                    },
                    new RegularSlotDefinition()
                    {
                        Name = "Second Slot"
                    }
                }
            };
            WorldObject worldObject = new TestWorldObject();

            IPartsContainer partsContainer = schema.Migrate(worldObject, PartsContainerFactory.Create());
            partsContainer.Initialize(worldObject);

            DebugUtils.AssertEquals(2, partsContainer.Slots.Count(), "Should have made two slots");

            ISlot firstSlot = partsContainer.Slots[0];
            ISlot secondSlot = partsContainer.Slots[1];
            DebugUtils.AssertEquals("First Slot", firstSlot.Name, "Should have set name on first slot");
            DebugUtils.AssertEquals("Second Slot", secondSlot.Name, "Should have set name on second slot");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldSetOptionalPartsInRegularSchema()
        {
            RegularSchema schema = new RegularSchema()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new RegularSlotDefinition()
                    {
                        Optional = true
                    },
                    new RegularSlotDefinition()
                    {
                        Optional = false
                    }
                }
            };

            WorldObject worldObject = new TestWorldObject();
            IPartsContainer partsContainer = schema.Migrate(worldObject, PartsContainerFactory.Create());
            partsContainer.Initialize(worldObject);

            if (partsContainer.Slots.Count != 2) return;
            ISlot firstSlot = partsContainer.Slots[0];
            ISlot secondSlot = partsContainer.Slots[1];

            DebugUtils.Assert(partsContainer.SlotRestrictionManager.IsOptional(firstSlot), "Slot should be optional");
            DebugUtils.Assert(!partsContainer.SlotRestrictionManager.IsOptional(secondSlot), "Slot should not be optional");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldSetCorrectTypesInRegularSchema()
        {
            RegularSchema schema = new RegularSchema()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new RegularSlotDefinition()
                    {
                        AllowedItemTypes = new[]
                        {
                            typeof(TestPart)
                        }
                    }
                }
            };

            WorldObject worldObject = new TestWorldObject();
            IPartsContainer partsContainer = schema.Migrate(worldObject, PartsContainerFactory.Create());
            partsContainer.Initialize(worldObject);

            if (partsContainer.Slots.Count != 1) return;

            ISlot firstSlot = partsContainer.Slots[0];
            IEnumerable<Type> allowedItemTypes = partsContainer.SlotRestrictionManager.AllowedItemTypes(firstSlot);
            DebugUtils.AssertEquals(1, allowedItemTypes.Count(), "Should only be one allowed item type");
            DebugUtils.AssertEquals(typeof(TestPart), allowedItemTypes.FirstOrDefault(), "Should have set allowed item type");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldReplaceDefaultPartInRegularSchema()
        {
            RegularSchema schema = new RegularSchema()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new RegularSlotDefinition()
                    {
                        MustHavePart = () => new TestPart()
                    },
                    new RegularSlotDefinition()
                }
            };

            IPartsContainer partsContainer = PartsContainerFactory.Create();
            partsContainer.TryAddSlot(TestUtility.CreateSlot(), new TestPart2());

            WorldObject worldObject = new TestWorldObject();
            partsContainer = schema.Migrate(worldObject, partsContainer);
            partsContainer.Initialize(worldObject);

            if (partsContainer.Slots.Count != 2) return;
            ISlot firstSlot = partsContainer.Slots[0];
            ISlot secondSlot = partsContainer.Slots[1];

            DebugUtils.AssertEquals(typeof(TestPart), firstSlot.Part?.GetType(), "Should have created a new item to fill the slot");
            DebugUtils.AssertEquals(null, secondSlot.Part?.GetType(), "Should not have created a new item to fill the slot");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldSetDefaultPartInRegularSchemaOnlyIfEmpty()
        {
            RegularSchema schema = new RegularSchema()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new RegularSlotDefinition()
                    {
                        MustHavePartIfEmpty = () => new TestPart()
                    },
                    new RegularSlotDefinition()
                    {
                        MustHavePartIfEmpty = () => new TestPart()
                    }
                }
            };
            IPartsContainer partsContainer = PartsContainerFactory.Create();
            TestPart2 part = new TestPart2();
            partsContainer.TryAddSlot(TestUtility.CreateSlot(), part);
            DebugUtils.AssertEquals(typeof(TestPart2), partsContainer.Slots.FirstOrDefault()?.Part?.GetType(), "Could not set existing part");

            WorldObject worldObject = new TestWorldObject();
            partsContainer = schema.Migrate(worldObject, partsContainer);
            partsContainer.Initialize(worldObject);


            if (partsContainer.Slots.Count != 2) return;
            ISlot firstSlot = partsContainer.Slots[0];
            ISlot secondSlot = partsContainer.Slots[1];

            DebugUtils.AssertEquals(typeof(TestPart2), firstSlot.Part?.GetType(), "Should not have replaced existing part");
            DebugUtils.AssertEquals(typeof(TestPart), secondSlot.Part?.GetType(), "Should have created new part to fill empty slot");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldMakeRestrictionManagerEnsureEmptyStorages()
        {
            Inventory storage = new LimitedInventory(1);
            RegularSchema schema = new RegularSchema()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new RegularSlotDefinition()
                    {
                        StoragesThatMustBeEmpty = new[] { storage },
                    },
                }
            };
            IPartsContainer partsContainer = PartsContainerFactory.Create();
            partsContainer.TryAddSlot(TestUtility.CreateSlot(), new TestPart());

            WorldObject worldObject = new TestWorldObject();
            partsContainer = schema.Migrate(worldObject, partsContainer);
            partsContainer.Initialize(worldObject);

            DebugUtils.Assert(!partsContainer.SlotRestrictionManager.IsSlotLocked(partsContainer.Slots[0]), "Slot should not be locked when storage is empty");
            storage.AddItem(typeof(CornItem));
            DebugUtils.Assert(partsContainer.SlotRestrictionManager.IsSlotLocked(partsContainer.Slots[0]), "Slot should be locked when storage is not empty");
        }
    }
}
