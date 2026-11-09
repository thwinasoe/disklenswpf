# DiskLens WPF 1.0.2

Investigate now has the eight Electron presets with a sidebar and compact selector: What changed, Recent large files, Old large files, Installer files, Archives and images, Developer clutter, Duplicate files and Storage health. Search and investigation rows are isolated. Switching presets clears selection and rejects stale query results.

Comparisons show current sizes, signed differences, all four change types, full-index Added/Growth/Freed summaries and up to 1,000 rows sorted by absolute difference. Missing files have current size zero and conservative explanations. Archive/image queries exclude installers; developer rules match Electron. Duplicate category filters are applied before hashing.

Window and executable icons are byte-for-byte copies of Electron build/icon.ico. PNG and HTML report favicons use build/icon.png. Navigation, investigation and file icons use the same Lucide 0.468.0 vector shapes; license included. The Electron development window's generic Electron runtime icon is not the branded application asset.

Validation: Release build, scanner/service self-tests including comparison summaries and all new query presets, live WPF fixture workflow, all vector path parsing, wide/compact rendering. Hardware hot-unplug and native deletion dialogs retain the prior manual validation limitations.
