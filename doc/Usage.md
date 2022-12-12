# Usage of the plugin

## Overview

After the installation of the Plugin, a new category named "SimHub" will be available in Stream Deck. Use the actions of this category to get Stream Deck buttons, which will update their state from SimHub properties.


## Actions offered by this plugin

### Hotkey

This action offers the same functionality as the built-in action "Hotkey": If the Stream Deck button is pressed, a keypress event will be sent to the active window. Please see the structure [VirtualKeyShort](/PluginNative/Tools/Keyboard.cs) for supported key names. For alphanumeric keys (0-1, A-Z) the prefix `KEY_` can be omitted, so either `KEY_P` and `P` will have the same result. Some example of valid values for the field "Hotkey" are:

- A (will send the event for the key "A" to the active window)
- KEY_A (same as above)
- SPACE (will send an event for the space key)
- F11 (event for F11)

Modifiers like Ctrl, Alt and Shift have to be specified by using the corresponding checkbox.

Please note, that not all entries in "VirtualKeyShort" make sense in the field "Hotkey".

The added value for this action is in the field "SimHub Property": It allows to enter the name of a SimHub property. The value of this SimHub property will update the state of the button. See [SimHub Property Server](https://github.com/pre-martin/SimHubPropertyServer) for a list of valid property names.

The logic for the action state in dependency from the SimHub property value is as follows:

- `boolean`: If the property value is `True`, the action will be in the "on" state, otherwise in the "off" state.
- `integer` and `long`: If the property value is greater than zero, the action will be in the "on" state, otherwise in the "off" state.

Examples:

- Button to toggle the Ignition in ACC (which is mapped to "Shift + I" by default):
  ![Ignition](Example-Ignition.png)
- Button to toggle the Engine in ACC (which is mapped to "S" by default):
  ![Engine](Example-Engine.png)
- Another useful button for ACC would be the Hotkey "Alt + L" with the SimHub property "gd.sdb.PitLimiterOn" to toggle the pit limiter.

### Hotkey 4-state

This action is the same as the "Hotkey" action: It sends a keystroke to the active window, and it can be connected to a SimHub property, which will update its state.

The difference is that this action can have up to four states. The logic for the action state in dependency from the SimHub property value is as follows:

- `boolean`: If the property value is `True`, the action will be in the state "1", otherwise in the state "0" state.
- `integer` and `long`: The property value will directly set the state. The value "0" maps to the action state "0", "1" to the action state "1" and so on.

Unfortunately, the Stream Deck UI support is very limited for actions which have more than two states. "Title" and icon can only be customized for the first two states. Maybe Stream Deck will enhance their UI one day to allow customization of all states. 
