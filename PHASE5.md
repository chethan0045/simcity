# Phase 5 — In-Game Economy (soft currency)

**Goal:** turn "earning a living" into a real craft-and-sell economy. Make goods at
a workbench, sell them at the market for Coins, and build seller **reputation** that
raises your prices. Still **soft currency** — real-money cash-out is Phase 7. This is
the economic spine the real marketplace later sits on (`GAME_DESIGN.md` §6).

**Honest scope:** the loop (craft → carry → sell → reputation) is real and testable.
Recipes/materials, NPC customers with demand, and player-to-player trade are deferred
(the latter needs co-op, Phase 6). The market currently represents selling to the town.

---

## The expanded earn loop
```
CRAFT at the Workbench   → spend time + energy, get a good (Trinket/Pottery/Furniture)
SELL at the Market        → goods → Coins (minus a 10% fee), reputation rises
REPUTATION ↑              → higher price multiplier (up to +50%) on future sales
…feeds the Phase 1 loop   → Coins pay for food and rent
```
You still have the quick **Work** job (direct Coins) from before — crafting+selling is
the higher-effort, higher-upside path that also builds standing.

---

## Files added / changed
```
Economy/ItemCatalog.cs        the craftable goods + base values (static for now)
Economy/Inventory.cs          goods the player carries
Economy/SellerProfile.cs      reputation (0..100) → price multiplier; totals
Interaction/CraftInteractable.cs   Workbench: time+energy → a good
Interaction/MarketInteractable.cs  Market: sell all goods for Coins, minus fee
— PlayerFactory: player gains Inventory + SellerProfile; HUD wired
— TownWorld: adds a Workbench and a Market station
— PlayerHud: shows goods carried + reputation
```

---

## How to run
Setup per `README.md`. Press **Play**, create your character, spawn into town. Then:
- Look at the **Workbench** → **E** to *Craft* (makes a random good; costs time/energy).
  Repeat to stock up; watch "Goods" climb in the HUD.
- Look at the **Market** → **E** to *Sell goods* (converts everything to Coins,
  minus the fee). Watch Coins jump and **Rep** tick up in the HUD and Console.
- Keep selling — as reputation rises, the same goods fetch more Coins.
- Use those Coins at the Diner and to cover rent (the Phase 1 stakes still apply).

---

## Success criteria (exit Phase 5 when…)
1. Craft → sell → Coins works, and the HUD reflects goods, Coins, and reputation.
2. Reputation visibly improves your sale prices over time.
3. The economy gives a satisfying alternative to the plain Work job.

## Next: Phase 6 — Co-op multiplayer
Netcode for GameObjects + Relay + Lobby: friends join by code, characters/world/NPCs
sync, and the market becomes player-to-player. See `GAME_DESIGN.md` §9 / §12.
(Big jump in complexity — strongly validate single-player first.)
