# Phase 4 — Relationships

**Goal:** turn the town's villagers from background life into people you *bond
with*. A social graph links every character (player + NPCs); bonds grow from being
together and from talking, climb through tiers (Stranger → … → Best Friend), and can
become romance (Partner). This delivers `GAME_DESIGN.md` §5.

**Honest scope:** the social *systems* are real and testable. Conversation is a
single "Talk" action (no dialogue trees yet) and family lines aren't modeled. Those
are later passes; this slice proves bonds form and matter.

---

## How relationships work
- Every character has a **`Relationships`** map: a value in [-100, 100] toward each
  other character, plus a **partner** flag.
- **`SocialSystem`** ticks once a second: any two characters standing within ~2.5m
  grow their mutual bond, scaled by **sociability**. So Phase 3's gatherings (and you
  hanging around an NPC) actually *build* relationships.
- **Talk (press E on a villager):** a direct boost — the prompt shows their name and
  current tier, e.g. `Talk — Mia (Friend)`.
- **Tiers:** Stranger → Acquaintance → Friend → Close Friend → Best Friend
  (and Disliked/Rival below zero).
- **Romance:** when two available adults both reach ≥80, they become **Partners**
  (logged with a ♥). One partner at a time.

Because bonding is proximity-driven and villagers already gather by the AI from
Phase 3, **relationships form on their own** as you watch — and you can steer your
own by spending time with, and talking to, the villagers you like.

---

## Files added / changed
```
Social/Relationships.cs     per-character bond values + partner flag
Social/Character.cs          identity (id, name, sociability) + global registry
Social/RelationshipTier.cs   value -> tier name
Social/SocialGraph.cs        symmetric bond changes + romance promotion
Social/SocialSystem.cs       proximity bonding tick (drives organic relationships)
Interaction/NpcInteractable.cs   "Talk" to actively build a bond
— PlayerFactory: player now has Character + Relationships; HUD shows them
— TownWorld: NPCs get Character + Relationships + a collider + Talk; spawns SocialSystem
— PlayerHud: a live "Relationships" list (name — tier — value)
```

---

## How to run
Setup per `README.md`. Press **Play**, create your character, spawn into town. Then:
- **Walk up to a villager and press E** to Talk — watch the tier climb in the prompt
  and in the **Relationships list** (top-left of the HUD).
- **Stand near villagers / in the plaza** — bonds grow just from proximity.
- **Watch the Console** for `[Romance] … are now partners ♥` as villagers who spend
  lots of time together pair up.
- Lower `GameClock.secondsPerDay` and `SocialSystem.tickInterval`/raise `baseRate`
  to see relationships evolve faster while testing.

---

## Success criteria (exit Phase 4 when…)
1. Talking to and being near a villager visibly raises the relationship/tier.
2. NPCs form bonds (and the occasional partnership) on their own over time.
3. The HUD relationship list makes the social web legible.

## Next: Phase 5 — In-game economy
Coins, crafting goods, selling to NPCs, a marketplace + reputation — the earn→spend
loop with a real economy (still soft currency; real money is Phase 7). See
`GAME_DESIGN.md` §6 / §12.
