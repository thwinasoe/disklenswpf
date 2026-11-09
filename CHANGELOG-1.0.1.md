# DiskLens WPF 1.0.1

- Insights now includes category and largest-folder/root-file bar charts from the selected completed index, available without another scan.
- A single history measurement is displayed as a prominent labeled bar instead of a tiny dot. Aggregated day/week/month periods explain when only one period exists.
- The default Scan view plots every completed scan, including scans recorded on the same day. Day/week/month views still use only the last observation per period.
- Chart labels follow the current theme for better contrast.
- A native multi-resolution DiskLens icon is embedded in both executable releases and the WPF window. This supplies the title-bar/taskbar/Explorer icon; WPF has no browser favicon.
- Drives has distinct View last scan and Scan / Rescan actions. Viewing a snapshot does not create a scan or modify the scan index.

Existing LocalAppData snapshots are reused. No migration or rescan is required to display these charts. Versioned release folders let a running 1.0.0 scan finish without replacing its executable. After that scan finishes, close 1.0.0 and open release/v1.0.1/self-contained/DiskLens.exe (or the framework-dependent variant).

Verification covers live WPF scanning/binding, category/folder chart data, single-period aggregation, individual-scan aggregation, icon loading, opening an existing snapshot without scanning, and rendered dark/light Insights. The scanner and Electron source are unchanged.
