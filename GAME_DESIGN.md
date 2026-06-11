# Game Design Document — *(working title: TBD)*

> A grounded, co-op, first-person life simulation with a real player-driven
> economy. You and a few friends create custom characters and *live another life*
> together in a shared 3D world — start with little, work jobs, pay your way, meet
> needs, build relationships, and earn a living by crafting goods and offering
> services to other players, with real-money cash-out for verified sellers.
> Realistic, ordinary-life stakes, told from inside the world.

**Status:** Design / pre-development · **Last updated:** 2026-06-09

> **⚠️ Design history:** Began as a 2D *god-observer* village sim → pivoted to a
> **3D, first-person, co-op life sim** → added a **real-money player marketplace**.
> The god/observer role and 2D/PixiJS stack are **dropped**.

---

## 1. Vision

| | |
|---|---|
| **Genre** | Life simulation · first-person · co-op · player economy |
| **Perspective** | Fully embodied **first-person** — you are a character in the world |
| **Multiplayer** | **Co-op**, 2–8 friends per session, host-authoritative |
| **Characters** | Custom creation: **gender + custom body design** |
| **Economy** | Player marketplace for in-game goods & services; **real-money cash-out for verified sellers** |
| **Monetization** | Cosmetic IAP (premium currency) + marketplace transaction fees |
| **Platforms** | Mobile (iOS/Android) + Desktop primary; Web a stretch goal *(§11)* |
| **Art / fidelity** | **Scalable realism** — near-photoreal on desktop/high-end, dialed down on mobile; one game *(§8)* |
| **Engine** | **Unity** (Netcode for GameObjects + Relay/Lobby) |
| **Scope** | Ambitious full game + a marketplace/payments platform — phased |
| **Touchstones** | **inZOI** (realism bar), **Nobody – The Turnaround** (ordinary-life grind), **The Sims 4** (systems depth); co-op & economy from Stardew, Second Life, Roblox |

**The fantasy:** "My friends and I made our characters and started with almost
nothing. I worked odd jobs to make rent, got good at building, and other players
started paying me to design their homes — until I cashed out my first real payout
from a hobby I love."

### 1.1 Positioning vs. reference games

| Reference | What it nails | What we take | Their gap = our edge |
|---|---|---|---|
| **inZOI** | Photoreal modern life, deep customization | Visual aspiration for our **desktop tier**; modern city-life feel | Single-player; **PC-only (Unreal 5)**; no economy |
| **Nobody – The Turnaround** | Realistic ordinary-life struggle: start poor, jobs, rent, skills | **Core tone** — rise from humble beginnings; cost-of-living stakes | Single-player; no co-op; no real economy |
| **The Sims 4** | Deep, broad life sim; relationships, careers, families, mods | **Systems-depth target** | Single-player; no real money; dated realism |
| **The Guild 3** | Career/business + generational progression | Long-term progression & status climb | Niche, clunky, no modern realism |
| **BitLife** | Deep life choices: education, career, marriage, investments | Breadth of life/finance decisions | Text-only; no immersion |

**Our wedge — nobody combines all three:** grounded **first-person realism** +
**drop-in co-op with friends** + a **real-money player economy**. Every reference
above is **single-player**, and **none** let you earn real money from what you make.
That trio is our identity.

> **Realism reality check:** inZOI achieves its look in **Unreal Engine 5 on gaming
> PCs**. Per our *scalable realism* decision (§8), inZOI-level fidelity is the
> aspiration for our **high-end desktop tier only** — mobile will be a scaled-down,
> still-attractive version. If matching inZOI's look on PC ever outranks mobile +
> co-op in priority, that's the trigger to revisit the Unity-vs-Unreal choice (we
> picked Unity precisely *for* mobile + co-op).

---

## 2. Design pillars

1. **You live it.** First-person embodiment. You don't watch a life — you have one.
2. **Better with friends.** Designed around 2–8 people living together; cooperation
   and social dynamics are the heart.
3. **Emergent stories.** Drama emerges from needs, relationships, and choices —
   never scripted.
4. **A world that lives.** AI NPC villagers populate the world so it feels alive
   even with few players online.
5. **What you make has real value.** Skill and creativity translate into in-game
   income — and, for verified sellers, real money. Creators are first-class.

If a feature doesn't serve at least one pillar, it's out.

---

## 3. Core loop

```
LIVE   → first-person; meet needs (eat, sleep, rest, health) by interacting
DO     → gather, craft, build, farm, work a role/job, decorate
RELATE → friends & NPCs: friendship, romance, rivalry, family, shared projects
EARN   → sell goods & services to other players for Coins → cash out (if verified)
GROW   → skills, home/settlement, reputation, relationships, and wealth deepen
└────── (loop)
```

