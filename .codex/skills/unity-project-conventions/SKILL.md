---
name: unity-project-conventions
description: Apply the user's Unity C# architecture, lifecycle, serialization, UI, event, and asset-lifetime conventions. Use when implementing, modifying, refactoring, or reviewing a Unity project while preserving its existing architecture and Unity version constraints.
---

# Unity Project Conventions

Use this skill with `personal-code-style`. Read the project's Unity version, assembly layout, render pipeline, input solution, and existing conventions before editing.

## Workflow

1. Confirm the relevant Unity version and existing project pattern from repository files.
2. Identify the owner of state, rules, presentation, and object lifetime.
3. Extend the nearest established pattern with the smallest change.
4. Respect Unity lifecycle, scene, serialization, and domain-reload behavior.
5. Run the narrowest relevant EditMode/PlayMode tests or build checks.

## Defaults

- Prefer `[SerializeField] private` for Inspector references; do not expose mutable public fields for convenience.
- Separate authoring data from runtime state. Treat ScriptableObjects as assets, not automatically as per-session mutable state.
- Centralize initialization only where ordering is a real dependency. A common dependency order is configuration/data, runtime state, runtime objects, presentation, then game flow.
- Use managers for coordination and stable entry points, not as containers for all gameplay logic.
- Keep UI passive: render state and forward commands to the owning system.
- Pair event subscription with unsubscription at matching lifecycle boundaries.
- Treat Addressables handles, instantiated objects, pooled objects, coroutines, tasks, and native resources as explicit lifetime responsibilities.
- Keep Editor-only code in Editor assemblies/folders or behind appropriate compilation guards.
- Use assembly definitions and namespace boundaries already established by the project.

## Avoid blanket rules

Do not require every class to be `sealed`, every Boolean comparison to use `== true`, every property to use block syntax, every field to have a tooltip, or every missing reference to fall back to `GetComponent`. Apply these only when the local project or concrete risk justifies them.

Read [references/unity-conventions.md](references/unity-conventions.md) for detailed lifecycle and architecture checks when designing a new subsystem or reviewing a broad Unity change.

For the full original Korean guidelines (architecture layers, manager roles, naming system including Get/Find and Try/Request prefixes, human-readability and debugging rules, UI/event/data patterns with code examples), read [references/full-unity-guidelines.md](references/full-unity-guidelines.md). Treat it as the detailed source this skill summarizes; where the two disagree, this SKILL.md's relaxed stance wins.
