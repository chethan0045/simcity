# Phase 1 — Embodied Vertical Slice ("is it fun?")

**Goal:** prove that *living an ordinary character's day, in first person, is
satisfying* — before we invest in art, NPCs, multiplayer, or the real-money
economy. If the core loop below isn't fun solo with greybox cubes, no amount of
realism will save it. This is the phase that decides the game.

**Tone target:** *Nobody – The Turnaround* — an ordinary person who starts with
little, works to get by, and slowly climbs. See `GAME_DESIGN.md` §1.1 / §6.1b.

---

## The core loop being tested

```
        ┌──────────────────────────────────────────────┐
        │   WORK the desk   →  earn Coins, spend energy  │
        │        ↓                                       │
        │   EAT at fridge   →  restore hunger, -Coins    │
        │        ↓                                       │
        │   SLEEP in bed    →  restore energy, skip to AM │
        │        ↓                                       │
        │   NEW DAY         →  rent is charged            │
        └──────────────────────────────────────────────┘
   Needs (hunger, energy) decay over game-time the whole while.
   Money in vs. money out is the tension. Don't go broke; don't burn out.
```

If a player naturally falls into "I should work a bit, then eat, then sleep,
then make rent" — and *cares* about the balance — the slice succeeds.

---

## Scope

### Must-have (the slice)
- **First-person movement + interaction** — reused from Phase 0.
- **Needs:** Hunger + Energy, decaying over game-time (`CharacterNeeds`).
- **Game clock:** day/time, day rollover, time-skipping; fakes a day/night sun
  (`GameClock`).
- **Eat** (`FoodInteractable`): +hunger, −Coins.
- **Sleep** (`BedInteractable`): skip to morning, +energy (you wake hungrier).
- **Work** (`WorkInteractable`): skip a shift, −energy, +Coins.
- **Money** (`Wallet`) + **daily rent / debt** (`Housing`) — the cost-of-living stakes.
- **HUD** (`PlayerHud`): need bars, Coins, day/time, rent-debt warning.
- **A tiny apartment** built at runtime (`Phase1Bootstrap`) — bed, fridge, desk.

### Explicitly NOT in Phase 1 (later phases)
- Real art / animation (greybox cubes only — Phase 8).
- NPCs / AI villagers (Phase 3) · relationships (Phase 4).
- The marketplace & real money (Phases 5 & 7) — `Wallet` is just a stub for now.
- Save/load, settings, audio, mobile/touch input, the new Input System.
- A real UGUI HUD (OnGUI is a stand-in).

---

## Files added this phase
```
Core/GameClock.cs                 day/time, skip, day-rollover + decay events
Stats/CharacterNeeds.cs           hunger/energy, decay over game-time
Economy/Wallet.cs                 Coins (soft currency stub)
World/Housing.cs                  daily rent + debt
Interaction/BedInteractable.cs    sleep
Interaction/FoodInteractable.cs   eat
Interaction/WorkInteractable.cs   work a shift
UI/PlayerHud.cs                   OnGUI need bars + money + clock
Bootstrap/Phase1Bootstrap.cs      builds the apartment + wires it all (auto-runs)
```
`Phase0Bootstrap` auto-run is disabled so it doesn't race Phase 1.

---

## How to run
Same as Phase 0 (see `README.md`). With the Unity project set up, press **Play** —
the apartment builds itself. Then:
- Look at the **Desk** → press **E** to *Work* (earn Coins, get tired, time passes).
- Look at the **Fridge** → **E** to *Eat* (spend Coins, restore hunger).
- Look at the **Bed** → **E** to *Sleep* (jump to morning, restore energy).
- Watch the **need bars** (top-left), **Coins/clock** (top-right), and the Console
  for `[Work]`/`[Eat]`/`[Sleep]`/`[Rent]` logs. Survive a few days without going
  broke.

---

## Tuning knobs (balance for "fun")
All exposed as public fields, easy to tweak in the Inspector or in code:

| Knob | Where | Default | Effect |
|---|---|---|---|
| `secondsPerDay` | GameClock | 900 (15 min) | pace of the whole sim; lower = faster days |
| Hunger `decayPerHour` | CharacterNeeds | 4 | how often you must eat |
| Energy `decayPerHour` | CharacterNeeds | 3 | passive tiredness |
| `hungerRestored` / `cost` | FoodInteractable | 35 / 6 | food value vs. price |
| `energyRestoredPerHour` | BedInteractable | 12 | sleep effectiveness |
| `shiftHours` / `payPerHour` / `energyCostPerHour` | WorkInteractable | 4 / 10 / 8 | the job's risk/reward |
| `dailyRent` | Housing | 20 | cost-of-living pressure |

Starting balance check (rough): a 4h shift earns **40 Coins**, rent is **20/day**,
a meal is **6**. So one shift ≈ rent + ~3 meals — survivable but tight, which is
the intended *Nobody* feel. Tune until the daily decision actually matters.

---

## Success criteria (exit Phase 1 when…)
1. You can play several in-game days: work, eat, sleep, pay rent — without bugs.
2. The hunger/energy/money balance creates a real decision each day.
3. It's *enough fun* to make you want to do it again. ← the whole point.

If #3 is weak, fix the loop here (pacing, stakes, feedback) **before** Phase 2.

## Next: Phase 2
Character creation & persistent avatar (gender + custom body), then Phase 3's AI
NPCs to make the world feel alive. See `GAME_DESIGN.md` §12.