**Session shape**
- **Solo / offline:** live your character, progress, prep & manage, idle earnings (§8).
- **Co-op:** host opens a world, friends join by code; live, build, trade in real time.

---

## 4. Character system

### 4.1 Creation
- **Gender & identity:** choose gender presentation; **cosmetic/social only — no
  gameplay lockouts by gender** (recommended).
- **Custom body design:** body shape/height/build via blendshapes/sliders, face,
  skin, hair, clothing — on a **modular humanoid avatar** (UMA or custom rig) so
  customization and animation share one skeleton.
- **Origin / starting status:** see §6.
- **Persistence:** your character (look + progress + wallet + reputation) travels
  into any co-op session you join.

### 4.2 The living character (player & NPC share this model)
- **Needs:** hunger, energy/sleep, hygiene, social, fun, health.
- **Skills** (improve with use): cooking, building, farming, crafting, fishing,
  medicine, design, etc. — skills feed directly into what you can **sell**.
- **Personality traits** (NPCs; flavor/buffs for players).
- **Mood & memory** color mood and (for NPCs) decisions.

### 4.3 NPC AI
- **Utility-based decision making:** each tick NPCs score actions against
  needs/traits and act — behavior emerges. NPCs form relationships and can be
  customers for player services.

---

## 5. Relationship system
- **Social graph** across all characters: acquaintance → friend → close / partner,
  plus rivalry and family.
- **Interactions** (talk, help, gift, share a meal, conflict) shift values;
  personality/context modulate outcomes.
- **Romance & family:** partnerships; optionally children/family lines (later, §11).
- **Reputation** ties into the economy: good sellers build standing that attracts
  customers.

---

## 6. Economy, starting status & player marketplace

### 6.1 Starting financial status — *Origin* at character creation
Pick an **Origin** that sets starting Coins, a starter dwelling, and a small perk —
adds replayability and ties into the role system:

| Origin | Starting Coins | Starts with | Trade-off |
|---|---|---|---|
| **The Drifter** | ~50 | tent, clothes on your back | + foraging/survival bonus · rags-to-riches |
| **The Worker** | ~250 | small cabin + basic tools | + one job-skill bonus · balanced |
| **The Heir** | ~1,000 | furnished starter home | no skill bonus, higher upkeep · gentle start |

All players share one currency, **Coins**. The core arc is climbing from your start.

### 6.1b Cost of living — the daily-grind stakes *(Nobody-inspired)*
Earning only matters if money is *needed*. Recurring expenses create the
ordinary-life pressure that makes the grind meaningful:
- **Rent / housing upkeep**, **utilities/bills**, **food**, tool repair, and
  optional wants (better clothes, a nicer home).
- Falling behind has soft, recoverable consequences (downgrade housing, mood/needs
  hit) — **stakes, not punishment**; never pay-to-not-lose with real money.
- Difficulty/economy is **tuned around active play**, with offline costs paused or
  reduced so logging off never penalizes you (ties to §9 offline rules).
- Honest scope note: keep this **light and motivating**, not a spreadsheet — the
  fantasy is "living a life," not "doing chores."

### 6.2 The player marketplace
- **Goods:** crafted items, décor, blueprints, decorated homes, cosmetic creations.
- **Services:** in-world labor — build my house, farm my land, design my outfit,
  guide/tutor a new player, host an event.
- **Fixed, known prices — no chance/randomness anywhere.** This is commerce, **not
  gambling** (and no loot boxes).
- **Platform fee** on each transaction = core revenue.
- **Reputation & reviews** surface trustworthy sellers.

### 6.3 Real-money model (decided)
- **In-game only:** all goods/services are virtual/in-world (no real-world delivery).
- **Two currency flows, deliberately separated:**
  - *Buy Coins* with real money (purchased Coins) — spendable, **not cashable**.
  - *Earn Coins* by selling (earned Coins) — spendable **and** cashable.
- **Cash-out is for verified sellers only, on earned Coins only.** Requires KYC,
  meets payout thresholds/holding periods, age-gated (no minors).
- **The golden rule:** *you can only cash out money you earned by selling, never
  money you bought* — this is the primary anti-fraud / anti-money-laundering control.

### 6.4 Monetization (v1)
- **Cosmetic IAP:** premium currency buys cosmetic/expressive items only
  (outfits, hair, body/face extras, décor, emotes) — **never power; never
  pay-to-win** (protects the co-op core).
- **Marketplace transaction fees** on player-to-player sales.
- Later (optional): cosmetic **season pass**; *earnable* convenience (never raw power).
- **Gameplay-affecting Coins are earned in-game, not bought.**

### 6.5 Payments & compliance (do NOT build in-house)
- **Use a payments/marketplace provider** for purchases + payouts: **Stripe
  Connect**, **PayPal**, or **Tilia** (purpose-built for game economies w/ cash-out).
