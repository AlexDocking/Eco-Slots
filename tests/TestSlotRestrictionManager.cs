using Eco.Core.Tests;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.TechTree;
using Eco.Shared.Utils;
using Parts.Kitchen;
using System;

namespace Parts.Tests
{
    [ChatCommandHandler]
    public class TestSlotRestrictionManager
    {
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldOnlyAcceptCorrectItems()
        {
            InventorySlot slot = TestUtility.CreateInventorySlot();
            WorldObject worldObject = new KitchenCupboardObject();
            IPartsContainer partsContainer = PartsContainerFactory.Create();
            slot.Initialize(worldObject, partsContainer);
            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();
            slotRestrictionManager.SetTypeRestriction(slot, new Type[] { typeof(KitchenCupboardWorktopItem) });

            KitchenBaseCabinetBoxItem box = new KitchenBaseCabinetBoxItem();
            Result attemptedInvalidResult = slot.TryAddPart(box);
            DebugUtils.Assert(attemptedInvalidResult.Failed, "Slot inventory should not accept wrong item");
            DebugUtils.AssertEquals(null, slot.Part, "Slot part should not have changed");

            KitchenCupboardWorktopItem worktop = new KitchenCupboardWorktopItem();
            Result attemptedValidResult = slot.TryAddPart(worktop);
            DebugUtils.Assert(attemptedValidResult.Success, "Slot inventory should accept correct item");
            DebugUtils.AssertEquals(worktop, slot.Part, "Slot part should have changed");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldNotAllowRemovalUnlessOptional()
        {
            InventorySlot slot = TestUtility.CreateInventorySlot();
            WorldObject worldObject = new KitchenCupboardObject();
            PartsContainer partsContainer = new PartsContainer();
            slot.Initialize(worldObject, partsContainer);

            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();
            slotRestrictionManager.SetOptional(slot, false);

            bool canRemovePart = slot.Inventory.GetMaxPickup(Item.Get<KitchenBaseCabinetBoxItem>(), 1).Val > 0;
            DebugUtils.Assert(!canRemovePart, "Slot should not allow part to be removed when it is mandatory");

            slotRestrictionManager.SetOptional(slot, true);
            canRemovePart = slot.Inventory.GetMaxPickup(Item.Get<KitchenBaseCabinetBoxItem>(), 1).Val > 0;
            DebugUtils.Assert(canRemovePart, "Slot should allow part to be removed when it is optional");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldOnlyAllowCertainTypesOfPartsIntoInventory()
        {
            InventorySlot slot = TestUtility.CreateInventorySlot();
            WorldObject worldObject = new KitchenCupboardObject();
            PartsContainer partsContainer = new PartsContainer();
            slot.Initialize(worldObject, partsContainer);

            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();

            bool acceptsAnyPart = slot.Inventory.GetMaxAccepted(Item.Get<KitchenCupboardWorktopItem>(), 0).Val > 0;
            DebugUtils.Assert(acceptsAnyPart, "Slot should accept any part until set otherwise");

            slotRestrictionManager.SetTypeRestriction(slot, new Type[] { typeof(KitchenBaseCabinetBoxItem) });

            bool acceptsValidPart = slot.Inventory.GetMaxAccepted(Item.Get<KitchenBaseCabinetBoxItem>(), 0).Val > 0;
            DebugUtils.Assert(acceptsValidPart, "Slot should allow in items on the whitelist");

            bool acceptsInvalidPart = slot.Inventory.GetMaxAccepted(Item.Get<KitchenCupboardWorktopItem>(), 0).Val > 0;
            DebugUtils.Assert(!acceptsInvalidPart, "Slot should not allow in items not on the whitelist");
        }

        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldNotAllowRemovalUnlessStorageIsEmpty()
        {
            InventorySlot slot = TestUtility.CreateInventorySlot();
            WorldObject worldObject = new KitchenCupboardObject();
            PartsContainer partsContainer = new PartsContainer();
            slot.Initialize(worldObject, partsContainer);

            Inventory storage = new LimitedInventory(1);
            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();
            slotRestrictionManager.AddRequiredEmptyStorage(slot, storage);

            bool acceptsPartWhenStorageIsEmpty = slot.Inventory.GetMaxAccepted(Item.Get<KitchenCupboardWorktopItem>(), 0).Val > 0;
            DebugUtils.Assert(acceptsPartWhenStorageIsEmpty, "Slot should accept any part when the storage is empty");

            storage.AddItem(new CornItem());
            bool acceptsPartWhenStorageIsNotEmpty = slot.Inventory.GetMaxAccepted(Item.Get<KitchenCupboardWorktopItem>(), 0).Val > 0;
            DebugUtils.Assert(!acceptsPartWhenStorageIsNotEmpty, "Slot should not accept any part when the storage is not empty");

            storage.Clear();
            slot.TryAddPart(new KitchenCupboardWorktopItem());

            bool canRemovePartWhenStorageIsEmpty = slot.Inventory.GetMaxPickup(Item.Get<KitchenCupboardWorktopItem>(), 1).Val > 0;
            DebugUtils.Assert(canRemovePartWhenStorageIsEmpty, "Slot should allow the part to be removed when the storage is empty");

            storage.AddItem(new CornItem());
            bool canRemovePartWhenStorageIsNotEmpty = slot.Inventory.GetMaxPickup(Item.Get<KitchenCupboardWorktopItem>(), 1).Val > 0;
            DebugUtils.Assert(!canRemovePartWhenStorageIsNotEmpty, "Slot should not allow the part to be removed when the storage is not empty");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTriggerSlotEnabledEvent()
        {
            ISlot slot = TestUtility.CreateSlot();
            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();
            int calls = 0;
            slotRestrictionManager.SlotLockedChangedEvent.Add(s =>
            {
                calls++;
                DebugUtils.AssertEquals(slot, s, "Event was called with wrong slot instance");
            });
            Inventory storage = new LimitedInventory(2);
            slotRestrictionManager.AddRequiredEmptyStorage(slot, storage);
            DebugUtils.Assert(!slotRestrictionManager.IsSlotLocked(slot), "Slot should be not locked when storage is empty");

            storage.AddItem<TestPart>();
            DebugUtils.Assert(slotRestrictionManager.IsSlotLocked(slot), "Slot should be locked when storage is not empty");
            DebugUtils.AssertEquals(1, calls, "Should have triggered the event when inventory became non-empty");

            calls = 0;
            storage.AddItem<CornItem>();
            DebugUtils.Assert(slotRestrictionManager.IsSlotLocked(slot), "Slot should still be locked when storage is not empty");
            DebugUtils.AssertEquals(0, calls, "Should have not triggered the event when inventory remained non-empty");

            calls = 0;
            storage.Clear();
            DebugUtils.Assert(!slotRestrictionManager.IsSlotLocked(slot), "Slot should be not locked when storage is empty");
            DebugUtils.AssertEquals(1, calls, "Should have triggered the event when inventory became empty");
        }
    }

}
