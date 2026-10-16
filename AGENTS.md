# AGENTS.md — Unity-AI-Navigation

Unity package `com.ivanmurzak.unity.mcp.navigation` — wraps Unity's **AI Navigation** package
(`com.unity.ai.navigation`) for AI agents. On branch `experiment/rql` (local experiment) it ships no MCP tools: an
RQL module for Unity-MCP's `rql-query` / `rql-mutate` (surface `Bake()` / `Clear()`, agent `SetDestination`, agent-type
bake settings and areas via the `navigation` source, name views of agent types / areas / masks, and a guide rendered
into the `unity-rql` skill). An
extension of [AI Game Developer](https://github.com/IvanMurzak/Unity-MCP) (`com.ivanmurzak.unity.mcp`),
generated from `Unity-AI-Tools-Template`; the user-facing tool list is in `README.md`.

## Layout

- `Unity-Package/` — Unity project hosting the package; only `Packages/com.ivanmurzak.unity.mcp.navigation/`
  ships.
  - `package.json` — the version source of truth and the dependency pins.
  - `Editor/Scripts/Rql/` — the RQL module (`NavigationModule`), one adapter per file, the `navigation` source.
  - `Runtime/` — runtime assembly (currently empty).
  - `Tests/Editor/` (EditMode; `Rql/`: `NavigationTestBase` + one file per adapter / source) and `Tests/Runtime/` (PlayMode).
- `Unity-Tests/<unity-version>/` — one test project per supported Unity version, consuming the package via
  `file:`. A new version must also be added to `test_pull_request.yml` and `release.yml` (and
  `open-all-projects-*`).
- `Installer/` — Unity project that exports the `.unitypackage` installer
  (`Assets/com.IvanMurzak/<name> Installer/`).
- `commands/` — PowerShell helpers (below) and `check-versions.py` (synced in by the release cascade — do not
  hand-edit; CI runs it as the `version-consistency` gate).
- `.github/workflows/` — `test_pull_request.yml` (PR test matrix), `test_unity_plugin.yml` (reusable test
  job), `release.yml`, `bump_version.yml`.
- `docs/` — user docs: `Deploy-{OpenUPM,GitHub,npmjs}.md`, `Manual-Package-Rename.md`, `openupm-signing.md`,
  `img/`, `promo/`.

## Commands (PowerShell, from the repo root)

```powershell
./commands/bump-version.ps1 -NewVersion "x.y.z"   # package.json + Installer.cs in lock-step; -WhatIf to preview
./commands/get-version.ps1
./commands/update-ai-game-developer.ps1           # bump the com.ivanmurzak.unity.mcp pin; -WhatIf to preview
./commands/open-all-projects-windows.ps1          # open all Unity projects (or -unix.sh)
```

Tests run inside the Unity Editor (NUnit + `[UnityTest]`); CI runs them with `game-ci/unity-test-runner` in
EditMode / PlayMode / Standalone for every Unity version. `release.yml` releases on push to `main` only when
the `package.json` version has no tag yet (installer, signed UPM tarball for OpenUPM, test matrix, tag, GitHub
Release); dispatch `bump_version.yml` to bump in CI. Under `pull_request_target`, tests need the
maintainer-applied `ci-ok` label and abort if the PR touches `.github/workflows/`.

## Critical invariants

- **RQL surface.** Everything is registered by `NavigationModule` (`IRqlModule`, Order 100, found through TypeCache)
  and documented in its `AddGuide("Navigation", …)` text, which is rendered into the `unity-rql` skill.
- **Bake settings** are project-wide: `navigation | get agentTypes` (`NavMeshAgentType`) and `areas` (`NavMeshArea`)
  edit the `NavMeshProjectSettings` serialized object (`NavMeshNames.Settings()`), not a scene component.
- **Names over ids.** `agentType`, `areaName` / `defaultAreaName`, `areaNames` and `affectedAgents` are name views
  (`NavigationMembers`) over the numeric members; private backing fields that duplicate a public property are hidden
  (`NavigationMembers.HideBackingFields`), because writing them bypasses the property logic.
- Every C# file starts with the copyright box header and `#nullable enable`.
