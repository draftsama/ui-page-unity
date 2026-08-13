---
status: testing
phase: 01-bugfixes
source: [01-VERIFICATION.md]
started: 2026-08-12T18:35:00Z
updated: 2026-08-12T18:35:00Z
---

## Current Test

number: 1
name: Transition-cancel/destroy/throw releases stuck m_IsTransitionPage flags
expected: |
  Cancel a mid-flight fade by deleting the target GameObject; confirm m_IsTransitionPage reads
  false on all surviving pages and the group transitions again immediately. Repeat with a
  throwing IPageShowEnd handler — exception propagates to the caller, group still transitionable
  afterward.
awaiting: user response

## Tests

### 1. Transition-cancel/destroy/throw releases stuck m_IsTransitionPage flags
expected: m_IsTransitionPage == false on all surviving pages after cancellation/destruction/throw; group accepts a new transition immediately
result: [pending]

### 2. Concurrent same-group transitions collide as immediate no-op
expected: Firing TransitionPageAsync(B) and TransitionPageAsync(C) back-to-back on the same group — exactly one runs, the other is an immediate no-op never replayed later; a different group transitions independently
result: [pending]

### 3. SetShow on destroyed CanvasGroup logs error, leaves m_IsOpened unchanged
expected: Removing CanvasGroup in inspector then calling SetShow(false) produces one clickable console error naming the page/group; m_IsOpened unchanged; no exception thrown
result: [pending]

### 4. SetShow recovers silently on merely-stale CanvasGroup reference
expected: Re-adding CanvasGroup after removal then calling SetShow(false) succeeds silently with no console output
result: [pending]

### 5. Transition aborts cleanly when a participating page loses its CanvasGroup
expected: Removing target page's CanvasGroup mid-transition logs an error (no NullReferenceException) and the group remains transitionable afterward
result: [pending]

### 6. Play-mode entry always starts with empty registry regardless of Reload Domain setting
expected: With Reload Domain disabled, opening page B then exiting and re-entering play mode shows default page A on the second run
result: [pending]

### 7. Exiting play mode mid-transition repairs state without touching authored/non-transitioning pages
expected: Stopping play mode mid-transition repairs the mid-transition page to a consistent state; a non-default page previewed (not mid-transition) survives untouched; scene/prefab is never marked dirty by the repair
result: [pending]

## Summary

total: 7
passed: 0
issues: 0
pending: 7
skipped: 0
blocked: 0

## Gaps
