# Phase 0 — Foundation

**Goal:** the plumbing — prove you can stand in a 3D space and move/look in first
person, with an interaction system to build on. No gameplay yet.

## What it provides
- **First-person controller** (`FirstPersonController`): WASD + mouse-look + sprint
  + jump on a CharacterController (legacy input; migrates to the new Input System
  for touch later).
- **Interaction system** (`Interactable` base + `PlayerInteractor`): raycast from
  the camera, show a prompt for the focused object, trigger it with `E`.
- **Pipeline-agnostic materials** (`MaterialUtils`): works in Built-in or URP.
- A runtime **greybox builder** (`Phase0Bootstrap`) — superseded by later phases'
  bootstrappers, kept for reference (its auto-run is disabled).

## Files
```
Player/FirstPersonController.cs
Interaction/Interactable.cs · PlayerInteractor.cs · GreyboxInteractable.cs
Common/MaterialUtils.cs
Bootstrap/Phase0Bootstrap.cs   (auto-run disabled; Phase3Bootstrap runs today)
```

## How to run
See **[README.md](README.md)** for the Unity setup. Today, pressing Play runs the
current town (Phase 3+) via `Phase3Bootstrap`; Phase 0's greybox is legacy. To use
the old greybox specifically, re-enable the `[RuntimeInitializeOnLoadMethod]`
attribute in `Phase0Bootstrap` and disable it on the later bootstrappers.

## Success criteria
You can walk around a 3D space in first person and press `E` to use an object.

## Next: Phase 1 — the ordinary-life day loop. See `PHASE1.md`.
