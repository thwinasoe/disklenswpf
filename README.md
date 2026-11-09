# DiskLens WPF

Native Windows disk explorer built with C# / WPF and MVVM on .NET 10 LTS. The existing Electron project is a read-only reference and is not required at runtime. No AI, model runtime, Electron, WebView or third-party NuGet dependency is included.

## Run

- `release/framework-dependent/DiskLens.exe`: needs the **.NET 10 Windows Desktop Runtime x64**.
- `release/self-contained/DiskLens.exe`: includes the runtime; extract the whole folder before running.
- ZIP packages in `release` are portable distributions, not installers.

Choose a folder or a connected drive and click Scan. The previous completed snapshot stays available when a new scan fails or is cancelled. Folder scans are indeterminate; whole-drive estimates use recorded volume usage, capped at 99% during traversal. Finalization is explicitly identified.

The map supports folder drill-down and parent navigation. Map tiles include the largest 150 items and an explicit remainder tile. Search is paged at 500 rows and supports filename/path contains, `*`, `?`, semicolon alternatives and minimum/maximum bytes. Investigation includes recent files (30 days), old files (365 days), installers, explained cleanup candidates, scan changes, scan issues, file types, categories, document formats and category/folder growth. Select a historical snapshot to investigate offline data. History charts use actual measurements only, with the last observation in each day/week/month.

Duplicate checks use identical-size candidates and SHA-256, compare file metadata before/after hashing, and exclude aliases of the same physical file by volume and file ID. Use the search filter, size limits, installer-only checkbox and age-days filter to narrow duplicate checking. Results identify a suggested keeper and are capped at 500 displayed rows. Open folder selects the exact file. Cleanup is available for individual files and selected files (Ctrl/Shift selection); suggested keepers are protected. Every cleanup revalidates metadata and reparse-point ancestry, blocks Windows/Program Files/system files, and uses Windows Recycle Bin confirmation. Stop applies between native cleanup dialogs; cancel the current native dialog to abort it. No permanent-delete or automatic-cleanup operation is offered.

CSV, HTML and JSON exports support displayed reports and a streaming export of the complete index. CSV protects against spreadsheet formula interpretation; HTML escapes file names. Report exports explicitly cover displayed rows, not undisplayed pages. Whole-index exports include every indexed file.

Settings support exclusions, safe mode, history retention (1–365 snapshots per location), minimum duplicate size, light/dark theme and daily/weekly reminders while the app remains open. Reminders use the visible scan/Stop workflow. Forget deletes only DiskLens snapshots. Connected volumes are probed in isolated workers; their serial/label is remembered, and drive-letter reuse is excluded from comparisons when serials are available. Removable volumes automatically use safe mode; safe mode adds pacing to the single-worker traversal. Network and USB storage use the same portable traversal and timeout handling.

## Scanner reliability and storage

`Scanner.cs` controls a separate filesystem worker using streamed JSON records. A directory listing or metadata call that produces no record for ten seconds causes that worker to be terminated. The incomplete directory is recorded, a fresh worker is started, and other discovered directories continue. A failed or timed-out root rejects the scan. Reparse points are skipped; the scanner never follows junction/symlink loops. Cancellation kills the worker and interrupts SQLite work. The directory queue, file index, folder aggregates and duplicate hash table are disk-backed; UI result sets are bounded. SQLite is supplied by Windows (`winsqlite3.dll`), so no SQLite package is shipped. The scanner uses logical file sizes, not allocated/compressed/sparse sizes. Hard-link aliases count in scan totals, but not as duplicate reclaimable copies.

Each completed scan has an immutable SQLite file and JSON metadata in `%LOCALAPPDATA%/DiskLens.Wpf`. Publication writes metadata last; an orphan database from an interrupted publication is not loaded as a completed scan. Old snapshots are retained according to Settings (default 20 per location). Full indexes trade disk space for low RAM and persistent offline investigation; a million-row synthetic index was approximately 146 MiB. Retaining many large snapshots needs adequate local free space. Snapshots from the Electron app are not imported.

Windows storage health is read-only: physical-disk health/operational status and up to 50 disk/NTFS/controller events from the last 14 days. It has a 12-second command deadline. Missing permissions or unsupported providers can make results unavailable. Scan issues and Windows signals do not prove a drive healthy and no repair commands are run.

## Build and verify

```powershell
dotnet build DiskLens.Wpf.csproj -c Release
bin/Release/net10.0-windows/DiskLens.exe --self-test
bin/Release/net10.0-windows/DiskLens.exe --ui-test
dotnet publish DiskLens.Wpf.csproj -c Release -r win-x64 --self-contained false -p:DebugType=None -o release/framework-dependent
dotnet publish DiskLens.Wpf.csproj -c Release -r win-x64 --self-contained true -p:DebugType=None -o release/self-contained
```

