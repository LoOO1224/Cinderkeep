# Detailed conventions

## Priority

1. Explicit user request
2. Repository and directory instructions
3. Established local style
4. These personal defaults

Do not rewrite existing code solely to enforce these defaults. Apply them to new or directly modified code and propose broader migration separately.

## Responsibility boundaries

- Coordinators and managers initialize systems, change high-level flow, and expose stable entry points.
- Domain systems implement combat, crafting, rewards, progression, spawning, and other game rules.
- Models own runtime state and protect state transitions through explicit operations.
- Data assets and configuration hold authoring data, identifiers, and tunable values without runtime orchestration.
- UI displays state and sends user intent to an owning system.
- Editor and debugging utilities remain outside shipping runtime paths where the engine supports that separation.

These are decision aids, not a mandate to create one class per bullet. Keep small features together when separation would add indirection without reducing risk.

## Debuggable code

- Prefer named intermediate values where a call chain hides which operation failed.
- Keep state-changing calls out of Boolean expressions.
- Use early returns for invalid prerequisites and expected rejection paths.
- Split a compound condition when its parts have domain meaning or need independent inspection.
- Keep a compact expression when it remains immediately readable; verbosity alone does not improve quality.

## Consistent semantics

- `TryX`: failure is expected and is represented without an exception.
- `FindX`: performs a search and may legitimately return no result.
- `GetX`: retrieves known state; absence is exceptional or explicitly represented by the type.
- `RequestX`: asks another owner to perform an operation.
- `IsX`, `HasX`, `CanX`: query state without causing side effects.

## Verification

Translate the request into observable checks. For a bug, reproduce the failure before fixing it when practical. For a refactor, establish equivalent behavior before and after. For new behavior, test success, important rejection paths, and engine lifecycle boundaries proportional to risk.
