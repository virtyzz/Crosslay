# Crosslay

Lightweight Windows MVP for an external crosshair overlay. It draws a static user-defined crosshair in a transparent, frameless, always-on-top, click-through window and keeps configuration in a local JSON file.

## Implementation Plan

1. Base overlay: tray app, transparent topmost no-focus window, click-through input, procedural crosshair rendering, local `config.json`.
2. Basic editor: live controls for length, gap, thickness, opacity, dot, T-shape, outline, color, and atomic save.
3. Profiles and hotkeys: multiple profiles, active profile switching, global hotkeys through OS APIs, monitor/DPI selection.
4. Import pipeline: PNG/JPG import first, then SVG through safe raster cache, then freehand canvas as a separate editor mode.
5. Packaging: signed Windows build, compatibility notes, public anti-cheat policy, later native Qt/C++ port if strict RAM/distribution budgets are mandatory.

This MVP intentionally avoids DLL injection, process memory access, game rendering hooks, screen capture, OCR, object detection, and input automation.

## Build

```powershell
dotnet build
```

## Publish

Create a Windows installer from GitHub Actions by pushing a version tag:

```powershell
git tag v0.1.0
git push origin v0.1.0
```

The workflow builds a self-contained `win-x64` app, creates `Crosslay-Setup-<version>.exe`, uploads it as an Actions artifact, and attaches it to the matching GitHub Release.

Crosslay uses WebView2 for the editor. Windows 10/11 usually includes the WebView2 Runtime; if it is missing, install the Microsoft Edge WebView2 Runtime.

## Run

```powershell
dotnet run
```

The tray icon opens the editor on double-click. Right-click it to show/hide the overlay or exit.

The editor includes a monitor selector. The overlay falls back to the primary monitor if the saved monitor is disconnected.

PNG and JPG images can be imported as an image layer. Imported files are copied into `%APPDATA%\Crosslay\assets`, then drawn with configurable scale, opacity, offset, and anchor point. `Offset X/Y` moves the whole image; `Anchor X/Y` selects the image pixel that should align with the crosshair center when offset is zero.

The procedural crosshair layer can be disabled with `Procedural enabled`, leaving only the imported image layer visible.

## Hotkeys

Hotkeys are editable in the editor. Click a hotkey field and press the desired combination. Use `Clear` to disable an action or `Default` to restore its default binding. `Backspace` or `Delete` in a field also disables that action.

Default bindings:

- `Ctrl+Alt+X`: show/hide overlay.
- `Ctrl+Alt+Left`: previous profile.
- `Ctrl+Alt+Right`: next profile.
- `Ctrl+Alt+Up`: increase crosshair opacity.
- `Ctrl+Alt+Down`: decrease crosshair opacity.
- `Ctrl+Alt+PageUp`: increase size.
- `Ctrl+Alt+PageDown`: decrease size.
