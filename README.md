# Slots - WIP

World Objects can have slots which can contain one part per slot. Each part can have properties such as defining extra public storage slots, decreases to fuel consumption rate, model colour or any other affect that can be thought up.
It uses a MVC architecture to keep the UI and world affectors decoupled from the parts container, as it is designed to be as flexible and extensible as possible to modders.
When a part is swapped out or a property on the part changes, the autogen UI controller and the affectors are notified through events. The UI will refresh, and the affector will make a change in the world, such as changing the world object's model, disabling the object or changing the power output.
Modders can add new types of part properties and effects, make new autogen UI for their parts or change how the slots are presented in the UI without modifying any of the existing classes.


Parts are not required to be items. Slots therefore do not necessarily have to hold items - modders could make new types of slots that operate differently to '1 item per slot', so long as the slot contains at most 1 'part'. A slot could hold a stack of items, so long as the 'part' is a wrapper for the stack.

The responsibility of what a slot accepts as a valid part belongs with the slot instead of its parent container, as different types of slots may need to implement their restrictions in different ways.


## Key classes and interfaces:
### IPart
Any class which wants to be a part in a slot needs to derive this interface. It has an event for notifying about changes to its part properties, which gets passed all the way back to the parts container and then to any relevant UI or affectors.
### IPartProperty
Defines an attribute of a part, such as how many extra storage slots the object should have if installed, or how clean the solar panel is (for instance).
### ISlot
Stores a single IPart.
### IPartsContainer
Is a list of slots owned by an object. It is stored on the WorldObject's PartsContainerComponent.
### RegularPartsContainerMigrator
Migrates any existing parts container and ensure it is set up with the correct slots and parts.
WorldObjectItems specify their object's slots, restrictions and pre-existing parts with this.
It's WIP as it's currently performing the roles of both migration and schema.
## #SlotsUIComponent
Displays all the slots using views created specifically for each slot. Different types of slot can then be rendered differently, and mods can override how they are displayed in the list.
### StorageSizeModifierComponent, ModelPartColourComponent & ModelReplacerComponent
These components listen for changes to the object's installed parts and make their relevant changes to the object and its other components.