### Cross-team collaboration

This project splits work across systems (economy, hustles, slots, apartment, art). To keep branches mergeable, treat shared surfaces as **contracts**, not playgrounds for big redesigns.

#### Computer dashboard cards are the bridge

The **computer dashboard** is where players open features. **Navigation / hustle cards** on that screen are the main integration point between subsystems: each card represents a feature (or entry into it) and should stay a thin, predictable surface.

- Prefer **adding** a card and **wiring** it to existing managers (`GameManager`, `HustleManager`, etc.) over **replacing** how the whole dashboard works.
- Keep card APIs stable: data the card needs (name, cost, lock state, callback on purchase) should change in **small, reviewable** steps.
- If two teams need the same screen, coordinate on **one** list of cards and ownership of that prefab or scene object — avoid parallel “dashboard v2” efforts.

#### Avoid large overhauls on shared UI

**Large overhauls** of the computer UI (full layout rewrites, renamed hierarchies, moving all hustle logic into a new architecture) create wide diffs that touch the same files everyone else edits. That **blocks merges** and forces painful rebases when multiple features land at once.

Instead:

- **Iterate locally** inside your feature (scripts, prefabs for your hustle, your scene extensions) and touch dashboard code only where you **must** to register your card or hook.
- **Refactors** of the global dashboard should be rare, time-boxed, and announced so teams can land in-flight work first or branch from a single agreed baseline.
- When in doubt, add a small extension point (e.g. one new card slot, one new event) rather than restructuring the entire nav.

#### Summary

| Do | Don’t |
|----|--------|
| Add or extend a card; hook feature logic behind it | Rip out and rebuild the whole computer UI in your branch |
| Reuse existing dashboard patterns and naming | Rename/move everything for “cleanup” mid-feature |
| Short PRs that touch shared prefabs minimally | Sweeping visual or code overhauls that merge-conflict with every other team |

The goal is **parallel delivery**: many small merges beat one big integration week.
