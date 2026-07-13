---
name: personal-code-style
description: Apply the user's shared code consistency, readability, and debugging conventions across game projects. Use when implementing, modifying, refactoring, or reviewing code in Unity, Unreal, or engine-independent game systems, especially when matching the user's preferred style matters.
---

# Personal Code Style

Apply these rules only to code within the requested change. Preserve stronger project-local conventions and existing style unless the user explicitly requests migration.

## Workflow

1. Inspect nearby code and project instructions before choosing a pattern.
2. State any assumption that materially affects architecture or behavior.
3. Make the smallest change that satisfies the request.
4. Keep responsibilities, ownership, and data flow visible in names and structure.
5. Verify the requested behavior and review the diff for unrelated changes.

## Core rules

- Give each class and function one clear responsibility.
- Keep managers and coordinators focused on lifecycle, routing, and orchestration. Put game rules in the owning domain or system.
- Keep UI responsible for presentation and input forwarding, not authoritative game state or rules.
- Prefer composition over inheritance unless a genuine polymorphic contract exists.
- Use guard clauses to keep nesting shallow.
- Split complex conditions into named Boolean values or meaningfully named functions.
- Extract a repeated condition or rule when repetition creates a real maintenance risk; do not abstract one-off code speculatively.
- Break long call chains into named intermediate values when it improves debugging.
- Avoid nested ternaries, hidden side effects in conditions, magic values, and multiple statements on one line.
- Choose semantic names consistently: `Try` for expected failure, `Find` when absence is normal, `Get` for retrieval, `Request` for cross-system commands, and `Is`/`Has`/`Can` for state queries.
- Make ownership and lifetime explicit. Match the engine's lifetime model rather than imposing generic patterns.
- Comment intent, responsibility, constraints, or non-obvious tradeoffs. Do not narrate obvious syntax.

## Review checklist

- Can a developer predict where to modify this behavior later?
- Can important intermediate values be inspected in a debugger?
- Does the change introduce a new global dependency, oversized manager, or UI-to-domain coupling?
- Does every changed line trace to the request?
- Were relevant tests, builds, or targeted checks run?

For expanded rationale and examples, read [references/conventions.md](references/conventions.md) only when resolving a style conflict or conducting a detailed review.