- Provider/process handles: **KYC/AML**, **tax reporting** (e.g. US 1099-K),
  chargebacks/disputes, payout rails.
- **App-store reality:** in-app purchases of currency must use Apple/Google IAP
  (~30%) with steering limits → **sell Coins and run cash-out on the web**, keep
  in-app to browsing/spending. Design around this from day one.
- **Needs legal review** before launch — this is a regulated, money-handling feature.

---

## 7. World & gameplay systems
- **3D environment:** hand-crafted valley/island — terrain, biomes, day/night,
  weather/seasons affecting needs and activities.
- **Resources & crafting:** gather → craft tools/furniture/food → build & upgrade.
- **Building:** players place/upgrade homes and shared structures (grid or free —
  TBD); builds are sellable goods/services.
- **Roles/jobs:** farmer, builder, cook, healer, designer… structure progression,
  co-op division of labor, and what you can sell.
- **Progression:** skills, home/settlement growth, reputation, wealth, and the
  relationship web — no hard "win" (living-world game).

---

## 8. Art direction & graphics

**Target: scalable realism.** One game whose visual fidelity scales with the
device — near-photorealistic on desktop & high-end phones, gracefully dialed
down on weaker phones. "Realistic" as in believable materials, lighting, and
proportions — *grounded*, not cartoon. Reference bar: Palia / Sims 4 / Enshrouded
on mobile; pushing toward photoreal on desktop.

### 8.1 How "scalable" works in practice
- **Quality tiers** (auto-detected, user-overridable): Low / Medium / High / Ultra.
  Each tier scales shadow quality, post-processing, draw distance, texture
  resolution, real-time vs. baked lighting, effects, and active-NPC count.
- **Pipeline:** **Unity URP** as the base (mobile-capable, modern PBR). Evaluate a
  separate **HDRP desktop build** for the Ultra tier *only if* the desktop version
  justifies it — but don't maintain two pipelines early; URP-High is the first goal.
- **Performance budgets:** target ~60 fps desktop / 30–60 fps mobile per tier;
  per-tier caps on visible characters, lights, and draw calls.

### 8.2 Art techniques to get "realistic" affordably
- **PBR materials** (albedo/normal/roughness/metallic) everywhere; scan-based or
  high-quality texture libraries for environments.
- **Baked global illumination** (lightmaps + light probes) for mobile performance;
  real-time GI / screen-space effects reserved for High/Ultra tiers.
- **LODs, GPU instancing, occlusion culling, texture streaming, addressables** —
  non-negotiable for realism at scale, especially on mobile.
- **Atmosphere sells realism cheaply:** good skybox, fog, color grading, and a
  day/night + weather cycle do more for "believable" than raw polygon counts.

### 8.3 Realistic characters (the hard part)
- Realistic *custom* humans risk the uncanny valley and are the most expensive art.
- **Note:** the gold-standard realistic-character tool, **MetaHuman, is
  Unreal-only** — not available in our Unity stack. In Unity we'll need a custom
  realistic modular rig or a high-quality character asset solution, with
  blendshape-based body/face customization on a shared humanoid skeleton.
- Lean on quality **hair, skin shaders, cloth, and animation** — believable
  *movement* reads as "realistic" as much as the model does.

### 8.4 Production reality
- Realistic art is the **largest content cost** in the project. Plan for an art
  pipeline, asset budget (store packs / scans early, custom later), and a
  technical artist. Prototype phases (0–1) can use **placeholder/greybox art** —
  prove the game is fun before investing in realism.

---

## 9. Offline & idle play

**A) A co-op friend is offline (someone else hosts):**
- Their character runs on **AI autopilot** (goes home, sleeps, does basic job) so
  the world isn't emptied; their **home & assets persist and are protected**.
- Optional **capped passive income** from their job while away.

**B) Solo / true offline (no friends online):**
- Full **single-player mode** with AI NPC villagers.
- **Idle / "while you were away" progression:** queue a work order before logging
  off; a digest on return shows earnings & events.
- **Prep & manage (good offline content):** customize character/home, decorate,
  craft, plan builds, **manage finances & marketplace listings**, browse the
  market, leave async gifts/letters for friends.

**Rule:** offline keeps you progressing gently; **active co-op is always the
richer, faster path** (offline must never out-earn playing).

---

## 10. Technical architecture (Unity)

```
┌───────────────────────────────────────────────────────────────┐
│  Simulation / gameplay layer (C#)                              │
│  needs · skills · NPC utility-AI · relationships · world state │
│  — decoupled, testable systems; single-player & co-op share it │
└───────────────┬───────────────────────────────────────────────┘
   ┌────────────┴───────────┐   ┌──────────────────────────────┐
   │ First-person controller │   │ Networking                   │
   │ camera · input · IK     │   │ Netcode for GameObjects +    │
   │ interaction system      │   │ Unity Relay + Lobby (co-op)  │
   └────────────┬───────────┘   └──────────────────────────────┘
   ┌────────────┴───────────┐   ┌──────────────────────────────┐
   │ Modular avatar / anim   │   │ Backend services             │
   │ (UMA or custom rig)     │   │ accounts · wallet · market · │
   │ Mecanim humanoid        │   │ payments (Stripe/Tilia) · KYC│
   └─────────────────────────┘   └──────────────────────────────┘
        Targets: iOS/Android · Desktop · Web (stretch, §11)
```

