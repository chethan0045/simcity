# Phase 6 — Co-op Multiplayer

**Goal:** play the town *together*. A friend hosts, others join with a short code, and
everyone shares one world: you see each other walk around, the same villagers live their
lives for the whole group, the clock is in lockstep, and you can hand Coins to a friend.
This is the first delivery of `GAME_DESIGN.md` §9 (co-op) / §12 (architecture) and the
biggest complexity jump in the project so far.

**Honest scope.** The networking spine is real: Relay + Lobby join-by-code, owner-authoritative
player movement, server-driven NPCs, a synced clock, and a player-to-player coin transfer.
Deferred to later passes (and called out below): cross-client **relationship** sync, a fully
**shared economy/market state**, NPC *action* replication beyond movement, host-migration,
and reconnection. Relationships and each player's needs/wallet are simulated locally and only
the visible/transacted bits are networked — enough to prove co-op is fun and correct.

> ⚠️ Like every phase here, this was authored without a Unity install — and netcode is the
> least forgiving thing to write blind. Treat the first run as a debugging session (see
> "Known risks" below). The single-player game is unaffected until you opt in.

---

## Opt-in: co-op is OFF by default

The whole netcode layer is wrapped in `#if SIMCITY_NETCODE`. **Without** the define, the repo
compiles and plays single-player exactly like Phase 3 — no packages required. To turn co-op
**on**:

1. **Add the packages.** If you opened *this folder* as the project, `Packages/manifest.json`
   already lists them. If you used the README's "fresh project + copy `_Project`" path, add via
   *Window → Package Manager → + → Add package by name*:
   - `com.unity.netcode.gameobjects`
   - `com.unity.services.core`, `com.unity.services.authentication`
   - `com.unity.services.relay`, `com.unity.services.lobbies`
2. **Add the scripting define.** *Project Settings → Player → Scripting Define Symbols* → add
   `SIMCITY_NETCODE` → Apply. Unity recompiles and the co-op code lights up.
3. **Link a Unity project for Gaming Services.** *Project Settings → Services* → sign in and
   link/create a Unity project ID (Relay + Lobby are free at this scale). Without this, Host/Join
   fail at the "Allocating Relay…" step with an auth/credentials error.

With co-op off, the Co-op menu is skipped and you drop straight into the solo town.

---

## How it works

```
Character creator ─▶ Co-op menu ─┬─ HOST  → UGS sign-in → Relay alloc → Lobby (carries relay code)
                                 │          → StartHost → spawn NPCs + clock → share LOBBY code
                                 ├─ JOIN   → join Lobby by code → read relay code → StartClient
                                 └─ SOLO   → the offline single-player town (Phase 3)
```

- **Players** are NGO `PlayerPrefab`s, auto-spawned per connection. The machine that *owns* a
  player gets the full first-person rig (camera, input, needs, wallet, HUD); everyone else sees
  it as an avatar. Movement is **owner-authoritative** (`OwnerNetworkTransform`) so it feels
  responsive. Each player publishes its chosen **look** via a `NetworkVariable<NetAppearance>`.
- **Villagers** spawn on the **host** and stream out via a server-authoritative `NetworkTransform`;
  the host runs the brain. Only a tiny appearance **seed** is networked — each machine rebuilds
  the identical villager from it (`NetworkNpc`).
- **Clock** is host-authoritative (`NetworkClock`): clients stop advancing their own clock and
  mirror the host's day/hour, so time-skips (sleep/work) and the sun match for everyone.
- **Player-to-player Coins:** look at a friend and press **E** to *Give 10 Coins*. The giver
  deducts locally, the server credits the receiver on *their* machine over an RPC
  (`NetworkPlayer.TryGiveCoins`). This is the seed of the co-op marketplace.

Because there are no scenes or prefab assets in this project, the NetworkManager, transport, and
the player/NPC/clock "prefabs" are all built **in code** (`NetworkBootstrap`) with hand-assigned
`GlobalObjectIdHash`es so host and clients agree on object types.

---

## Files added / changed
```
Net/CoopService.cs            UGS init + anon auth + Relay alloc/join + Lobby (code) + start NGO
Net/NetworkBootstrap.cs       code-only NetworkManager + UTP + player/NPC/clock prefab registry
Net/NetworkPlayer.cs          per-player: owner rig vs remote avatar, synced look, coin gifting
Net/OwnerNetworkTransform.cs  owner-authoritative NetworkTransform (responsive player movement)
Net/NetworkNpc.cs             host-driven villager; seed-synced look; server-only brain
Net/NetworkClock.cs           host-authoritative day/time replicated to clients
Net/NetAppearance.cs          NetworkVariable-friendly AppearanceConfig snapshot
Net/PlayerGiftInteractable.cs "Give Coins" — the first player-to-player economic action
World/CoopWorld.cs            builds the shared town + ensures the network runtime
UI/CoopMenuScreen.cs          Host / Join-by-code / Solo front door (OnGUI)
Bootstrap/Phase6Bootstrap.cs  live entry: creator → co-op menu (on) or solo town (off)
— GameClock: `autoAdvance` flag + SetTime() so NetworkClock can drive time on clients
— TownWorld: extracted BuildSharedEnvironment() (reused by CoopWorld)
— PlayerFactory: OutfitNetworkedOwner()/OutfitNetworkedRemote() for networked avatars
— Phase3Bootstrap: auto-run disabled (Phase6Bootstrap supersedes; runs the same solo flow)
— Packages/manifest.json: netcode + UGS packages (optional; see opt-in above)
```

---

## How to run

**Single-player (default):** nothing changes — press Play, create a character, live in the town.

**Co-op:** complete the opt-in steps, then to test on **one machine**:
- Make a Windows build (*File → Build Settings → Build*). Run the **build** as the host and
  press Play in the **Editor** as the client (or run two builds).
- Host: pick **Host**, note the **share code** shown on screen.
- Client: type the code into **Friend's code** and press **Join**.
- You should see the other player's avatar moving, the same villagers on both, a matching clock,
  and Coins move when one of you presses **E** on the other (watch the `[Gift]` Console logs).

To play with a friend over the internet, both run a build, the host shares the code, done — Relay
handles the connection (no port-forwarding).

---

## Known risks (what to expect on first run)

- **Code-only NGO prefabs** are the sharpest edge: NetworkObjects normally get their
  `GlobalObjectIdHash` from an asset GUID. We assign it via reflection in `NetworkBootstrap`. If
  NGO rejects runtime prefabs in your version, the fallback is to make real prefab assets with
  `NetworkObject` + the same components and assign them in the inspector.
- **UGS not linked** → Host/Join throw at sign-in/Relay. Link a project in *Services*.
- **NetworkVariable-before-Spawn** (the NPC seed) assumes the value rides the initial spawn
  snapshot; if it doesn't, the `OnValueChanged` handlers correct the look a frame later.
- **API drift:** the Relay `RelayServerData` constructor and NGO RPC attributes shift between
  versions — if they don't resolve, check the installed package versions against the manifest.

Paste me the Console errors and I'll fix them — this phase fully expects a first-run pass.

---

## Success criteria (exit Phase 6 when…)
1. A friend can **Host**, share a code, and another can **Join** over the internet.
2. Players see each other move (with correct looks), share the same **villagers** and **clock**.
3. Coins transfer between players with **E**, and the solo path still works untouched.

## Next: Phase 7 — Real-money layer
Backend wallet, payments-provider integration, cash-out of *earned* Coins, KYC/compliance — its
own track with legal review (GAME_DESIGN.md §6.3 / §12). Validate co-op thoroughly first.
