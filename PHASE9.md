# Phase 9 — Ship (build pipeline + release checklist)

**Goal:** the production track — get the game onto devices and into stores. Most of "ship" is
**ops, accounts, and infrastructure** (store enrollment, signing certificates, backend hosting,
legal/compliance sign-off) that can't be authored as gameplay code. So this phase delivers the
**build/release tooling** that *can* live in the repo, and lays out the rest as a checklist.

This is the **last phase on the roadmap** (`GAME_DESIGN.md` §12).

> ⚠️ Reality check: **all of Phases 0–9 are written but have never been run in Unity.** Before
> any of the below matters, the project needs to open, compile, and play. The single highest-value
> next step is validation (see `README.md`), not shipping.

---

## What's in the repo now

### Headless build automation (`Assets/_Project/Editor/BuildScript.cs`)
Command-line entry points so builds are reproducible and CI-able:
```
Unity -batchmode -quit -projectPath . -executeMethod Simcity.EditorTools.BuildScript.BuildWindows
Unity -batchmode -quit -projectPath . -executeMethod Simcity.EditorTools.BuildScript.BuildAndroid
```
- Stamps the version from the `BUILD_VERSION` env var.
- Builds the scenes enabled in Build Settings, and **fails loudly** if none are (this project
  builds its world procedurally, so even an empty scene works — you just need one listed).
- Exits non-zero on failure, so CI catches it.

### CI workflow (`.github/workflows/unity-build.yml`)
A `game-ci/unity-builder` matrix building **Windows + Android** on tags (`v*`) or manually, with
`Library/` caching and artifact upload. It documents the prerequisites inline (see below).

### App identity (`Common/AppInfo.cs`)
One source of product name + version, shown in the **F10** settings footer and used by the build
stamp — so the version you see in-game is the version you shipped.

---

## Prerequisites to actually build (not yet in this repo)
The repo currently contains only `Assets/_Project` + `Packages` (the "copy into a fresh project"
layout from `README.md`). A real build/CI run also needs:
- [ ] The **full Unity project** committed: `ProjectSettings/` (incl. `ProjectVersion.txt`, which
      game-ci reads to pick the editor version), and `Packages/packages-lock.json`.
- [ ] **At least one scene** added to Build Settings.
- [ ] Repo **secrets** for the Unity license: `UNITY_LICENSE` (or `UNITY_SERIAL`), `UNITY_EMAIL`,
      `UNITY_PASSWORD`.
- [ ] Git **LFS** for any binary art once the Phase 8 asset work lands.

---

## Release checklist (the ops track — needs real accounts/infrastructure)

**Builds & platforms**
- [ ] Android: package name, keystore + signing config, target/min SDK, AAB for Play.
- [ ] iOS (if pursued): bundle id, provisioning/signing, App Store Connect.
- [ ] Desktop: Windows (and Mac/Linux if pursued) signed builds.
- [ ] Decide the **web** build's role (browsing/marketplace per the app-store currency rules, §6.5).

**Backend & live services**
- [ ] Host the **authoritative real-money wallet** + the payments-provider integration
      (Phase 7 is a client-side sandbox; the real backend is built here).
- [ ] Unity Gaming Services for **co-op** at scale: Relay/Lobby quotas, Authentication, project IDs.
- [ ] Player data persistence/backup, save migration, server-authoritative economy.
- [ ] Telemetry/analytics + crash reporting; remote config for live balancing.

**Store & compliance**
- [ ] Store listings, screenshots, age ratings (IARC), privacy policy + privacy manifests.
- [ ] **Payments/KYC/AML/tax** live via the provider; **legal review** of the real-money feature.
- [ ] Apple/Google **IAP** rules for currency purchases; keep buy/cash-out on the web.
- [ ] Accessibility, localization, ToS/EULA, data-deletion (GDPR/CCPA) flows.

**Quality gate**
- [ ] Per-tier performance profiled on real devices; memory/battery/thermals acceptable.
- [ ] Soak/stress test co-op; payment sandbox → provider test mode → limited live rollout.

---

## Success criteria (ship when…)
1. CI produces signed Windows + Android builds from a tagged commit.
2. The backend (wallet/payments/co-op) runs in the provider's test mode end-to-end.
3. Store listings, compliance, and legal sign-off are complete — then a staged rollout.

---

## Roadmap complete
Phases 0–9 are now drafted. The project spans: foundation → embodied life-sim loop → character
creation → living NPC town → relationships → in-game economy → co-op → (sandbox) real-money →
game-feel/quality → ship tooling. **Everything is code-complete on paper and unrun.** The next
real milestone isn't a Phase 10 — it's opening Unity, validating the stack, and iterating on what
actually breaks. See `README.md` for the validation path.
