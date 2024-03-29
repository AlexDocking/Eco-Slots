﻿using Eco.Core.Tests;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.TechTree;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parts.Tests
{
    [ChatCommandHandler]
    public static class TestRegularPartsContainerMigrator
    {
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldCreateCorrectNumberOfSlotsInRegularMigrator()
        {
            DefaultPartsContainerMigrator migrator = new DefaultPartsContainerMigrator()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new DefaultInventorySlotDefinition()
                    {
                        Name = "First Slot"
                    },
                    new DefaultInventorySlotDefinition()
                    {
                        Name = "Second Slot"
                    }
                }
            };
            WorldObject worldObject = new TestWorldObject();

            IPartsContainer partsContainer = migrator.Migrate(worldObject, PartsContainerFactory.Create());
            partsContainer.Initialize(worldObject);

            DebugUtils.AssertEquals(2, partsContainer.Slots.Count, "Should have made two slots");

            ISlot firstSlot = partsContainer.Slots[0];
            ISlot secondSlot = partsContainer.Slots[1];
            DebugUtils.AssertEquals("First Slot", firstSlot.Name, "Should have set name on first slot");
            DebugUtils.AssertEquals("Second Slot", secondSlot.Name, "Should have set name on second slot");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldSetOptionalPartsInRegularMigrator()
        {
            DefaultPartsContainerMigrator migrator = new DefaultPartsContainerMigrator()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new DefaultInventorySlotDefinition()
                    {
                        Optional = true
                    },
                    new DefaultInventorySlotDefinition()
                    {
                        Optional = false
                    }
                }
            };

            TestUtility.CreateWorldObject(PartsContainerFactory.Create(), out IPartsContainer partsContainer, migrator);

            if (partsContainer.Slots.Count != 2) return;
            ISlot firstSlot = partsContainer.Slots[0];
            DebugUtils.Assert(firstSlot.SetPart(new TestPart()), "Should have been allowed to set the part");
            ISlot secondSlot = partsContainer.Slots[1];
            DebugUtils.Assert(secondSlot.SetPart(new TestPart()), "Should have been allowed to set the part");

            DebugUtils.Assert(firstSlot.CanRemovePart(), "Slot should be optional");
            DebugUtils.Assert(!secondSlot.CanRemovePart(), "Slot should not be optional"); ;
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldSetCorrectTypesInRegularMigrator()
        {
            DefaultPartsContainerMigrator migrator = new DefaultPartsContainerMigrator()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new DefaultInventorySlotDefinition()
                    {
                        AllowedItemTypes = new[]
                        {
                            typeof(TestPart)
                        }
                    }
                }
            };

            WorldObject worldObject = new TestWorldObject();
            IPartsContainer partsContainer = migrator.Migrate(worldObject, PartsContainerFactory.Create());
            partsContainer.Initialize(worldObject);

            if (partsContainer.Slots.Count != 1) return;

            ISlot firstSlot = partsContainer.Slots[0];
            IEnumerable<Type> allowedItemTypes = firstSlot.SlotDefinition.RestrictionsToAddPart.OfType<LimitedTypeSlotRestriction>().SelectMany(restriction => restriction.AllowedTypes);
            DebugUtils.AssertEquals(1, allowedItemTypes.Count(), "Should only be one allowed item type");
            DebugUtils.AssertEquals(typeof(TestPart), allowedItemTypes.FirstOrDefault(), "Should have set allowed item type");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldReplaceDefaultPartInRegularMigrator()
        {
            DefaultPartsContainerMigrator migrator = new DefaultPartsContainerMigrator()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new DefaultInventorySlotDefinition()
                    {
                        MustHavePart = () => new TestPart()
                    },
                    new DefaultInventorySlotDefinition()
                }
            };

            IPartsContainer partsContainer = PartsContainerFactory.Create();
            partsContainer.TryAddSlot(TestUtility.CreateSlot(), new TestPart2());

            WorldObject worldObject = new TestWorldObject();
            partsContainer = migrator.Migrate(worldObject, partsContainer);
            partsContainer.Initialize(worldObject);

            if (partsContainer.Slots.Count != 2) return;
            ISlot firstSlot = partsContainer.Slots[0];
            ISlot secondSlot = partsContainer.Slots[1];

            DebugUtils.AssertEquals(typeof(TestPart), firstSlot.Part?.GetType(), "Should have created a new item to fill the slot");
            DebugUtils.AssertEquals(null, secondSlot.Part?.GetType(), "Should not have created a new item to fill the slot");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldSetDefaultPartInRegularMigratorOnlyIfEmpty()
        {
            DefaultPartsContainerMigrator migrator = new DefaultPartsContainerMigrator()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new DefaultInventorySlotDefinition()
                    {
                        MustHavePartIfEmpty = () => new TestPart()
                    },
                    new DefaultInventorySlotDefinition()
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
            partsContainer = migrator.Migrate(worldObject, partsContainer);
            partsContainer.Initialize(worldObject);


            if (partsContainer.Slots.Count != 2) return;
            ISlot firstSlot = partsContainer.Slots[0];
            ISlot secondSlot = partsContainer.Slots[1];

            DebugUtils.AssertEquals(typeof(TestPart2), firstSlot.Part?.GetType(), "Should not have replaced existing part");
            DebugUtils.AssertEquals(typeof(TestPart), secondSlot.Part?.GetType(), "Should have created new part to fill empty slot");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldEnsureEmptyStorages()
        {
            DefaultPartsContainerMigrator migrator = new DefaultPartsContainerMigrator()
            {
                SlotDefinitions = new SlotDefinitions()
                {
                    new DefaultInventorySlotDefinition()
                    {
                        RequiresEmptyStorageToChangePart = true,
                        Optional = true
                    },
                }
            };
            IPartsContainer partsContainer = PartsContainerFactory.Create();
            partsContainer.TryAddSlot(TestUtility.CreateSlot(), new TestPart());

            WorldObject worldObject = new TestWorldObject();
            worldObject.InitializeForTest();
            Inventory storage = worldObject.GetComponent<PublicStorageComponent>().Storage;

            partsContainer = migrator.Migrate(worldObject, partsContainer);
            partsContainer.Initialize(worldObject);
            ISlot slot = partsContainer.Slots[0];

            DebugUtils.Assert(slot.CanAcceptPart(new TestPart()), "Slot should accept a new part when storage is empty");
            storage.AddItem(typeof(CornItem));
            DebugUtils.Assert(!slot.CanAcceptPart(new TestPart()), "Slot should not accept a new part when storage is not empty");

            storage.RemoveItem(typeof(CornItem));
            slot.SetPart(new TestPart());
            DebugUtils.Assert(slot.CanRemovePart(), "Slot should allow the part to be removed when the storage is empty");
            storage.AddItem(typeof(CornItem));
            DebugUtils.Assert(!slot.CanRemovePart(), "Slot should not allow the part to be removed when the storage is not empty");
        }
    }
}
