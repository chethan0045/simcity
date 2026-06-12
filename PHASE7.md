# Phase 7 — Real-Money Layer (sandbox)

**Goal:** stand up the architecture for the real-money economy — buying Coins, and cashing
out Coins you earned by selling — with the anti-fraud "golden rule" enforced in code. This
delivers the *client-side spine* of `GAME_DESIGN.md` §6.3/§6.5.

**Honest scope — read this first.** Real money is a regulated, money-handling feature. Nothing
here moves real money:

> ⚠️ **This is a SANDBOX.** The only payments provider in the repo is a **mock** that charges
> nothing, pays out nothing, and collects no card data or PII. There is **no backend, no
> Stripe/PayPal/Tilia integration, no real KYC, no tax reporting**. Do **not** point this at a
> real provider or real funds. Going live is a separate backend track that **requires legal
> review** (`GAME_DESIGN.md` §6.5).

What *is* real and testable: the **dual-currency wallet**, the **compliance gates**, and the
buy → verify → cash-out **flow** through a provider interface.

---

## The golden rule (the whole point)

> **You can only cash out money you earned by selling — never money you bought.**

This is the primary anti-fraud / anti-money-laundering control. We model it by splitting the
wallet into buckets (`Economy/Wallet.cs`):

| Bucket | Source | Spendable? | Cashable? |
|---|---|---|---|
| **cashable** | marketplace **sales** | yes | **yes** |
| **purchased** | bought with real money (IAP) | yes | **no** |
| **granted** | Origin grant, wages, gifts | yes | **no** |

`Coins` (the HUD total) is the sum. Spending consumes **non-cashable buckets first**, so your
cashable balance is preserved as long as possible (and there's no spend-then-re-earn laundering
path). Only `MarketInteractable` adds cashable Coins — so to cash out, you must actually craft
and **sell**.

---

## Cash-out gates (`RealMoneyService.ValidateCashOut`)

Every one of these must pass, matching §6.3:
1. **KYC verified** — identity confirmed by the provider (we never see PII).
2. **Age-gated** — 18+ (real flow gets age from the provider's KYC).
3. **Payout threshold** — at least `PayoutPolicy.MinPayoutCoins`.
4. **Earned-only** — amount ≤ your **cashable** balance (the golden rule).
5. **Holding period** — earnings must settle `PayoutPolicy.HoldingPeriodDays` in-game days
   (chargeback/fraud window).

A **platform fee** (`PayoutPolicy.PlatformFeePercent`) comes off the gross payout — the core
revenue model. Buying Coins and cashing out would, in production, happen **on the web** (app-store
rules limit in-app currency sales); the in-app Bank stands in for that portal.

---

## Files added / changed
```
Economy/Wallet.cs                       split into cashable / purchased / granted buckets
                                        (Coins/Add/TrySpend/OnChanged unchanged → Phases 1-6 intact)
Economy/RealMoney/CoinPack.cs           purchasable Coin bundles (sandbox prices)
Economy/RealMoney/PayoutPolicy.cs       thresholds, conversion, fee, holding period, min age
Economy/RealMoney/IPaymentsProvider.cs  the seam to Stripe Connect / PayPal / Tilia
Economy/RealMoney/MockPaymentsProvider  SANDBOX provider — no real money, PII, or charges
Economy/RealMoney/RealMoneyService.cs   compliance engine: golden rule + KYC/age/threshold/holding
Interaction/BankInteractable.cs         the Bank station → opens the portal
UI/RealMoneyPortalScreen.cs             sandbox web-portal UI: balances, buy, verify, cash out
— MarketInteractable: sale proceeds now credit CASHABLE Coins
— TownWorld: adds a Bank station
— PlayerFactory: player gains a RealMoneyService (solo + co-op owner)
— PlayerHud: shows the cashable balance alongside total Coins
```

No new packages, no scripting define — it compiles and runs as a sandbox out of the box. (It is
*not* gated like Phase 6's netcode because it pulls in nothing external.)

---

## How to run
Setup per `README.md`. Press **Play**, spawn into town, then:
1. **Craft** at the Workbench, **Sell** at the Market — watch the HUD's **cashable** balance rise
   (wages and your starting Coins are *not* cashable; only sale revenue is).
2. Walk to the **Bank** → **E** to open the portal.
3. Try **Buy Coins** — purchased Coins are added but stay **non-cashable**.
4. Try **Request payout** before verifying / before the threshold / on purchased Coins — the gate
   message explains exactly why it's blocked.
5. **Verify identity**, sell past the threshold, wait out the holding period (lower
   `GameClock.secondsPerDay` or `PayoutPolicy.HoldingPeriodDays` to test fast), then cash out —
   watch the `[SANDBOX pay]` Console logs confirm a simulated payout.

---

## Before this could ever touch real money
- A **backend** that owns the authoritative wallet and talks to a real provider
  (**Stripe Connect / Tilia / PayPal**) — the client never holds funds or PII.
- Real **KYC/AML, age verification, tax reporting (1099-K), chargeback handling, payout rails** —
  all the provider's responsibility, not ours.
- **Web-based** buy/cash-out to respect Apple/Google IAP rules; in-app stays browse/spend.
- **Legal review.** This is regulated. Don't ship it without counsel.

---

## Success criteria (exit Phase 7 when…)
1. Sale earnings are cashable; purchased/granted Coins are not — visibly, in the wallet + HUD.
2. The cash-out gates (KYC, age, threshold, holding, earned-only) all block correctly and explain why.
3. The buy → verify → cash-out flow runs end-to-end through the provider interface (sandbox).

## Next: Phase 8 — Realistic art & game feel
Scalable-realism art pass (PBR, baked GI, LODs), character art/animation, audio, real UGUI/Canvas
UI, mobile controls, tutorial, balancing (`GAME_DESIGN.md` §12).
