using Eco.Core.Tests;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Parts.UI;
using System.Collections;
using System.Collections.Generic;
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
            InventorySlotController view = new InventorySlotController(slot);
            view.SlotInventory.AddItem(new TestPart());

            DebugUtils.AssertEquals(typeof(TestPart), slot.Part?.GetType(), "View should have updated the slot with the new part");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldUpdateViewWhenSlotChanges()
        {
            InventorySlot slot = TestUtility.CreateInventorySlot();
            InventorySlotController view = new InventorySlotController(slot);
            slot.SetPart(new TestPart());

            DebugUtils.AssertEquals(typeof(TestPart), view.SlotInventory.Stacks?.First().Item?.GetType(), "View should have updated itself when the slot changed");
        }
        //TODO
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

        private class MockSlot : ISlot
        {
            public string Name { get; }
            public IPart Part { get; }
            public ISlotDefinition SlotDefinition { get; }
            public IPartsContainer PartsContainer { get; }
            public ThreadSafeAction NewPartInSlotEvent { get; } = new ThreadSafeAction();
            public ThreadSafeAction<ISlot, IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<ISlot, IPart, IPartProperty>();
            public Result CanAcceptPart(IPart validPart) => default;
            public Result CanRemovePart() => default;
            public Result CanSetPart(IPart part) => default;
            public void Initialize(WorldObject worldObject, IPartsContainer partsContainer) { }
            public bool SetPart(IPart part) => default;

            public LocString Tooltip() => default;

            public Result TryAddPart(IPart part) => default;
            public Result TrySetPart(IPart part) => default;
        }
        private class FakeSlotViewCreator : SlotViewFactory
        {
            public string MockSlotViewName { get; set; } = "MockSlotView";
            public override object CreateView(ISlot slot)
            {
                object view = base.CreateView(slot);
                if (view != null) return view;
                switch (slot)
                {
                    case MockSlot: return MockSlotViewName;
                    default: return null;
                }
            }
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldUseCorrectViewTypeForSlot()
        {
            SlotsUIComponent uiComponent = new SlotsUIComponent();
            uiComponent.ViewCreator = new FakeSlotViewCreator();

            uiComponent.CreateViews(new PartsContainer(new ISlot[] { new MockSlot() }));
            IEnumerable<object> views = uiComponent.Views;
            DebugUtils.AssertEquals(1, views.Count(), "Should only be one view for the one slot");
            DebugUtils.AssertEquals("MockSlotView", views.FirstOrDefault(), "Should have used view creator to return the right view object for the slot");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldReplaceViewsIfCalledTwice()
        {
            SlotsUIComponent uiComponent = new SlotsUIComponent();
            FakeSlotViewCreator slotViewCreator = new FakeSlotViewCreator();
            slotViewCreator.MockSlotViewName = "ViewType1";
            uiComponent.ViewCreator = slotViewCreator;

            PartsContainer partsContainer1 = new PartsContainer(new ISlot[] { new MockSlot() });
            uiComponent.CreateViews(partsContainer1);
            IEnumerable<object> views = uiComponent.Views;
            DebugUtils.AssertEquals(1, views.Count(), "Should only be one view for the one slot");
            DebugUtils.AssertEquals(slotViewCreator.MockSlotViewName, views.LastOrDefault(), "Should have used view creator to return the right view object for the slot");

            slotViewCreator.MockSlotViewName = "ViewType2";
            PartsContainer partsContainer2 = new PartsContainer(new ISlot[] { new MockSlot() });
            uiComponent.CreateViews(partsContainer2);
            views = uiComponent.Views;
            DebugUtils.AssertEquals(1, views.Count(), "Should only be one view for the one slot");
            DebugUtils.AssertEquals(slotViewCreator.MockSlotViewName, views.LastOrDefault(), "Should have used view creator to return the right view object for the slot");

            DebugUtils.Assert(!partsContainer1.Slots[0].NewPartInSlotEvent.Any, "Should have removed callback when creating the view for the first slot when replacing it with view for the second slot");
            DebugUtils.Assert(partsContainer2.Slots[0].NewPartInSlotEvent.Any, "Should have added a callback to refresh the view when the slot gets a new part");
        }
    }
}
