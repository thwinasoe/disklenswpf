# DiskLens WPF 1.0.6

Map folders use a deterministic path-based palette with collision resolution for up to 12 visible sibling folders. Colours remain stable when sizes/order change; adding/removing siblings can change collision assignments and colours repeat beyond the palette size. Tile area still represents logical bytes. Files use their indexed category colours, with unknown/system files and the remainder neutral. Labels remain visible in both themes; the legend explains folders, file categories and size.

Drive discovery snapshots History and publishes connected/offline entries on the application's Dispatcher, including startup before a synchronization context is installed. Volume probing stays asynchronous and isolated. A generation guard prevents older discovery results replacing newer results.

Verification: Release build, 51 service/reliability assertions and actual WPF UI workflow passed. The regression invokes drive discovery from a background thread while watching collection-change thread affinity. Map screenshots verify distinct folder colours, neutral system files and a compact light layout. Electron and saved scan indexes were not changed.
