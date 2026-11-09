# DiskLens WPF 1.0.9

Duplicate checks offer visible Quick (default, up to 5,000 candidates, smallest first then path) and Complete (all filtered candidates) modes. Quick can split a size group and is explicitly labelled limited whenever more candidates remain; it never claims full coverage. Both modes hash complete contents with SHA-256. Display remains capped at 500 rows; reclaimable totals cover verified groups within the selected candidate set.

Filters now precede size grouping. A conservative SQL path/size/age prefilter narrows rows; the existing .NET wildcard/category/installer matcher is authoritative. Eligible rows live in disk-backed temporary SQLite tables, avoiding a full candidate list in memory. Prepared bound statements, batched transactions, one ordered candidate stream and a reused worker read buffer reduce overhead. Worker communication remains sequential to avoid extra disk contention. Cancellation-aware 10-second inactivity deadlines kill blocked workers. Before/after size/date guards, live hard-link identity, keeper protection and Recycle Bin safeguards remain.

Electron reference difference: electron/duplicates.cjs filters in memory, groups by size and truncates candidates to 5,000 before sequential full SHA-256. WPF Complete covers all candidates that match the filters. Quick caps candidate selection at 5,000.

Measurement: identical cached fixture, 1,000 physical files of 65,536 bytes (62.5 MiB), three Complete checks each. Baseline times 1,146.23 / 715.93 / 805.99 ms; optimized times 1,282.87 / 727.25 / 716.63 ms. Medians 805.99 -> 727.25 ms (~9.8% lower). First-pass time regressed and timings vary; this small warm fixture is not proof of a full-drive speedup. Raw reports: verification/v1.0.9/duplicate-baseline.json and duplicate-optimized.json.

Verification: 57 service/reliability assertions; actual WPF Quick and Complete workflows/rendering. Added tests ensure filters eliminate singleton hashes, candidate limits report remaining coverage, changed files/hard-link aliases are rejected, cancellation works, and files differing after the first read buffer are not misclassified. Existing Electron source and saved snapshots remain intact.
