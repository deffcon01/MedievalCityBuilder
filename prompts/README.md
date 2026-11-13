# Prompt Library

Bounded, self-contained task prompts for the AI coder (OpenCode). Each file is one vertical slice with explicit scope, verification, and stop conditions — per the AGENTS.md workflow.

## How to use

### Manually (one task at a time)
1. Start OpenCode in the repo root.
2. Tell it: `Read prompts/00X-<name>.md and execute the task exactly. Follow AGENTS.md.`
3. Review its report, run the verification commands yourself if you want a second check, then move to the next file.

### Semi-automated (driver script)
Run `tools/run-prompts.ps1`. It executes the next pending prompt on a fresh `task/<prompt-name>` branch off main, runs `dotnet test` after, allows up to 3 fix attempts on failure, requires a clean tree, pushes the branch, and stops. Merge the PR, then run the script again for the next prompt. Do an attended test run before leaving it overnight.

Note: the `.done` markers are local-only (gitignored) — they track which prompts this machine has executed, not which PRs are merged. Always merge the PR before running the next prompt.

## Rules

- Execute prompts strictly in order — each builds on the previous one's output.
- One prompt = one branch = one PR. Merge before starting the next.
- Never run two prompts in parallel.
- If a prompt stops on a design question, answer it (or ask the user), then re-run the same prompt — do not skip ahead.
- New prompts: copy the template structure (Objective / Read first / Allowed changes / Requirements / Definition of done / Stop conditions / Commit), number it next in sequence, and keep it to one bounded slice.

## Current sequence

| # | File | TODO.md area |
|---|------|--------------|
| 001 | road-entrance-snapping | 1.3 leftover |
| 002 | zone-scene-brush | 1.4 Zone painting |
| 003 | zone-minimum-size | 1.4 |
| 004 | zone-road-distance | 1.4 |
| 005 | zone-terrain-validation | 1.4 |
| 006 | zone-persistence | 1.4 |
| 007 | development-demand-events | 1.5 |
| 008 | available-zone-search | 1.5 |
| 009 | parcel-fitting | 1.5 |
| 010 | building-asset-definitions | 1.5 |
| 011 | weighted-asset-selection | 1.5 |
| 012 | building-placement | 1.5 |
| 013 | citizen-household-models | 1.6 |
| 014 | housing-capacity-homelessness | 1.6 |
| 015 | employment-wealth-happiness | 1.6 |
| 016 | phase2-vertical-slice | Phase 2 |

Prompts 017+ (Phase 3+: services, needs, economy, procedural world) are added after the vertical slice is verified.
