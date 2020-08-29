# CommuniKeys

A mod that adds the ability to define key-to-message mappings so you can quickly send chat messages with a single keypress. 
Perfect for multiplayer sessions without voice communication where quick coordination can make or break a run!

_This mod follows the way how the game sends chat messages, so no commands/malformed input can be sent to other players._

## Installation

Simply copy `CommuniKeys.dll` to your BepInEx plugin folder.

## Configuration

After the game has been started with the mod installed once, you will have a config file available with following options:

### `KeyTextMapping`

This configuration key contains all mappings for message->hotkey combinations. It uses following formats:

A single combination is composed of the `key` and the `message` delimited by a semicolon (`;`). The key needs to
be a valid [Conventional Game Input](https://docs.unity3d.com/Manual/class-InputManager.html). If the given key is invalid,
 you'll know by pressing the key not sending any messages. (the mod will remove any combination causing errors ingame)

You can define multiple messages by separating the above combination with pipes (`|`). With this you can define an
infinite amount of hotkeys and messages!

By default following messages are configured (on the number row):
- `1`: Yes!
- `2`: No!
- `3`: Ready for Teleporter?
- `4`: Ready for next Stage?
- `5`: Shrine of the Mountain?
- `6`: Final Stage?
- `7`: Check for Newt Altars!
- `8`: Don't take all items!

## Changelog

### 1.0.0

- Initial release!


