# DiskLens WPF 1.0.10

Search clears old rows when a request starts and displays animated indeterminate progress until its current request completes. Enter submits the Search query. Common exact-extension wildcards such as *.docx and *.pdf use the existing extension index; general/Unicode patterns retain conservative candidate filtering and final .NET matching. The optimized extension list deliberately excludes special Unicode case-folding letters. No scan migration or rescan is required.

Read-only verification of a saved snapshot returned only .docx rows for the *.docx filter. The screenshot showed Querying disk index while previous unfiltered rows remained visible; the current service could not reproduce a wildcard mismatch. No blanket full-drive performance claim is made.

Double-clicking an actual file row in Search or Investigate dispatches the existing guarded Explorer reveal action, selecting that exact file in its containing folder. Headers/empty areas and non-file report rows do not open a stale selection. Routed WPF double-click tests verify exact path dispatch in both views through a test-only Explorer launch interception; the actual native Explorer UI is not automated by these tests.

Release build, 57 service/reliability checks and actual WPF search/loading/double-click workflows passed. Evidence: verification/v1.0.10. Electron and saved snapshots are unchanged.
