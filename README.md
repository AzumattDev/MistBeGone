# Description

## A simple plugin that removes the Mist from Mistlands. Globally. Everywhere. It gives no fucks. Will control anything that uses the vanilla mist components to generate mist. Use global keys or not to control when.

---

`Feel free to reach out to me on discord if you need manual download assistance.`

Shoutout to KuramA#7575 for helping me confirm that this is client only and is not required on the server. However, you
can install the mod on the server as of v1.0.3 to sync the Global Keys configuration to determine when the mist clearing
is available. All Global Keys from "Global Keys Needed" configuration, must be set for mist clearing to occur (as long as Use Global Keys configuration is on!).

# Configurations

Configuration changes should update live

## If on, the configuration is locked and can be changed by server admins only. [Synced with Server]

### Setting type: Toggle

### Default value: On

### Acceptable values: Off, On

Lock Configuration = On

## If on, the mist will only be removed if all required global keys are set. [Synced with Server]

### Setting type: Toggle

### Default value: Off

### Acceptable values: Off, On

Use Global Keys = Off

## Comma-separated list of global keys required. Example: 'defeated_eikthyr,defeated_dragon'

### Accepted values are PlayerDamage, EnemyDamage, WorldLevel, EventRate, ResourceRate, StaminaRate, MoveStaminaRate, StaminaRegenRate, SkillGainRate, SkillReductionRate, EnemySpeedSize, EnemyLevelUpRate, PlayerEvents, Fire, DeathKeepEquip, DeathDeleteItems, DeathDeleteUnequipped, DeathSkillsReset, NoBuildCost, NoCraftCost, AllPiecesUnlocked, NoWorkbench, AllRecipesUnlocked, WorldLevelLockedTools, PassiveMobs, NoMap, NoPortals, NoBossPortals, DungeonBuild, TeleportAll, Preset, NonServerOption, defeated_eikthyr, defeated_dragon, defeated_goblinking, defeated_gdking, defeated_bonemass, activeBosses, KilledTroll, killed_surtling, KilledBat, AshlandsOcean, Count [Synced with Server]

### Setting type: String

### Default value: defeated_eikthyr,defeated_dragon,defeated_gdking,defeated_bonemass,defeated_goblinking

Global Keys Needed = defeated_eikthyr,defeated_dragon,defeated_gdking,defeated_bonemass,defeated_goblinking

# Installation "Modes" explained:

### Client Only:

Clears Mist, gives no fucks (by default). Clients control configurations as they see fit.

### All Clients and Server:

Server will sync global keys configuration down to the clients to determine when to clear the mist. All Global Keys
from "Global Keys
Needed" configuration, must be set for mist clearing to occur (if Use Global Keys is on!).

### All Clients:

Clears Mist, gives no fucks. Clients control configurations. Each client will control their own mist clearing.

### Just Server:

Technically does nothing other than kick the player if they do not have the mod installed.

### Some Clients and Server:

Not possible, anyone who didn't have the mod installed would be kicked from the server when attempting to join without
the mod installed.

# Author Information

### Azumatt

`DISCORD:` Azumatt#2625

`STEAM:` https://steamcommunity.com/id/azumatt/

For Questions or Comments, find me in the Odin Plus Team Discord or in mine:

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/Pb6bVMnFb2)
<a href="https://discord.gg/pdHgy6Bsng"><img src="https://i.imgur.com/Xlcbmm9.png" href="https://discord.gg/pdHgy6Bsng" width="175" height="175"></a>