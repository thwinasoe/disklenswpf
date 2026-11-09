# DiskLens WPF 1.0.4 performance verification

Three complete scans per version, using the same 30,001 physical files / 311 folders / 300,007 logical bytes on this PC with the filesystem cache warm. Each scan includes filesystem enumeration, database writes, query-index construction, folder aggregation and snapshot publication. Search timings cover the Store.Search service and exclude WPF rendering. The first pass includes application JIT/startup effects. Scanner worker startup and native volume probing are included on every pass. Applications already running on the PC were left untouched; timing is not a controlled laboratory result.

| Operation (median of three passes) | WPF 1.0.3 | WPF 1.0.4 | Time reduction |
|---|---:|---:|---:|
| Complete scan + publication | 1442.51 ms | 1124.96 ms | 22.0% |
| First 500 files | 8.84 ms | 5.97 ms | 32.5% |
| Wildcard *report*.pdf | 75.56 ms | 22.34 ms | 70.4% |
| No-match contains search | 39.38 ms | 26.91 ms | 31.7% |

Peak parent working set across the measured scan/search passes: old 92,938,240 bytes (88.63 MiB), new 85,995,520 bytes (82.01 MiB). Worker memory is excluded, so this is not total application memory. Index bytes after a scan: old 15,953,920, new 15,577,088. Both produced identical file and byte totals, no issues, and matching search result counts.

Baseline was also measured before optimization; both baseline JSON runs are retained. Final comparisons use baseline-warm-recheck.json and final-benchmark.json. These fixture results must not be extrapolated directly to a full C: scan or a million physical files. The separate million-row test verifies index/query behavior only.

Changes: reusable bound SQLite statements; at most 128 metadata records / 1 MiB per worker frame; pooled read buffers; no serialized UI-derived fields; incremental direct-folder totals; secondary indexes built after traversal; precompiled exclusions/search filters; SQL LIKE candidate filtering with Unicode fallback and .NET verification; cancellation of superseded searches. Single-worker filesystem traversal and safe-mode pacing are retained. SQLite synchronization/durability settings are unchanged.

Reliability verification includes multi-batch Unicode/apostrophe paths, partial last batches, root/child timeout handling, retained data from received batches, folder aggregation, mid-scan cancellation with worker termination and preserved snapshots, oversized frames, wildcard/Unicode/literal-bracket semantics, exports, duplicate/hard-link handling, comparisons and million-row search. Live WPF startup/scan/render checks passed. Hardware disconnect and native Recycle Bin dialogue validation retain their earlier limitations.

Reproduce using an empty benchmark fixture directory and the bundled runtime or installed .NET 10 Desktop Runtime:

```powershell
.\DiskLens.exe --benchmark C:\Temp\DiskLens-benchmark-fixture C:\Temp\DiskLens-benchmark.json
```

The benchmark creates 30,001 files in its fixture directory and application-owned snapshot indexes beside its output; it does not delete them automatically. Keep user data outside that fixture.


Insights and investigation database queries now load when their tabs are opened. Leaving Investigate cancels its pending query; this avoids full-index comparison work on Map startup or after selecting a snapshot. This UI scheduling benefit is not included in the scanner/service timing table.
