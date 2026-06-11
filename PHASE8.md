# Phase 8 — Realistic Art & Game Feel (code slice)

**Goal:** the polish pass — make the game *scale*, *feel*, and *onboard* well. Phase 8 in the
roadmap (`GAME_DESIGN.md` §12) is mostly an **art and asset** track (PBR materials, baked GI,
LODs, rigged characters + animation, audio, a real UGUI HUD). Almost none of that can be
authored without Unity open and actual art tooling — so this phase delivers the **code-shaped
half** now and is honest about what's deferred to in-editor work.

**Honest scope.** Built here: a scalable **quality-tier** system + settings menu, **mobile
touch controls**, camera **game feel** (head-bob + sprint FOV), and a **first-run tutorial**.
Deferred (needs Unity + assets — see the checklist at the end): the actual realistic art,
animation, audio clips, and the Canvas/UGUI reskin of all these greybox OnGUI panels.

---

## What's in this slice

### Scalable realism — quality tiers (`Common/QualityManager.cs`)
Four tiers — **Low / Medium / High / Ultra** — each mapping to Unity `QualitySettings`
(shadows, shadow resolution/distance/cascades, anti-aliasing, pixel lights, LOD bias) plus a
**target frame rate** (capped on mobile for battery/thermals; uncapped on Ultra desktop). The
choice is **persisted** and **applied at boot**, and the platform picks a sensible default
(Medium on mobile, High on desktop). Render-pipeline-agnostic; a URP render-scale pass can hang
off the same dial later.

### Settings menu (`UI/SettingsScreen.cs`) — press **F10**
Switch quality tier and mouse sensitivity live; both persist. Frees the cursor and freezes the
player while open. (Greybox OnGUI — the real options screen is part of the UGUI reskin.)

### Mobile touch controls (`Player/MobileControls.cs`)
Left-thumb virtual **joystick** to move (push far to sprint), right-side **drag to look**, and
on-screen **Jump / E** buttons. Auto-enables on touch platforms. It exposes the *same* intents
as the keyboard/mouse path, so `FirstPersonController` and `PlayerInteractor` consume it without
any platform branches of their own. Set `MobileControls.forceOn` to test it in the editor.

### Game feel (`Player/FirstPersonController.cs`)
Subtle **head-bob** while walking (faster/deeper when sprinting) and a small **FOV kick** when
sprinting that eases back when you stop. Input is now sourced through the desktop/mobile
abstraction, and sensitivity comes from the saved setting.

### First-run tutorial (`UI/TutorialScreen.cs`) — press **F1** to reopen
A welcome panel laying out the controls and the core loop (craft → sell → earn; eat/sleep/rent;
talk; bank). Shows once on first play (tracked in PlayerPrefs), dismiss with Enter.

---

## Files added / changed
```
Common/QualityManager.cs        quality tiers + target FPS + sensitivity; persisted, boot-applied
UI/SettingsScreen.cs            F10 settings overlay (quality + sensitivity, live)
UI/TutorialScreen.cs            first-run onboarding (F1 to reopen)
Player/MobileControls.cs        touch joystick / look / Jump / E (auto on mobile)
— FirstPersonController: desktop/mobile input abstraction + head-bob + sprint FOV
— PlayerInteractor: interact via the on-screen E button on touch
— PlayerFactory.BuildClientUx(): spawns settings + tutorial + touch controls (solo & co-op owner)
```
No new packages or scripting defines — it builds and runs as-is.

---

## How to run
Setup per `README.md`. Press **Play**:
- The **tutorial** appears on first run (Enter to dismiss; **F1** to reopen).
- Move and sprint — notice the **head-bob** and the **FOV** kick.
- Press **F10** — switch **quality tiers** (watch shadows/AA change) and **sensitivity** (live).
- To try **touch controls** in the editor, select the `MobileControls` object and tick
  **Force On** (or make an Android/iOS build): drag the left side to move, right side to look,
  tap **Jump / E**.

---

## Deferred to in-editor art work (the rest of Phase 8)
These genuinely need Unity + assets and aren't authorable blind:
- [ ] **Art pass:** PBR materials/textures, baked GI/lightmaps, post-processing, skybox, LOD groups.
- [ ] **Characters:** rigged humanoid mesh + blendshapes replacing the primitive avatar
      (the seam is already in `CharacterAppearance.Apply` — only the applier changes).
- [ ] **Animation:** locomotion/idle/interaction clips + an Animator (replaces transform nudges).
- [ ] **Audio:** footsteps, ambience, UI/interaction SFX, music (needs clips + an AudioManager).
- [ ] **UGUI reskin:** replace every OnGUI panel (HUD, creator, co-op menu, bank, settings,
      tutorial) with a proper Canvas/TextMeshPro UI, controller-navigable and mobile-safe-area aware.
- [ ] **Balancing & perf:** profile on a real device per tier; tune the economy/needs curves.

---

## Success criteria (exit Phase 8 when…)
1. The game runs acceptably across quality tiers and on a real mobile device with touch controls.
2. Movement *feels* good (bob/FOV), and new players understand the loop from the tutorial.
3. The art/animation/audio/UGUI checklist above is worked through in-editor.

## Next: Phase 9 — Ship
Mobile builds, backend/hosting, store prep (web evaluated) — the production track
(`GAME_DESIGN.md` §12).
