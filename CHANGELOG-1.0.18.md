# DiskLens WPF 1.0.18

Ranked sizes and Explorer table now include an Open folder toolbar action. A selected folder opens in Windows Explorer; a selected file opens its containing folder with the file selected. The action is disabled without a selection or for aggregate rows. Open selected continues to navigate inside DiskLens.

UI regression checks verify the toolbar command in both list views, folder opening without changing the current DiskLens folder, file selection in Explorer and disabled states. The 640-pixel compact layout is rendered for verification.
