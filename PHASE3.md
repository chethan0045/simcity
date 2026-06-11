# Phase 3 — Living World & NPCs

**Goal:** make the world feel *alive*. Populate a small town with AI villagers who
run their own lives — eating, sleeping, working, and gathering — driven by
**utility AI** (no scripted routines). This delivers pillar #4 ("a world that
lives") and gives Phase 4 someone to build relationships with.

**Honest scope:** still greybox. NPCs are the customizable capsule avatars from
Phase 2 with random looks. Movement is direct (no pathfinding yet — a NavMesh agent
replaces `SimpleMover` once there's real geometry). Resources/crafting/building from
the design's Phase 3 are deferred to a later pass; this slice focuses on the
headline: **believable autonomous villagers**.

---

## How the AI works (utility AI)
Each NPC has the same `CharacterNeeds` as the player (hunger/energy decaying over
game-time). A `NpcBrain` scores a set of actions every ~1.25s and runs the best:

| Action | Wins when… | Does |
|---|---|---|
| **Sleep** | energy low, or it's night | walk to a Bed, restore energy |
| **Eat** | hunger low | walk to the Diner, restore hunger |
| **Work** | daytime, rested & fed | walk to the Workshop, put in a shift (tiring) |
| **Socialize** | daytime, otherwise idle | gather at a bench/plaza for a bit |
| **Wander** | nothing better | stroll to a random spot (default) |

Behavior emerges: villagers work by day, eat when hungry, head to bed at night,
and mill around the plaza in between — all from need scores, nothing scripted.

---

## Files added this phase
```
AI/SimpleMover.cs               direct ground movement (NavMesh later)
AI/UtilityAction.cs             base class: Score → OnEnter → GetTarget → Perform
AI/NpcActions.cs                Eat / Sleep / Work / Socialize / Wander
AI/NpcBrain.cs                  scores actions each tick, runs the best
World/Amenity.cs                Food/Bed/Work/Social markers + nearest-of-type registry
World/TownWorld.cs              builds the town + facilities + NPCs + player
World/WorldCommon.cs            shared lighting/clock/box helpers (refactor)
World/PlayerFactory.cs          shared player-rig + HUD builder (refactor)
UI/CharacterCreatorLauncher.cs  shared creator launch (refactor out of Phase 2)
Bootstrap/Phase3Bootstrap.cs    entry: creator → save → TOWN (auto-runs)
```
Refactor: `ApartmentWorld` and `Phase2Bootstrap` now use the shared helpers;
`Phase2Bootstrap` auto-run is disabled so only `Phase3Bootstrap` runs.

---

## How to run
Setup per `README.md`. Press **Play** → create your character (first run) → spawn
into the town. Then just **watch**: villagers should head to the workshop during the
day, the diner when hungry, beds at night, and the plaza benches in between. The
facilities are shared — you can walk up and **E** to use the Diner / Workshop / Beds
yourself. Speed up time by lowering `GameClock.secondsPerDay` to watch a full
day/night cycle of behavior quickly. (F9 resets your character.)

Optional: tick `verboseLogging` on an NPC's `NpcBrain` to see its decisions in the
Console.

---

## Success criteria (exit Phase 3 when…)
1. Multiple NPCs move around and visibly satisfy their own needs.
2. Their behavior shifts believably with time of day (work → eat → sleep).
3. The town *reads as alive* when you just stand and watch.

## Next: Phase 4 — Relationships
Give NPCs (and the player) a social graph: meet, befriend, romance. Build on the
Socialize action so gatherings actually form bonds. See `GAME_DESIGN.md` §5 / §12.
