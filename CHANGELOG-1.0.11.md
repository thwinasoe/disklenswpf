# DiskLens WPF 1.0.11

Duplicate files replaces its mode dropdown and generic Check / Verify action with dedicated Quick check and Full check buttons using the existing Lucide Hash icon. Quick checks up to 5,000 candidates; Full checks all filtered candidates. Both commands disable while an operation is active; the existing Stop action remains available. The generic Check / Verify action remains for other investigation views.

The hashing algorithm, limits, candidate filters and cleanup safeguards are unchanged. Actual WPF UI regression executes each rendered button's bound command, verifies the selected mode/results, checks both commands are unavailable while checking and Stop is available, and renders the final layout. Build and UI checks passed. Saved scans and Electron remain unchanged.