Tests generate their own fixtures and reports. `--self-test` verifies scanning, exclusions, nested aggregates, persistence, wildcard search, installers, explanations, duplicates, hard links, comparisons, exports/escaping, cancellation, blocked-worker termination and million-row bounded queries. `--ui-test` runs the actual WPF window, scans a fixture, verifies binding/startup, and renders theme/size combinations. See `verification/` and `FEATURE-PARITY.md` for evidence and limits. No installed system files are recycled by tests.

## Architecture

- `MainVm.cs`: MVVM commands, selection, scan context, preferences and operation lifecycle.
- `Scanner.cs`: worker protocol, traversal, progress, exclusions and finalization.
- `Db.cs`: small Windows SQLite adapter with bounded page cache and cancellation interrupt.
- `Services.cs`: persistence, paged queries, investigations, comparisons, duplicates, safeguards and streaming exports.
- `NativeWorkers.cs`: isolated volume probes and read-only Windows storage signals.
- `MainWindow.xaml` / code-behind: responsive WPF views and drawing-based binary treemap.
- `HistoryChart.cs`: recorded-measurement history chart.
- `SelfTest.cs` / `UiTest.cs`: data/reliability and actual WPF smoke tests.

## Known limits

Portable filesystem traversal is used, not an NTFS MFT reader. An enormous flat directory still produces one IPC record per item. The watchdog bounds inactivity, not the total scan duration. A timed-out directory can be only partially indexed; issue count is shown with the snapshot. Removed-file comparisons explicitly include files newly inaccessible or excluded; absence is not proof of deletion. Comparisons show the largest 500 file changes, and folder growth covers direct children of the root. There is no scan-consistent filesystem snapshot: files can change while scanning. Native cleanup confirmations and actual USB unplug/replug, network outages and physical-health hardware behavior need manual testing on representative devices. Packages are unsigned; no installer or auto-updater is provided.

## Version 1.0.4 performance update

See [CHANGELOG-1.0.4.md](CHANGELOG-1.0.4.md) and [PERFORMANCE-1.0.4.md](PERFORMANCE-1.0.4.md). The standard launcher selects release/v1.0.4/self-contained; existing snapshots are compatible and running older instances are preserved.

Current release: 1.0.5 under release/v1.0.5; Start DiskLens.bat opens that release. What changed reloads completed saved snapshots and shows the compared dates. No rescan is needed when two compatible snapshots already exist. See CHANGELOG-1.0.5.md.

Current release: 1.0.6 in release/v1.0.6. Folder map colours distinguish sibling folders; area indicates size, file colours indicate category. Drive discovery explicitly publishes on the UI Dispatcher. See CHANGELOG-1.0.6.md.

Current release: 1.0.7 (release/v1.0.7). Title bar includes version. Launch with --show-changes to open Investigate directly. Read-only actual-view verification is available via --verify-live-comparison output-path. See CHANGELOG-1.0.7.md.

Current release: 1.0.8 (release/v1.0.8). Investigation shows animated indeterminate progress during index queries and hides it on completion or navigation. See CHANGELOG-1.0.8.md.

Current release: 1.0.9 (release/v1.0.9). Duplicate checks default to visibly labelled Quick (up to 5,000 candidates); choose Complete to check all filtered candidates. Both use full SHA-256. Filters apply before grouping. See CHANGELOG-1.0.9.md for measured performance and coverage limits.

Current release: 1.0.10 (release/v1.0.10). Search supports Enter, clears stale rows and displays loading progress. Common extension wildcards use the extension index. Double-click file rows in Search/Investigate to reveal their containing folder. See CHANGELOG-1.0.10.md.

Current release: 1.0.11 (release/v1.0.11). Duplicate files has dedicated Quick check and Full check buttons, disabled while checking; Stop remains available. See CHANGELOG-1.0.11.md.

Current release: 1.0.12 (release/v1.0.12). Map offers Explorer table, Ranked sizes and Space map, with persisted view and density preferences. Switching preserves folder and selection. Double-click or Open selected navigates folders/reveals files; space map single-click selects. All views share the indexed-item limit and remainder. See CHANGELOG-1.0.12.md.

Current release: 1.0.13 (release/v1.0.13). Theme-controlled button hover/pressed surfaces fix white text on pale hover backgrounds. Dark/light hover renders and label contrast checks passed. See CHANGELOG-1.0.13.md.

Current release: 1.0.14 (release/v1.0.14). Folder views remove full-width empty bar tracks and striping, align larger labels and values, and use subtle hover/selection. Compact density remains available. See CHANGELOG-1.0.14.md.
