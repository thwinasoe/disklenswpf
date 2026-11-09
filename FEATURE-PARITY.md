# Feature parity against DiskLens Electron

Reference inspected: README.md; scanner.cjs; main.cjs; index-store.cjs; duplicates.cjs; drives.cjs; health.cjs; insights.cjs; App.jsx; styles.css and treemap interface.

| Reference feature | WPF status | Evidence / scope |
|---|---|---|
| Native folder/drive scan, progress, Stop | Implemented | Worker traversal; data and live-WPF fixture tests |
| Timeout handling, finalization | Implemented with changed design | Killable workers, SQLite interrupt, explicit finalization; blocked-worker test |
| Bounded memory / indexed search | Implemented | SQLite disk-backed queue/index; 500-row pages; million-row synthetic test |
| Map and folder drill-down | Implemented | Drawing-based binary map; explicit omitted-item tile; UI renders |
| Contains/wildcard/path/size search | Implemented | Case-insensitive name/path, semicolon patterns, min/max, paged results |
| Investigation, deterministic cleanup reasons | Implemented | Dedicated eight-preset Investigate navigation, bounded archive/image/developer queries, deterministic reasons |
| External/USB/network and safe scans | Implemented; hardware validation outstanding | Isolated volume probe; removable automatic pacing; volume serial stored |
| Scan history / remembered offline drives | Implemented | Full per-location snapshots, selectable offline data, last-seen volume information |
| Insights day/week/month | Implemented | Last recorded observation per period, no fabricated measurements |
| File types / document formats / categories | Implemented | SQLite aggregate views and report exports |
| Category and folder growth | Implemented with scope limit | Adjacent same-volume/root snapshots; root child folder growth |
| Added/grown/shrunk/removed comparisons | Implemented with display limit | Indexed joins; largest 1,000 absolute differences with full-index summary totals; absent files labeled conservatively |
| Duplicates / hard links / filters / Stop | Implemented with display limit | SHA-256 worker, native identity, size/path/type/age filters; max 500 displayed rows |
| Installer detection | Implemented | Package extensions and setup-style executable name tokens; tests |
| Open folder / Recycle Bin / batch cleanup | Implemented; native deletion flow needs manual confirmation test | Exact Explorer selection, metadata and protected-path checks, each file confirmed; keeper protected |
| CSV / HTML / JSON | Implemented | Displayed report or complete streaming index; escaping and valid JSON tests |
| Light / dark / window sizing | Implemented | Actual WPF renders at 1200x800 and 640x480 |
| Exclusions / retention / minimum duplicate size | Implemented | Saved preferences; 1–365 full snapshots (default 20) |
| Recent locations / forget location | Implemented | Select history, forget only application indexes/metadata |
| Daily/weekly app-open reminders | Implemented | Visible scan workflow; disabled by default; schedule waiting period not elapsed in testing |
| Storage-health signals and scan issues | Implemented; hardware validation outstanding | Windows physical status/events bounded command, inaccessible/timeout paths persisted |
| Temperature/wear/error reliability counters | Implemented; hardware validation outstanding | Included when Windows supplies Get-StorageReliabilityCounter fields |
| Duplicate reclaimable-bytes summary | Implemented | Content-verified distinct-file groups and logical reclaimable totals calculated in SQLite |
| Full Electron visual layout/details panels | Partial visual parity | Palette, tabs, map and investigations preserved; matching Electron Lucide vectors and exact branded icon; native WPF layouts differ |
| Electron saved-index migration | Not implemented | Independent native data store; Electron remains untouched |
| AI/local models | Excluded as requested | No AI/model dependencies or features |

Verification limitations: million-row evidence is an index/query stress test, not a million-physical-file scan. Worker fault test simulates a blocked process. Device disconnection, permission changes, native Recycle Bin interaction, scheduled reminders and physical health must be exercised on representative hardware. Results with skipped paths are incomplete measurements.


Version 1.0.2: Investigate is a separate tab. Recent large files uses >=1 MB and last 30 days; old large files uses >=1 MB and older than 180 days. File investigation lists show 500 rows; Search provides paging. Duplicate results remain capped at 500, comparisons at 1,000. Electron saved-index migration and exact pixel-level layout parity remain unfinished.

Version 1.0.4: prepared bound statements and <=128-entry/1-MiB binary worker frames reduce scan overhead; compiled query matchers and SQL candidate filters preserve Unicode/wildcard semantics. Superseded searches cancel. See PERFORMANCE-1.0.4.md for physical-fixture measurements; no full-drive speed claim.

Version 1.0.5: What changed reloads saved history, normalizes location/volume identity and compares timestamp instants. Verified against existing saved snapshots, plus actual WPF stale-history regression. See CHANGELOG-1.0.5.md.

Version 1.0.6: deterministic folder map palette, category-based file colours and explanatory legend; drive discovery publication marshalled to UI Dispatcher. Background-thread regression and dark/light map renders passed.

Version 1.0.7: actual WPF investigation verified against saved snapshot indexes; visible version and direct comparison launch help distinguish old windows.

Version 1.0.8: animated investigation progress bound to current request lifecycle; pending/completed/navigation states verified in actual WPF.

Version 1.0.9: Quick/Complete duplicate checks, pre-group filters, prepared/batched writes, bounded disk-backed candidates, full SHA-256 and changed-file/hard-link safeguards. 57 assertions plus actual WPF mode workflows passed; small fixture timing evidence and limits in CHANGELOG-1.0.9.md.

Version 1.0.10: actual saved *.docx search verified (477 matches), indexed common-extension patterns, search loading state, Enter submission and row double-click Explorer dispatch. General wildcard and Unicode regressions passed.

Version 1.0.11: dedicated Quick/Full duplicate commands replace the dropdown; bound-button mode/results and running/Stop command states verified in actual WPF.
