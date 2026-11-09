# DiskLens WPF 1.0.17

Saved scan history now includes the regular DiskLens app-data folder and the Codex package's redirected DiskLens app-data folder. Scans created from Codex can otherwise be invisible when DiskLens is started normally. History deduplicates overlapping snapshot IDs and comparison opens each index in its existing location. No snapshot migration or new scan is required.

Regression tests reproduce the actual storage split: a latest scan in normal app-data and its baseline in redirected app-data. They verify discovery, index lookup, comparison results, duplicate handling and explicit removal without resurrection from a second metadata copy.