- **Co-op:** host-authoritative; authority simulates world & NPCs, clients send
  input / receive synced state. Relay for NAT traversal, Lobby for join-by-code.
- **Economy is server-authoritative:** wallet, marketplace, and transactions live
  on the **backend** (never trust the client with money/items). Payments &
  cash-out handled by the provider, not in-game code.

---

## 11. Open questions & known constraints

**Open design questions**
- AI NPC count/density (scope lever); aging/children (keep as optional late
  feature?); cross-session world persistence when host offline (backend cost);
  title; realistic-character art solution in Unity (no MetaHuman — see §8.3).

**⚠️ Known constraints**
- **Web is hard** with Unity WebGL for 3D first-person MP (size, perf, WebSocket-only
  networking) → **mobile + desktop primary; web later/reduced**.
- **Multiplayer** is a top scope driver → single-player core first, co-op in Phase 6.
- **The economy makes you a marketplace + payments platform** → app-store IAP cut &
  steering rules, KYC/AML, tax, fraud, age-gating, jurisdiction limits. Use a
  provider; get legal review. This is its own track (Phase 7).
- **Character customization + animation** is a real subsystem — budget time.

---

## 12. Development roadmap

Single-player core proven *before* multiplayer; economy after gameplay is real.
Each phase ends runnable.

- **Phase 0 — Foundation.** Unity project, first-person controller, test
  environment, basic interaction.
- **Phase 1 — Embodied vertical slice ("is it fun?").** One character; needs
  (hunger/energy); eat, sleep, **work, and pay rent** — the ordinary-life day loop
  in a greybox apartment. **Decides the game.** *(Drafted — see `PHASE1.md`.)*
- **Phase 2 — Character creation & avatar.** Modular body/gender customization,
  animation, persistent character + Origin/starting status. *(Drafted: creator UI,
  appearance data model, save/load, greybox avatar — see `PHASE2.md`. Real rigged
  mesh + animation deferred to art work.)*
- **Phase 3 — Living world & NPCs.** AI villagers, resources, crafting, building,
  jobs/roles, day-night/weather. *(Drafted: utility-AI villagers + small town +
  shared facilities — see `PHASE3.md`. Resources/crafting/building deferred to a
  later pass; pathfinding/NavMesh deferred.)*
- **Phase 4 — Relationships.** Social graph, interactions, friendship/romance.
  *(Drafted: relationship values/tiers, proximity bonding, Talk, romance, HUD —
  see `PHASE4.md`. Dialogue trees & family lines deferred.)*
- **Phase 5 — In-game economy (no real money yet).** Coins, crafting-to-goods,
  player↔NPC selling, marketplace UI, reputation — fully playable with soft currency.
  *(Drafted: craft → sell → reputation loop, Workbench + Market — see `PHASE5.md`.
  Recipes/materials, NPC demand, P2P trade deferred.)*
- **Phase 6 — Co-op multiplayer.** Netcode + Relay + Lobby; sync characters, world,
  NPCs, and player↔player trading.
- **Phase 7 — Real-money layer.** Backend wallet, payments provider integration,
  KYC, buy-Coins (web), verified-seller cash-out, fraud controls, legal review.
- **Phase 8 — Realistic art & game feel.** Scalable-realism art pass (PBR, baked
  GI, LODs, quality tiers), character art/animation, audio, UI, mobile controls,
  tutorial, balancing.
- **Phase 9 — Ship.** Mobile builds, backend/hosting, store prep; web evaluated.

---

## 13. Risks
- **Scope:** 3D + first-person + co-op + customization + a payments platform is
  small-studio scale — phasing & single-player-first are the mitigations.
- **Regulatory/financial:** the real-money economy carries licensing, fraud, and
  tax obligations — isolate in Phase 7, use a provider, get counsel.
- **Pay-to-win drift:** keep purchased currency non-cashable and non-powerful;
  protect the co-op core.
- **Money-laundering vector:** mitigated by "cash out earned-only, never bought,"
  KYC, thresholds, holding periods.
- **Web feasibility / mobile performance / "empty world"** (see §11, NPCs).
- **Realistic-art cost & uncanny valley:** realism is the biggest content expense
  and realistic custom characters are hard in Unity (no MetaHuman); greybox first,
  invest in realism only after the game is proven fun (§8.4).
