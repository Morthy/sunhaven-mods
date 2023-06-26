# Custom Items

Custom Items allows you to add your own custom items to the game. Currently the mod supports:

* Adding generic items
* Adding placeable furniture (including special usecases like beds and tables)
* Making any custom item available for purchase in any shop
* Making any custom item available to craft in any crafting table

You'll need basic familiarity with JSON files to add your own items. You can find examples in the [examples folder](../examples)

## Important things to know

### Item IDs

Every item in the game has a unique numeric item ID. When you add a custom item, you must also choose an ID for your item. 
It's important to make sure your ID doesn't conflict with existing items in the game or items added by other mods.

You can check to see if any Item ID is taking by the game by using the handy website provided by kryzik: https://kryzik.github.io/sun-haven-items/

Right now there's no easy way to know if item IDs will conflict with another mod, and no way to know if a custom item ID will conflict with a future game update.

In the case of a conflict, the custom item will not be added.

### Info and errors

In your BepInEx/config/BepInEx.cfg file, make sure Logging.Console is enabled to show the game console. If there are any errors with custom items, they will be shown here.

It's also recommended to change LogLevels to "All", this will show debug messages that allow you to see if your custom items have been successfully created.

In the beta versions, the mod may not gracefully handle all errors and so you might not get a useful error message if you make a mistake in your item definition. For support please join the discord (https://discord.gg/NCdDfYPPyy).

## Creating a base item

Items are added by creating a JSON definition file and providing a .png image. Both files should be named using the item ID you've chosen. As an example:

* 61000.json
* 61000.png

Any .json file in the CustomItems folder will be considered, but I recommend creating all items for your mod in your own folder within CustomItems. Your directory structure could look like this:

```
plugins/CustomItems/
plugins/CustomItems/CustomItems.dll
plugins/CustomItems/YourModName/
plugins/CustomItems/YourModName/1001.json
plugins/CustomItems/YourModName/1001.png
```

Your item definition file (.json) requires at minimum the following information:

```json
{
  "id": 61000,
  "name": "My custom item",
  "description": "It does custom things",
  "helpDescription": "",
  "useDescription": "(Left click to place)",
  "stackSize": 999,
  "canSell": true,
  "canTrade": true,
  "canTrash": true,
  "goldSellPrice": 300,
  "orbSellPrice": 300,
  "ticketSellPrice": 300,
  "category": 1,
  "rarity": 1
}
```

| Key             | Description                                                                                 |
|-----------------|---------------------------------------------------------------------------------------------|
| id              | The ID of the item. Should match the file name.                                             |
| name            | The name of the item.                                                                       |
| description     | The description of the item, shown in the tooltip.                                          |
| helpDescription | The help description of the item, shown in the tooltip. Can be empty.                       |
| useDescription  | The use description of the item, shown in the tooltip. Can be empty.                        |
| stackSize       | The quantity of the item per stack                                                          |
| canSell         | Whether the item is sellable                                                                |
| canTrade        | Whether the item is tradeable                                                               |
| canTrash        | Whether the item is trashable                                                               |
| goldSellPrice   | The sell price of the item in gold. An item should only have one sell price.                |
| orbSellPrice    | The sell price of the item in orbs. An item should only have one sell price.                |
| ticketSellPrice | The sell price of the item in tickets. An item should only have one sell price.             |
| category        | Item category:<br/>0 = Equip, 1 = Use, 2 = Craftable, 3 = Monster, 4 = Furniture, 5 = Quest |
| rarity          | Item rarity:<br/>0 = Common, 1 = Uncommon, 2 = Rare, 3 = Epic, 4 = Legendary                |

The item image (.png) can be any size, the game will modify it to fit within the item icon. If you're adding custom furniture, this image be of the furniture as it will be shown in the game world (more info in the furniture section).

## Creating custom furniture

You can extend a base custom item to make it act as furniture by adding additional fields to the item definition file.

For furniture, the image you provide (e.g. 61000.png) will be what is displayed in the game world when the furniture is placed.

Keep in mind that Sun Haven is a pixel based game so will enlarge your image when rendered in-game. The furniture will be rendered at around 4x the actual image size.

A furniture definition looks like this. Note that not all decoration fields are required.

```json
{
  "id": 61000,
  "name": "My custom furniture",
  "description": "Furniture",
  "helpDescription": "",
  "useDescription": "(Left click to place)",
  "stackSize": 999,
  "canSell": true,
  "canTrade": true,
  "canTrash": true,
  "goldSellPrice": 300,
  "orbSellPrice": 300,
  "ticketSellPrice": 300,
  "category": 1,
  "rarity": 1,
  "decoration": {
    "size": [12, 12],
    "functionality": "table",
    "tableSize": [10, 8],
    "tableOffset": [1, 2],
    "placeableOnTables": false,
    "placeableOnWalls": false,
    "placeableAsRug": false
  }
}
```


| Key               | Required      | Description                                                                          |
|-------------------|---------------|--------------------------------------------------------------------------------------|
| size              | Yes           | The size (width, height) of the decoration. For more information see "size" below.   |
| functionality     | No            | Specifies extra functionality of the furniture. Current options: <br/>table<br/>bed  |
| placeableOnTables | Yes           | Whether it can be placed on tables.                                                  |
| placeableOnWalls  | Yes           | Whether it can be placed on walls.                                                   |
| placeableAsRug    | Yes           | Whether it can be placed on the floor as a rug.                                      |
| tableSize         | Only if table | The size (width, height) of the surface area of the table where items be can placed. |
| tableOffset       | Only if table | The size offset (width from left, width from bottom) of the above.                   |
| bedSleepOffset    | Only if table | The offset (x, y) as a range between 0-1 of where the player sleeps in the bed.      |

### How do sizes work?

The sizes in the game item definitions are not the same as the width/height of your furniture image.

1 width in the game = 4 pixels in your image. If your image is 48x48px, your decoration size could be 12x12.

**Why could be? The size of the decoration isn't always directly proportional to the size of your image.** The size should represent the base of the decoration item in the 3d world, e.g. where it's actually placed.

For example, a large wardrobe is very tall but you don't want the size to include the full height, only the base surface so that it can be placed up against the wall, or to allow a player to walk behind it.

The size is currently also used to define the collider for the furniture. The collider is the area where the player is blocked from moving into.

## Making custom items available

So you've added a custom item to a game. Without using cheats, how can a player actually acquire them? Currently there are two options.

### Adding a custom item to a shop

Items can be added to as many shops as you wish by adding the following to your item definition file:

```json
	"shopEntries": [
		{
			"shop": "GeneralMerchantTable",
			"amount": 1,
			"costGold": 50,
			"costTickets": 0,
			"costOrbs": 0
		}
	],
```

For a list of possible shops, see here:

### Adding a custom item as a recipe to a crafting table

Items can be added to crafting tables by adding the following to your item definition file:

```json
	"recipes": [
		{
			"list": "RecipeList_Anvil",
			"hours": 1,
			"inputs": [
				{"id": 2002, "amount": 1},
				{"id": 2001, "amount": 1}
			]
		}
	]
```

## Need help? Please join the discord

https://discord.gg/NCdDfYPPyy

