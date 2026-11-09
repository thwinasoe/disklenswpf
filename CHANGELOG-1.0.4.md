# DiskLens WPF 1.0.4

Improves scanning with reusable parameterized SQLite statements, compact bounded binary worker batches and pooled buffers. Folder totals are accumulated during traversal; secondary database indexes are built after scanning. A single killable worker is retained, with safe-mode pacing and cancellation-aware read deadlines. Completed snapshots retain the existing schema and durability behavior.

Search now compiles patterns once, narrows candidate rows in SQLite while preserving Unicode and wildcard semantics with .NET verification, and cancels superseded file queries. Cleanup explanations are generated after result paging except when needed to filter cleanup candidates.

Build, service/reliability self-tests, million-row query checks and actual WPF startup/scan/render checks passed. See PERFORMANCE-1.0.4.md for measured complete-scan/search timings and their limits. No new third-party runtime dependencies, AI or models. Electron is unchanged.

Insights and investigation database queries now load when their tabs are opened. Leaving Investigate cancels its pending query; this avoids full-index comparison work on Map startup or after selecting a snapshot. This UI scheduling benefit is not included in the scanner/service timing table.
