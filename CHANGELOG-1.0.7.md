# DiskLens WPF 1.0.7

The title bar now displays the running application version to distinguish older windows. The --show-changes launch option opens Investigate directly. No comparison matching logic was changed in this release.

Read-only verification in the WPF investigation view confirmed that the comparison populated from existing saved snapshots. No scans or stored snapshots were changed.

Release build and actual WPF fixture/UI regression passed. A read-only --verify-live-comparison output-path option runs the real investigation view against saved history and produces diagnostics and a screenshot before closing its own window. The normal app launch stays open.
