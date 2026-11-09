# DiskLens WPF 1.0.16

History reloads completed snapshots from disk when opened. What changed refreshes the same history list before choosing its baseline, while preserving the selected scan through DataGrid selection updates. Unreadable or missing snapshot metadata/indexes produce an explicit warning instead of silently disappearing. Non-snapshot JSON reports are excluded from the metadata inventory.

Regression coverage restores deliberately incomplete in-memory history from saved snapshots in both Investigate and Insights / History and verifies that the current selection is preserved. Metadata recovery checks verify that invalid metadata does not hide valid snapshots and that warnings clear after recovery.
