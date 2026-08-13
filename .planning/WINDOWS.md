---
schema_version: 1
open_count: 7
waived_count: 0
fixed_count: 0
total_count: 7
last_updated: 2026-08-12T18:30:00.000Z
---

# Broken Windows Ledger

> Cross-phase defect register. `/gsd-ship` blocks while `open_count > 0`.
> Waive with `gsd-tools windows waive <id> "<reason>"` (reason required).
> Mark fixed with `gsd-tools windows fixed <id>`.

| id | phase | kind | file | line | description | status | reason | recorded_at | resolved_at |
|----|-------|------|------|------|-------------|--------|--------|-------------|-------------|
| 1 | 01 | unrun-verify | Runtime/UIPage.cs |  | Task 1 human-check: cancel a mid-flight fade transition in Unity play mode and confirm m_IsTransitionPage releases and the group is transitionable again (requires Unity Editor, unavailable in this execution environment) | open |  | 2026-08-12T10:27:56.253Z |  |
| 2 | 01 | unrun-verify | Runtime/UIPage.cs |  | Task 2 human-check: fire two overlapping TransitionPageAsync calls on the same group in Unity play mode and confirm exactly one runs with no later replay (requires Unity Editor, unavailable in this execution environment) | open |  | 2026-08-12T10:27:56.575Z |  |
| 3 | 01 | unrun-verify | Runtime/UIPage.cs |  | 01-02 D1 human-check: remove CanvasGroup in inspector, call SetShow(false), confirm one clickable console error and m_IsOpened unchanged; re-add CanvasGroup, confirm silent recovery (requires Unity Editor, unavailable in this execution environment) | open |  | 2026-08-12T18:30:00.000Z |  |
| 4 | 01 | unrun-verify | Runtime/UIPage.cs |  | 01-02 D2 human-check: re-add CanvasGroup after removal, repeat SetShow(false), expect silent success with no console output (requires Unity Editor, unavailable in this execution environment) | open |  | 2026-08-12T18:30:00.000Z |  |
| 5 | 01 | unrun-verify | Runtime/UIPage.cs |  | 01-02 D3 human-check: remove target page's CanvasGroup mid-transition, confirm logged error and group remains transitionable afterward (requires Unity Editor, unavailable in this execution environment) | open |  | 2026-08-12T18:30:00.000Z |  |
| 6 | 01 | unrun-verify | Runtime/UIPage.cs |  | 01-02 D4 human-check: disable Reload Domain, open page B, exit, re-enter play mode, confirm default page A shows on the second run (requires Unity Editor, unavailable in this execution environment) | open |  | 2026-08-12T18:30:00.000Z |  |
| 7 | 01 | unrun-verify | Runtime/UIPage.cs |  | 01-02 D6 human-check: stop play mode mid-transition and confirm repair; preview a non-default page and confirm it survives a play session with no transition; confirm scene not marked dirty (requires Unity Editor, unavailable in this execution environment) | open |  | 2026-08-12T18:30:00.000Z |  |

````json
[
  {
    "id": 1,
    "kind": "unrun-verify",
    "phase": "01",
    "file": "Runtime/UIPage.cs",
    "line": null,
    "description": "Task 1 human-check: cancel a mid-flight fade transition in Unity play mode and confirm m_IsTransitionPage releases and the group is transitionable again (requires Unity Editor, unavailable in this execution environment)",
    "status": "open",
    "reason": "",
    "recorded_at": "2026-08-12T10:27:56.253Z",
    "resolved_at": null
  },
  {
    "id": 2,
    "kind": "unrun-verify",
    "phase": "01",
    "file": "Runtime/UIPage.cs",
    "line": null,
    "description": "Task 2 human-check: fire two overlapping TransitionPageAsync calls on the same group in Unity play mode and confirm exactly one runs with no later replay (requires Unity Editor, unavailable in this execution environment)",
    "status": "open",
    "reason": "",
    "recorded_at": "2026-08-12T10:27:56.575Z",
    "resolved_at": null
  },
  {
    "id": 3,
    "kind": "unrun-verify",
    "phase": "01",
    "file": "Runtime/UIPage.cs",
    "line": null,
    "description": "01-02 D1 human-check: remove CanvasGroup in inspector, call SetShow(false), confirm one clickable console error and m_IsOpened unchanged; re-add CanvasGroup, confirm silent recovery (requires Unity Editor, unavailable in this execution environment)",
    "status": "open",
    "reason": "",
    "recorded_at": "2026-08-12T18:30:00.000Z",
    "resolved_at": null
  },
  {
    "id": 4,
    "kind": "unrun-verify",
    "phase": "01",
    "file": "Runtime/UIPage.cs",
    "line": null,
    "description": "01-02 D2 human-check: re-add CanvasGroup after removal, repeat SetShow(false), expect silent success with no console output (requires Unity Editor, unavailable in this execution environment)",
    "status": "open",
    "reason": "",
    "recorded_at": "2026-08-12T18:30:00.000Z",
    "resolved_at": null
  },
  {
    "id": 5,
    "kind": "unrun-verify",
    "phase": "01",
    "file": "Runtime/UIPage.cs",
    "line": null,
    "description": "01-02 D3 human-check: remove target page's CanvasGroup mid-transition, confirm logged error and group remains transitionable afterward (requires Unity Editor, unavailable in this execution environment)",
    "status": "open",
    "reason": "",
    "recorded_at": "2026-08-12T18:30:00.000Z",
    "resolved_at": null
  },
  {
    "id": 6,
    "kind": "unrun-verify",
    "phase": "01",
    "file": "Runtime/UIPage.cs",
    "line": null,
    "description": "01-02 D4 human-check: disable Reload Domain, open page B, exit, re-enter play mode, confirm default page A shows on the second run (requires Unity Editor, unavailable in this execution environment)",
    "status": "open",
    "reason": "",
    "recorded_at": "2026-08-12T18:30:00.000Z",
    "resolved_at": null
  },
  {
    "id": 7,
    "kind": "unrun-verify",
    "phase": "01",
    "file": "Runtime/UIPage.cs",
    "line": null,
    "description": "01-02 D6 human-check: stop play mode mid-transition and confirm repair; preview a non-default page and confirm it survives a play session with no transition; confirm scene not marked dirty (requires Unity Editor, unavailable in this execution environment)",
    "status": "open",
    "reason": "",
    "recorded_at": "2026-08-12T18:30:00.000Z",
    "resolved_at": null
  }
]
````
