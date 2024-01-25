using Eco.Core.Tests;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Linq;

namespace Parts.Tests
{
    [ChatCommandHandler]
    public class TestSlotViewController
    {
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldUpdateSlotWhenViewInventoryChanges()
        {
            InventorySlot slot = TestUtility.CreateInventorySlot();
            SlotViewController view = new SlotViewController(slot);
            view.SlotInventory.AddItem(new TestPart());

            DebugUtils.AssertEquals(typeof(TestPart), slot.Part?.GetType(), "View should have updated the slot with the new part");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldUpdateViewWhenSlotChanges()
        {
            InventorySlot slot = TestUtility.CreateInventorySlot();
            SlotViewController view = new SlotViewController(slot);
            slot.SetPart(new TestPart());

            DebugUtils.AssertEquals(typeof(TestPart), view.SlotInventory.Stacks?.First().Item?.GetType(), "View should have updated itself when the slot changed");
        }
        /*[CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldGiveCorrectErrorMessageWhenAddingWrongPartType()
        {
            ISlotDefinition slotDefinition = new RegularSlotDefinition()
            {
                AllowedItemTypes = new Type[] { typeof(TestPart) },
            };
            InventorySlot slot = TestUtility.CreateInventorySlot(slotDefinition);
            WorldObject worldObject = new TestWorldObject();
            worldObject.InitializeForTest();
            slot.Initialize(worldObject, PartsContainerFactory.Create());

            SlotViewController view = new SlotViewController(slot);
            Result result = view.SlotInventory.GetMaxAccepted(new TestPart(), 0);
            foreach (InventoryRestriction r in view.SlotInventory.Restrictions) Log.WriteLine(Localizer.Do($"SlotViewRestriction:{r.GetType()}"));

            DebugUtils.Assert(result.Failed, "Should not be allowed to add invalid part type");
            LimitedTypeSlotRestriction slotRestriction = slotDefinition.RestrictionsToAddPart.FirstOrDefault(restriction => restriction is LimitedTypeSlotRestriction) as LimitedTypeSlotRestriction;
            DebugUtils.AssertEquals(slotRestriction.Describe(), result.Message, "Error message did not match expected");
        }*/
    }
}
