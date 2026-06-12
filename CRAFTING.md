# Crafting & Materials (economy deepening)

A deepening of the Phase 5 economy (and the resources/crafting deferred from Phase 3): the
crafting loop is no longer "press the Workbench → get a random good from nothing." Now you
**gather raw materials** from the world and **craft finished goods by recipe**, then sell the
goods. Inputs cost less than the output is worth, so effort and planning pay off.

## The loop
```
GATHER   trees → Wood · rocks → Stone · bushes → Fiber · clay pit → Clay   (E; nodes deplete, regrow daily)
CRAFT    Workbench → choose a recipe you have the materials for (time + energy)
SELL     Market → sell finished GOODS for Coins (raw materials stay in your bag as inputs)
```

## Recipes (`Economy/ItemCatalog.cs`)
| Good | Value | Recipe |
|---|---|---|
| Basket | 16 | 4 Fiber |
| Trinket | 12 | 2 Fiber + 1 Wood |
| Pottery | 18 | 3 Clay |
| Furniture | 30 | 4 Wood + 2 Stone |
| Statue | 45 | 3 Stone + 2 Clay |

Materials carry small values (Wood 3 · Stone 4 · Fiber 2 · Clay 3) and aren't sold at the
market — they're crafting inputs. The catalog is still static; the natural next step is
data-driven ScriptableObjects with tiers and skill gating.

## What changed
```
Economy/ItemCatalog.cs            ItemKind (Material/Good), materials, goods, Recipes table + helpers
Interaction/ResourceInteractable  gatherable node: harvest a material; depletes, regrows each day
Interaction/CraftInteractable     now opens the crafting menu (was: craft a random good)
UI/CraftingScreen.cs              modal recipe list — have/need counts, craft the ones you can
Interaction/MarketInteractable    sells finished GOODS only (keeps raw materials)
UI/PlayerHud.cs                    shows Goods and Materials on separate lines
World/TownWorld.cs                 adds resource nodes (trees/rocks/bushes/clay) to the shared town
```
Because the nodes live in `TownWorld.BuildSharedEnvironment`, they appear in **co-op** too.

## How to try it
Press Play → spawn → walk to a **tree/rock/bush/clay pit** and press **E** to gather. Go to the
**Workbench** → **E** to open crafting; recipes you can afford are green — click **Craft**. Carry
the goods to the **Market** → **E** to sell. Watch Goods/Materials in the HUD and the
`[Gather] [Craft] [Market]` Console logs.

## Deferred (next steps)
- NPC gathering/crafting + material demand (NPCs only consume time-of-day routines today).
- Data-driven recipes (ScriptableObjects), skill/tool tiers, recipe discovery.
- Crafting goods directly into the player marketplace listing (ties to co-op P2P trade).
