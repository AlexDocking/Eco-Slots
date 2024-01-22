using Eco.Core.Tests;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
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
            Inventory storage = new LimitedInventory(1);
            /*RegularSchema schema = new RegularSchema()
            {
                Slots = new SlotDefinitions()
                {
                    new SlotDefinition()
                    {
                        Name = "First Slot",
                        AllowedItemTypes = new[]
                        {
                            typeof(TestPart)
                        },
                        StoragesThatMustBeEmpty = new[] { storage }
                    },
                    new SlotDefinition()
                    {
                        Name = "Second Slot",
                        Optional = false,
                        MustHavePart = () => new TestPart()
                    }
                }
            };*/

            RegularSchema schema = new RegularSchema()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new SlotDefinition()
                    {
                        Name = "First Slot"
                    },
                    new SlotDefinition()
                    {
                        Name = "Second Slot"
                    }
                }
            };
            WorldObject worldObject = new TestWorldObject();

            IPartsContainer partsContainer = schema.Migrate(worldObject, PartsContainerFactory.Create());
            partsContainer.Initialize(worldObject);

            DebugUtils.AssertEquals(2, partsContainer.Slots.Count(), "Should have made two slots");

            Slot firstSlot = partsContainer.Slots[0];
            Slot secondSlot = partsContainer.Slots[1];
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
                    new SlotDefinition()
                    {
                        Optional = true
                    },
                    new SlotDefinition()
                    {
                        Optional = false
                    }
                }
            };

            WorldObject worldObject = new TestWorldObject();
            IPartsContainer partsContainer = schema.Migrate(worldObject, PartsContainerFactory.Create());
            partsContainer.Initialize(worldObject);

            if (partsContainer.Slots.Count != 2) return;
            Slot firstSlot = partsContainer.Slots[0];
            Slot secondSlot = partsContainer.Slots[1];

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
                    new SlotDefinition()
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

            Slot firstSlot = partsContainer.Slots[0];
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
                    new SlotDefinition()
                    {
                        MustHavePart = () => new TestPart()
                    },
                    new SlotDefinition()
                }
            };

            IPartsContainer partsContainer = PartsContainerFactory.Create();
            partsContainer.AddPart(new Slot(), new TestPart2());

            WorldObject worldObject = new TestWorldObject();
            partsContainer = schema.Migrate(worldObject, partsContainer);
            partsContainer.Initialize(worldObject);

            if (partsContainer.Slots.Count != 2) return;
            Slot firstSlot = partsContainer.Slots[0];
            Slot secondSlot = partsContainer.Slots[1];

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
                    new SlotDefinition()
                    {
                        MustHavePartIfEmpty = () => new TestPart()
                    },
                    new SlotDefinition()
                    {
                        MustHavePartIfEmpty = () => new TestPart()
                    }
                }
            };
            IPartsContainer partsContainer = PartsContainerFactory.Create();
            partsContainer.AddPart(new Slot(), new TestPart2());

            WorldObject worldObject = new TestWorldObject();
            partsContainer = schema.Migrate(worldObject, partsContainer);
            partsContainer.Initialize(worldObject);

            DebugUtils.AssertEquals(typeof(TestPart2), partsContainer.Slots.First().Part?.GetType(), "Could not set existing part");

            if (partsContainer.Slots.Count != 2) return;
            Slot firstSlot = partsContainer.Slots[0];
            Slot secondSlot = partsContainer.Slots[1];

            DebugUtils.AssertEquals(typeof(TestPart2), firstSlot.Part?.GetType(), "Should not have replaced existing part");
            DebugUtils.AssertEquals(typeof(TestPart), secondSlot.Part?.GetType(), "Should have created new part to fill empty slot");
        }
    }
}
