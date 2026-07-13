# Unity conventions reference

## Architecture

- Keep gameplay rules in plain C# objects or focused components when possible.
- Use MonoBehaviours for Unity lifecycle, scene integration, and component-facing behavior rather than as a requirement for every domain type.
- Let runtime models own session state. Do not mutate shared ScriptableObject assets as accidental save state.
- Avoid adding a singleton solely to make access convenient. Prefer explicit references, composition roots, or existing project services.

## Initialization

- Do not rely on incidental `Awake` or `Start` ordering between unrelated objects.
- Use an explicit composition root when several systems have a real initialization dependency.
- Validate scene and prefab references in edit-time tooling or targeted runtime guards according to project maturity.
- Account for disabled domain reload and scene reload settings when static state is involved.

## Serialization and assets

- Remember that Unity serialization differs from general .NET serialization.
- Avoid renaming serialized fields without a migration strategy such as `FormerlySerializedAs`.
- Release Addressables handles through the owner that acquired them.
- Avoid universal `Resources.Load` fallbacks; use the project's chosen asset pipeline.
- Distinguish prefab assets, scene instances, pooled instances, and instantiated Addressables.

## Events and asynchronous work

- Subscribe and unsubscribe symmetrically, commonly in `OnEnable`/`OnDisable` or explicit lifetime methods.
- Prevent callbacks from targeting destroyed objects.
- Define cancellation and ownership for tasks, coroutines, and scene transitions.
- Avoid global events when a local reference or scoped event channel expresses ownership more clearly.

## UI

- UI reads presentation state and sends intent; authoritative state changes occur in the owning system.
- Avoid repeated scene searches and uncontrolled `GetComponent` calls in hot paths.
- Keep navigation, input focus, resolution scaling, localization, and accessibility in scope when relevant.

## Verification

- Compile affected assemblies.
- Run focused EditMode tests for pure logic and PlayMode tests for lifecycle or scene behavior.
- Inspect the Console for serialization and missing-reference warnings.
- For asset or scene changes, verify load, unload, scene transition, and repeated-entry behavior.
