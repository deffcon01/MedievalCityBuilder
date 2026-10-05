# Project Instructions for AI Coding Agents

## Mission

Build the medieval fantasy city builder described in GOAL.md and DESIGN.md.

The repository is in active prototyping.

Do not attempt to implement the entire game in one pass.

## Mandatory workflow

Before implementing a major system:

1. Read GOAL.md.
2. Read DESIGN.md.
3. Read ARCHITECTURE.md.
4. Inspect the existing project.
5. Identify the smallest useful vertical slice.
6. Create or update a task/plan.
7. Implement only the requested slice.
8. Build/compile.
9. Run relevant automated tests.
10. Run the Godot project or relevant headless test when possible.
11. Inspect errors and fix them.
12. Update documentation if behavior or architecture changed.
13. Report what was implemented, tested, and what remains.

## Do not

- Do not rewrite large sections of the project without justification.
- Do not invent finalized game-design values where DESIGN.md says testing is required.
- Do not create placeholder systems that silently become permanent architecture.
- Do not hard-code building asset lists into simulation logic.
- Do not couple core simulation logic to Godot scene nodes unnecessarily.
- Do not add multiplayer.
- Do not add combat unless explicitly requested.
- Do not build final-quality art before gameplay systems prove useful.
- Do not replace working systems merely for stylistic preference.

## Data-driven rule

Building assets, zoning definitions, resources, services, technologies and similar content should be data-driven.

New content should normally be addable without modifying core simulation algorithms.

## Testing rule

Every meaningful implementation should have a verification method.

For simulation systems, prefer automated tests.

For visual/gameplay systems, create small reproducible test scenes.

Use Godot headless execution where practical.

## Git rule

Make small, coherent commits.

Never use destructive Git operations unless explicitly instructed.

Do not push directly to main/master if a safer branch/PR workflow is available.

## AI-agent rule

If a task is ambiguous, inspect existing documentation/code first.

If a design decision materially changes the game, stop and ask the user rather than silently changing the design.

If a task is too large for one implementation pass, split it into smaller tasks.

## Overnight/autonomous work

When operating unattended:

- work only on explicitly approved tasks,
- do not change core design assumptions,
- do not install arbitrary dependencies,
- do not delete working systems,
- run tests/builds after each meaningful milestone,
- leave a clear progress report,
- stop when a task requires a design decision rather than guessing.

## Definition of done

A task is not complete merely because code was written.

It is complete when:

- code builds,
- relevant tests pass,
- runtime behavior is verified where applicable,
- no known errors were introduced,
- documentation is updated when necessary,
- the agent reports the exact verification performed.
