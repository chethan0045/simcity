# Phase 2 — Character Creation & Persistent Avatar

**Goal:** let the player *create a character* (name, gender, body, colors) and have
that character **persist** across sessions, then spawn into the Phase 1 life-loop as
that character. This is the "it's *my* person" hook.

**Honest scope:** this phase builds the **system**, not the art. The avatar is a
greybox (capsules + spheres) whose proportions and colors change live. A real
rigged humanoid mesh with blendshapes is **Phase 8 / asset work** — but the data
model, persistence, creator UI, and apply-to-avatar pipeline are all real and
testable now. `CharacterAppearance.Apply()` is the seam where the real mesh drops in
later without touching anything else.

---

## What you'll see
1. **First Play → Character Creator.** A turntable avatar on a pedestal with a panel
   on the left: name, gender, height & build sliders, skin/hair swatches, shirt/pants
   RGB sliders, **Randomize**, and **Start Life ▶**. The preview updates live.
2. **Start Life →** the choice is saved to disk and you spawn into the apartment as
   your character (the Phase 1 loop: work / eat / sleep / rent).
3. **Next Play →** it skips the creator and loads your saved character.
4. **F9 in game →** deletes the saved profile; re-enter Play to see the creator again.

---

## Files added this phase
```
Stats/AppearanceConfig.cs        serializable look (gender, height, build, colors) + Randomize
Stats/CharacterAppearance.cs     greybox avatar built from primitives; Apply(config) = the seam
Core/SaveSystem.cs               JSON profile persistence (+ CharacterProfile)
UI/CharacterCreationScreen.cs    OnGUI creator with live preview turntable
World/ApartmentWorld.cs          shared world builder (refactored out of Phase 1), applies appearance
World/DevTools.cs                F9 = delete profile (dev only)
Bootstrap/Phase2Bootstrap.cs     orchestrates creator → save → game (auto-runs)
```
`Phase1Bootstrap` now delegates to `ApartmentWorld` and its auto-run is disabled, so
only `Phase2Bootstrap` runs.

## Architecture notes
- **AppearanceConfig** is plain serializable data — the persisted "what the character
  is," independent of how it's drawn. This is what later travels into co-op sessions.
- **CharacterProfile** (in SaveSystem) wraps appearance today; wallet/needs/progress
  will join it so a full character persists.
- **Persistence** uses `JsonUtility` → `Application.persistentDataPath/profile.json`.
  On Windows that's typically `%userprofile%/AppData/LocalLow/<Company>/<Product>/`.

---

## How to run
Same setup as before (`README.md`). Press **Play** → the creator appears on first run.
Customize, hit **Start Life ▶**, and you're in the apartment as your character. Look
down to see your body; stop/replay (or F9 then replay) to revisit the creator.

> Note: the in-game body hides its head/hair so the first-person camera (inside the
> head) doesn't clip them — that's expected. The full body shows in the creator.

---

## Success criteria (exit Phase 2 when…)
1. You can create a character, the preview reflects your choices live.
2. The character is saved and reloads on the next session.
3. You spawn into the loop visibly as *that* character.

## Next: Phase 3 — Living world & NPCs
Populate the world with AI villagers (utility-AI), resources, and jobs so it feels
alive — and so there's someone to build relationships with in Phase 4. See
`GAME_DESIGN.md` §12.
