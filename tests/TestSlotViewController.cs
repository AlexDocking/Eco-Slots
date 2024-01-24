using Eco.Core.Tests;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
