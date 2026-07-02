# Crosslay Implementation Plan

## Product Direction

Build a lightweight external Windows crosshair overlay that does not inject into games, hook rendering APIs, read game memory, capture the screen, or automate input. The supported MVP game mode is borderless windowed.

## Stage 1 - Stable Overlay MVP

Status: done.

- Transparent frameless overlay window.
- Always-on-top and click-through behavior.
- Tray icon with show/hide, editor, and exit.
- Procedural crosshair rendering.
- Local JSON config with atomic save.
- Correct alpha rendering through layered window bitmap.
- True outline geometry that does not color-mix with the crosshair body.
- Independent procedural and image layers.

## Stage 2 - Editor And Profiles

Status: done.

Goal: make the app usable as a daily utility rather than a single test crosshair.

- Store multiple profiles in config.
- Track active profile by id.
- Editor profile list.
- Dark custom editor shell with borderless rounded window, custom title bar, top profile selector, and left navigation. Done.
- Use Crosslay visual identity based on `dayz-map.ru` colors: near-black background, graphite panels, muted text, red primary accent, and blue secondary accent. Done.
- Create, duplicate, delete, reset profile.
- Rename profile.
- Clear controls for length, gap, thickness, dot size, outline thickness, opacity, and outline opacity.
- Color buttons that only change RGB; opacity stays controlled by sliders.
- Live preview and live overlay update.
- Keep Free-version profile limit visible in code; current MVP limit is 3 profiles.

Done when: users can create and switch several crosshairs without editing JSON manually.

## Stage 3 - Hotkeys

Status: done.

- Register OS-level hotkeys for:
  - show/hide overlay;
  - next profile;
  - previous profile;
  - opacity up/down;
  - size up/down.
- Keep implementation transparent and documented; no low-level hooks unless absolutely necessary.
- MVP fixed hotkeys:
  - `Ctrl+Alt+X` show/hide overlay;
  - `Ctrl+Alt+Left/Right` previous/next profile;
  - `Ctrl+Alt+Up/Down` opacity up/down;
  - `Ctrl+Alt+PageUp/PageDown` size up/down.
- User-editable hotkeys. Done:
  - hotkey settings UI in the editor;
  - conflict validation before saving;
  - persisted bindings in config;
  - hotkey re-registration without restarting the app.

Done when: active profile and visibility can be changed during gameplay without opening the editor.

## Stage 4 - Monitor And DPI

Status: in progress.

- Select target monitor. Done.
- Move overlay to selected monitor. Done.
- React to display changes. Done.
- Preserve center alignment across DPI/resolution changes.

Done when: users with multiple monitors can reliably place the overlay on the intended display.

## Stage 5 - PNG/JPG Import

Status: in progress.

- Import PNG/JPG as image layer. Done.
- Cache imported assets under the local app data directory. Done.
- Preserve PNG alpha. Done.
- Add scale, opacity, and centering. Done.
- Add arbitrary image offset and image anchor point. Done.
- Add simple JPG background removal modes later.

Done when: users can use their own image crosshair without manual asset processing.

## Stage 6 - Packaging

Status: planned.

- Publish portable Windows build.
- Add app icon.
- Document config location.
- Add anti-cheat policy.
- Add compatibility notes.

Done when: app can be shared and tested without requiring source checkout.
