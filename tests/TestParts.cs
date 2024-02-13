using Eco.Core.Controller;
using Eco.Core.Tests;
using Eco.Core.Utils;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.Utils;
using Eco.Shared.View;
using Parts.Kitchen;
using Parts.UI;
using System;

namespace Parts.Tests
{

    [ChatCommandHandler]
    public static class TestParts
    {
        
        private class MySubscriptions : ISubscriptions<ThreadSafeSubscriptions>
        {
            public ThreadSafeSubscriptions Subscriptions { get; } = new ThreadSafeSubscriptions();

            public ThreadSafeSubscriptions GetOrCreateSubscriptionsList() { return Subscriptions; }

            public void ReleaseSubscriptionsList() => Subscriptions.ReleaseSubscriptionsList();
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTriggerColourChangedEventWhenModelPartColouringChanged()
        {
            //set up
            ISubscriptions<ThreadSafeSubscriptions> subscriptions = new MySubscriptions();
            ModelPartColourData colouring = new ModelPartColourData();
            colouring.Colour = Color.White;

            //subscribe
            int calls = 0;
            colouring.Subscribe(subscriptions, nameof(ModelPartColourData.Colour), () => calls += 1);

            //test
            colouring.Colour = Color.Orange;
            DebugUtils.AssertEquals(1, calls, "Change listener did not get called exactly once");
            DebugUtils.AssertEquals(Color.Orange, colouring.Colour, "Did not set colour correctly");
        }

        
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldChangeModelPartColouringColourThroughView()
        {
            //set up
            KitchenBaseCabinetBoxItem part = new KitchenBaseCabinetBoxItem();
            ModelPartColourData colouring = new ModelPartColourData();
            colouring.Colour = Color.White;
            part.ColourData = colouring;
            ColouredPartController view = new ColouredPartController();
            view.SetModel(part);

            //test
            Color targetColour = new Color("#102030");
            view.ColourHex = targetColour.HexRGB();
            DebugUtils.AssertEquals(new Color("#102030"), colouring.Colour, "View did not change model colour properly");

            view.R = 0.5f;
            DebugUtils.AssertEquals(0.5f, colouring.Colour.R, "View did not change model colour correctly");
            DebugUtils.AssertEquals(targetColour.G, colouring.Colour.G, "View did not change model colour correctly");
            DebugUtils.AssertEquals(targetColour.B, colouring.Colour.B, "View did not change model colour correctly");

            view.G = 0.6f;
            DebugUtils.AssertEquals(0.5f, colouring.Colour.R, "View did not change model colour correctly");
            DebugUtils.AssertEquals(0.6f, colouring.Colour.G, "View did not change model colour correctly");
            DebugUtils.AssertEquals(targetColour.B, colouring.Colour.B, "View did not change model colour correctly");

            view.B = 0.7f;
            DebugUtils.AssertEquals(0.5f, colouring.Colour.R, "View did not change model colour correctly");
            DebugUtils.AssertEquals(0.6f, colouring.Colour.G, "View did not change model colour correctly");
            DebugUtils.AssertEquals(0.7f, colouring.Colour.B, "View did not change model colour correctly");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldChangeViewWhenModelPartColouringChanged()
        {
            //set up
            KitchenBaseCabinetBoxItem part = new KitchenBaseCabinetBoxItem();
            ModelPartColourData colouring = new ModelPartColourData();
            colouring.Colour = Color.White;
            part.ColourData = colouring;
            ColouredPartController view = new ColouredPartController();
            view.SetModel(part);

            //test
            Color target = new Color(0.1f, 0.2f, 0.3f);
            colouring.Colour = target;
            DebugUtils.AssertEquals(target.HexRGB(), view.ColourHex, "View did not change to match model");
            DebugUtils.AssertEquals(target.R, view.R, "View did not change to match model");
            DebugUtils.AssertEquals(target.G, view.G, "View did not change to match model");
            DebugUtils.AssertEquals(target.B, view.B, "View did not change to match model");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTriggerOwnGlobalEventWhenModelPartColouringChanged()
        {
            //set up
            ModelPartColourData colouring = new ModelPartColourData();
            colouring.Colour = Color.White;

            int calls = 0;
            Action<ModelPartColourData> callback = (col) => { calls += 1; DebugUtils.AssertEquals(colouring, col, $"Invocation did not have right {nameof(ModelPartColourData)}"); };
            ModelPartColourData.OnColourChangedGlobal.Add(callback);

            //test
            colouring.Colour = Color.Orange;
            ModelPartColourData.OnColourChangedGlobal.Remove(callback);
            DebugUtils.AssertEquals(1, calls, "Global event did not fire correctly");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTriggerPartPropertyChangedGlobalEventWhenModelPartColouringChanged()
        {
            //set up
            KitchenBaseCabinetBoxItem part = new KitchenBaseCabinetBoxItem();
            ISubscriptions<ThreadSafeSubscriptions> subscriptions = new MySubscriptions();
            ModelPartColourData colouring = new ModelPartColourData();
            colouring.Colour = Color.White;
            part.ColourData = colouring;
            ColouredPartController view = new ColouredPartController();
            view.SetModel(part);

            int calls = 0;
            Action<IPart, IPartProperty> callback = (p, property) =>
                        {
                            calls += 1;
                            DebugUtils.AssertEquals(part, p, $"Invocation did not have right {nameof(IPart)}");
                            DebugUtils.AssertEquals(colouring, property, $"Invocation did not have right {nameof(IPartProperty)}");
                        };
            PartNotifications.PartPropertyChangedEventGlobal.Add(callback);

            //test
            colouring.Colour = Color.Orange;
            PartNotifications.PartPropertyChangedEventGlobal.Remove(callback);
            DebugUtils.AssertEquals(1, calls, "Global event did not fire correctly");
        }
    }
}
