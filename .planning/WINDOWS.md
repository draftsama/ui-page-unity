---
schema_version: 1
open_count: 2
waived_count: 0
fixed_count: 0
total_count: 2
last_updated: 2026-08-12T10:27:56.575Z
---

# Broken Windows Ledger

> Cross-phase defect register. `/gsd-ship` blocks while `open_count > 0`.
> Waive with `gsd-tools windows waive <id> "<reason>"` (reason required).
> Mark fixed with `gsd-tools windows fixed <id>`.

| id | phase | kind | file | line | description | status | reason | recorded_at | resolved_at |
|----|-------|------|------|------|-------------|--------|--------|-------------|-------------|
| 1 | 01 | unrun-verify | Runtime/UIPage.cs |  | Task 1 human-check: cancel a mid-flight fade transition in Unity play mode and confirm m_IsTransitionPage releases and the group is transitionable again (requires Unity Editor, unavailable in this execution environment) | open |  | 2026-08-12T10:27:56.253Z |  |
| 2 | 01 | unrun-verify | Runtime/UIPage.cs |  | Task 2 human-check: fire two overlapping TransitionPageAsync calls on the same group in Unity play mode and confirm exactly one runs with no later replay (requires Unity Editor, unavailable in this execution environment) | open |  | 2026-08-12T10:27:56.575Z |  |

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
  }
]
````
