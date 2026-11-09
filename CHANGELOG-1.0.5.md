# DiskLens WPF 1.0.5

What changed now reads completed snapshots from saved history when refreshed instead of relying on the window's cached history. Comparison roots and volume IDs are normalized, timestamps are compared as actual instants, and the view identifies the previous/current scan dates. Selecting the earliest snapshot explains that a later snapshot must be selected rather than incorrectly claiming insufficient saved history.

Read-only verification confirmed that compatible completed snapshots could be compared. No rescan or snapshot modification was needed.

49 service/reliability assertions passed, including normalized root/volume matching and timestamp offsets. Actual WPF regression scanning two small fixture snapshots and deliberately replacing the in-memory history with only the latest snapshot still populated What changed. Rendered evidence: verification/v1.0.5/ui-comparison-reloaded.png. The exact cause in the user's older running instance cannot be proven from its screenshot; stale window history is now covered by a regression check.
