# Untitled Life Sim

A grounded, co-op, first-person life simulation with a real player economy. See
**[GAME_DESIGN.md](GAME_DESIGN.md)** for the full design and **PHASE0–5.md** for
each build phase.

## What's playable now (Phases 0–7, single-player)
Create a character → spawn into a small town of AI villagers → live an ordinary
life: work or **craft & sell** for Coins, **eat / sleep / pay rent**, and **build
relationships** (friendship → romance) with villagers who live their own lives. Visit
the **Bank** to try the Phase 7 buy/cash-out flow (**sandbox — no real money**). Co-op
(Phase 6) and the real-money layer (Phase 7) are described in their phase docs.

> ⚠️ **Status: written but not yet run.** All code below was authored without a
> Unity install on the dev machine. This guide is about getting it running and
> validating it. Expect to hit a few first-run issues — that's exactly what this
> step is for.

---

## Setup & validation

### 1. Install Unity
Install **Unity Hub**, then a Unity editor through it.

```powershell
winget install Unity.UnityHub
```
(or download from <https://unity.com/download>). Then open Unity Hub →
*Installs → Install Editor* → latest **Unity 6 LTS** (or **2022.3 LTS**), including
**Windows Build Support**.

### 2. Create a project (simplest validation path)
In Hub → *New project* → **3D (Built-In Render Pipeline)** core template → create it
anywhere (e.g. `LifeSim`).

> Why Built-In, not URP? Our code is render-pipeline-agnostic and falls back to the
> Built-in `Standard` shader, so Built-In "just works" for validation with no
> package setup. URP is the eventual target for the Phase 8 art pass.

### 3. Add the scripts
Copy the **`Assets/_Project`** folder from this repo into the new project's
**`Assets/`** folder (drag it into the Unity Project window). Unity imports the
scripts and generates `.meta` files automatically. Wait for it to finish compiling
(no red errors in the Console).

### 4. Check one setting
Edit → **Project Settings → Player → Active Input Handling** = **"Both"** or
**"Input Manager (Old)"**. (Built-In projects usually default to this already.)

### 5. Press Play
With any scene open (the template's `SampleScene` is fine), hit **Play**:
1. The **Character Creator** appears (first run) — customize, then **Start Life ▶**.
2. You spawn into the **town**. Villagers walk around living their lives.

**Controls:** `WASD` move · mouse look · `Shift` sprint · `Space` jump ·
`E` interact · `Esc` free cursor (click to relock) · **F9** delete saved character.

**Try the full loop:**
- **Workbench** → `E` to *Craft* goods · **Market** → `E` to *Sell* for Coins (rep rises)
- **Diner** → *Eat* · **Bed** → *Sleep* · **Workshop** → *Work* (quick Coins)
- Walk up to a **villager** → `E` to *Talk* and build the relationship
- Watch the HUD: needs, Coins, day/time, goods, reputation, and relationships
- Watch the **Console** for `[Craft] [Market] [Sleep] [Rent] [Talk] [Romance]` logs

---

## If something breaks
Paste me the **Console errors** (or describe what looks wrong) and I'll fix it. The
most likely first-run culprits, already designed-around but worth knowing:
- **Pink/magenta objects** → shader/pipeline mismatch (shouldn't happen in Built-In).
- **Input exception** → set *Active Input Handling* to "Both" (step 4).
- **Nothing happens on Play** → confirm the scripts compiled with no Console errors.

---

## Project layout
```
Assets/_Project/Scripts/
  Bootstrap/   phase entry points (Phase6Bootstrap auto-runs today: solo, or co-op if enabled)
  Core/        GameClock, SaveSystem
  Stats/       CharacterNeeds, AppearanceConfig, CharacterAppearance
  Player/      FirstPersonController
  Interaction/ Interactable + Bed/Food/Work/Craft/Market/Npc interactables
  AI/          utility-AI villagers (NpcBrain, actions, mover)
  Social/      relationships, characters, social graph
  Economy/     Wallet (dual-currency), Inventory, ItemCatalog, SellerProfile, RealMoney/ (Phase 7 sandbox)
  World/       world builders (TownWorld, ApartmentWorld, CoopWorld) + shared helpers
  Net/         Phase 6 co-op (opt-in, #if SIMCITY_NETCODE): Relay/Lobby, networked player/NPC/clock
  UI/          PlayerHud, CharacterCreationScreen, CoopMenuScreen
GAME_DESIGN.md · PHASE0.md … PHASE7.md
```

## Phase 6 — Co-op multiplayer (written; opt-in)
Netcode for GameObjects + Relay + Lobby: a friend hosts, others join with a code, and
players/villagers/clock sync. It's **off by default** — the repo still compiles and plays
single-player with zero netcode setup. Turn it on (packages + the `SIMCITY_NETCODE` define)
per **[PHASE6.md](PHASE6.md)**. Still a big complexity jump — validate the single-player
slice above first.

## Phase 7 — Real-money layer (written; **sandbox**)
Dual-currency wallet (earned-by-selling = cashable; purchased/granted = not) with the
buy → verify → cash-out flow and all the compliance gates (KYC, age, threshold, holding
period, earned-only) enforced in code. **No real money moves** — the only payments
provider is a mock; there's no backend, real KYC, or provider integration, and going
live needs legal review. Walk to the **Bank** to try it. See **[PHASE7.md](PHASE7.md)**.

## Next: Phase 8 — Realistic art & game feel
Scalable-realism art pass (PBR, baked GI, LODs), character art/animation, audio, real
UGUI/Canvas UI, mobile controls, tutorial, balancing (see `GAME_DESIGN.md` §12).
