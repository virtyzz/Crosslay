# Технический документ: легковесный оверлей-прицел для игр

## 1. Цель и границы продукта

Нужно создать десктопное приложение, которое показывает пользовательский прицел поверх игры без внедрения в процесс игры и без перехвата игрового ввода. Первая целевая платформа - Windows; архитектура должна оставлять путь к Linux, Steam Deck и macOS.

Ключевые ограничения:

- потребление памяти в рабочем режиме: целевой бюджет до 50 МБ;
- размер дистрибутива: целевой бюджет до 30 МБ;
- отсутствие заметного влияния на FPS;
- прозрачное click-through окно без рамок, всегда поверх остальных окон;
- профили прицелов, горячие клавиши, импорт PNG/JPG/SVG, визуальный редактор и свободное рисование.

Главный архитектурный принцип: это внешний desktop overlay. Он не инжектится в игру, не читает память игры, не перехватывает DirectX/OpenGL/Vulkan swap chain и не модифицирует рендер игры.

## 2. Монетизация

Рекомендуемая модель: **Free + Lifetime Pro**.

Продукт воспринимается как локальная игровая утилита, а не как постоянно потребляемый сервис. Поэтому базовая платная модель должна быть одноразовой покупкой Pro-версии, а не обязательной подпиской. Подписка на старте может выглядеть неестественно для пользователя: приложение показывает прицел поверх игры и работает локально, без очевидной ежемесячной серверной ценности.

Бесплатная версия нужна как основной канал распространения и доверия. Она должна давать пользователю полноценный базовый сценарий:

- базовый overlay;
- процедурный редактор: dot, cross, circle/T-shape, gap, length, thickness, color, opacity;
- ограниченное число профилей;
- простой PNG/JPG import;
- show/hide hotkey;
- сохранение локального конфига.

Lifetime Pro должен продавать не сам факт использования своего прицела, а удобство, глубину настройки и управление наборами:

- неограниченное число профилей;
- SVG import через безопасный raster cache;
- свободное рисование в editor canvas;
- слои: procedural layer + imported image layer + freehand layer;
- recolor imported image, alpha cleanup, outline/glow/shadow;
- export/import profile pack;
- расширенные hotkeys для переключения профилей, размера и opacity;
- готовые preset packs;
- расширенный preview на темном/светлом/игровом фоне.

Импорт своего прицела не стоит полностью закрывать за оплатой. Это сильная acquisition-функция: пользователь быстрее понимает ценность приложения, если может сразу попробовать свой PNG/JPG. Более продвинутые форматы, обработку, слои и коллекции можно оставить в Pro.

Подписка уместна только как дополнительный слой, если позже появится реальная серверная ценность:

- cloud sync профилей;
- backup;
- marketplace/community packs;
- аккаунт и sharing;
- регулярно обновляемые наборы прицелов.

Возможная будущая модель: **Free + Lifetime Pro + optional Cloud/Plus subscription**. При этом все локальные функции должны оставаться доступными через Lifetime Pro, а подписка должна покрывать только облачные или регулярно обновляемые сервисные возможности.

Нельзя монетизировать функции, которые повышают античит-риски или выглядят как попытка обхода ограничений игры: injection, hooks, screen capture, OCR, object detection, automation, exclusive fullscreen bypass. Доверие к продукту важнее краткосрочной монетизации.

## 3. Сравнение технологических стеков

### Вариант A: C++ + Qt 6

**Оценка:** лучший вариант для production-версии при строгих лимитах памяти и размера.

Плюсы:

- Нативное окно, прозрачность, always-on-top и click-through можно реализовать через Qt flags и при необходимости через WinAPI.
- Qt поддерживает `Qt::WindowStaysOnTopHint`, `Qt::WindowTransparentForInput`, `Qt::FramelessWindowHint`, `WA_TranslucentBackground`.
- Хорошая поддержка SVG, PNG, JPG, HiDPI, QPainter, QOpenGLWidget/QRhi.
- Можно собрать self-contained дистрибутив без внешнего runtime.
- Реально уложиться в низкое потребление CPU/GPU при событийном рендеринге.

Минусы:

- Qt увеличивает размер дистрибутива; чтобы попасть в < 30 МБ, нужно аккуратно отбирать плагины и использовать статическую/минимальную сборку.
- C++ повышает стоимость разработки и требования к качеству памяти/ресурсов.
- Кроссплатформенное поведение прозрачных окон различается между Windows, X11, Wayland и macOS.

Когда выбирать:

- если нужен самый надежный Windows MVP и дальнейший production;
- если важнее контроль над системным окном, ресурсами и упаковкой, чем скорость UI-разработки.

### Вариант B: Rust + Tauri + Svelte/React/Vue

**Оценка:** хороший компромисс для приложения с богатым UI, но оверлей-часть лучше делать нативным Rust-модулем.

Плюсы:

- Rust дает безопасность памяти и небольшой backend.
- Tauri обычно дает меньший дистрибутив, чем Electron, потому что использует системный WebView.
- Frontend удобно использовать для редактора профилей, списков, цветовых контролов и импорта.
- Можно разделить приложение на два окна: HTML-редактор и минимальный native overlay.

Минусы:

- WebView-процесс может усложнить попадание в < 50 МБ RAM, особенно на Windows.
- Полноценный прозрачный click-through overlay зависит от возможностей WebView и платформенных API.
- Для глобальных горячих клавиш, click-through и z-order почти наверняка нужны platform-specific плагины/модули.
- WebView не идеален для "почти нулевого" рендеринга статичного прицела.

Когда выбирать:

- если редактор важен как полноценный продуктовый UI;
- если команда сильнее в web frontend;
- если допустимо реализовать overlay не в WebView, а отдельным нативным окном на Rust.

### Вариант C: Electron + TypeScript

**Оценка:** самый быстрый путь к богатому UI, но плохо подходит под требования легковесности.

Плюсы:

- Быстрая разработка редактора, профилей, импорта, настроек.
- В Electron есть прямые API для прозрачного окна, always-on-top и игнорирования mouse events.
- Большая экосистема для canvas/SVG/image processing.

Минусы:

- Память и размер дистрибутива почти наверняка выйдут за лимиты.
- Chromium-процесс ради статичного прицела избыточен.
- Может быть сложнее убедить пользователей и античит-среды, что приложение делает только простую отрисовку.

Когда выбирать:

- только для быстрого прототипа редактора;
- если лимиты < 50 МБ RAM и < 30 МБ дистрибутива можно ослабить.

### Вариант D: Python + PyQt

**Оценка:** удобен для прототипа, не подходит для финального легковесного дистрибутива.

Плюсы:

- Быстро проверить UX редактора и модель профилей.
- Qt API остаются похожими на будущую C++/Qt реализацию.
- Простая работа с изображениями, JSON, SVG.

Минусы:

- Python runtime и упаковка почти наверняка превысят лимит дистрибутива.
- Больше стартовое потребление памяти.
- Ниже доверие для production-оверлея.

Когда выбирать:

- для proof-of-concept за несколько дней;
- для проверки редактора, формата профилей и пользовательских сценариев.

### Рекомендация

Основной production-стек: **C++ + Qt 6 + нативные platform adapters**.

Аргументы:

- лучше всего соответствует целям по RAM/CPU/GPU;
- дает прямой доступ к нативным окнам;
- не требует установки .NET/Python/Node runtime;
- покрывает редактор, SVG, raster import и GPU-путь без отдельного большого web runtime.

Компромиссный вариант: **Rust + Tauri для редактора + отдельное нативное overlay-окно на Rust/winit/skia или platform API**. Этот путь интересен, если хочется современный безопасный backend и web UI, но его стоит выбирать только после отдельного прототипа click-through overlay.

Electron и Python/PyQt оставить как прототипные варианты.

## 4. Архитектурная схема

```text
+----------------------+        +----------------------+
| Settings / Editor UI |        | Global Hotkey Module |
| Qt Widgets / QML     |        | Windows/Linux/macOS  |
+----------+-----------+        +----------+-----------+
           |                               |
           v                               v
+----------+-----------+        +----------+-----------+
| Profile Store        |<------>| Runtime State        |
| JSON + asset files   |        | active profile, size |
+----------+-----------+        +----------+-----------+
           |                               |
           v                               v
+----------+-----------+        +----------+-----------+
| Crosshair Compiler   |------->| Overlay Window       |
| vector/image -> mask |        | transparent topmost  |
+----------+-----------+        +----------+-----------+
           |                               |
           v                               v
+----------+-----------+        +----------+-----------+
| Render Backend       |------->| Native Window Adapter|
| QPainter/OpenGL/QRhi |        | WinAPI/X11/macOS     |
+----------------------+        +----------------------+
```

## 5. Модули

### 5.1 Overlay Window

Отвечает за отдельное окно прицела:

- без рамки;
- прозрачный фон;
- always-on-top;
- click-through;
- не получает фокус;
- не отображается в taskbar;
- покрывает целевой монитор или весь virtual desktop;
- учитывает DPI scaling.

Windows-реализация:

- Qt: `Qt::FramelessWindowHint`, `Qt::WindowStaysOnTopHint`, `Qt::Tool`, `Qt::WindowTransparentForInput`, `WA_TranslucentBackground`, `WA_ShowWithoutActivating`.
- WinAPI fallback: `WS_EX_LAYERED`, `WS_EX_TRANSPARENT`, `WS_EX_TOPMOST`, `WS_EX_NOACTIVATE`, `SetLayeredWindowAttributes`, `SetWindowPos`.

Linux:

- X11: shaped/transparent window, `_NET_WM_STATE_ABOVE`, input shape region empty for click-through.
- Wayland: ограниченная управляемость overlay-окон; для Steam Deck/Gamescope понадобится отдельная проверка. Возможен режим XWayland или Gamescope layer-shell, если окружение разрешает.

macOS:

- `NSWindow` с transparent background, floating/screen-saver level, `ignoresMouseEvents = true`, non-activating panel.

### 5.2 Render Backend

Задача: отрисовать только прицел, без постоянной перерисовки всего экрана.

Режимы:

- Vector primitives: линии, круги, точки, gap, outline.
- Raster mask: PNG/JPG/SVG после нормализации в `QImage`/texture.
- Freehand layer: набор stroke-команд или растровый слой.

Для MVP достаточно `QPainter` поверх прозрачного QWidget: он аппаратно не всегда GPU-ускорен, но при статичном маленьком изображении нагрузка будет околонулевая. Для дальнейшего развития можно добавить QRhi/OpenGL path, где прицел - заранее подготовленная texture quad.

Ключевая оптимизация: компилировать профиль в готовый bitmap/texture и перерисовывать окно только при изменении профиля, размера, прозрачности, DPI или позиции.

### 5.3 Hotkey Module

Функции:

- показать/скрыть прицел;
- следующий/предыдущий профиль;
- увеличить/уменьшить размер;
- увеличить/уменьшить opacity;
- временно включить режим редактирования.

Windows:

- сначала `RegisterHotKey` для простых глобальных сочетаний;
- если нужны удержания/сложные бинды - low-level keyboard hook, но только после оценки античит-рисков.

Linux:

- X11 global shortcuts через XGrabKey;
- Wayland зависит от desktop portal/compositor и может требовать пользовательской настройки.

macOS:

- Event HotKey API или CGEventTap, с учетом разрешений Accessibility.

### 5.4 Crosshair Editor

Редактор отделен от overlay-окна. Это обычное окно настроек, которое может принимать ввод.

Основные панели:

- список профилей;
- предпросмотр на темном/светлом/игровом фоне;
- конструктор формы: dot, cross, circle, T-shape, outline, gap, length, thickness;
- цвет и opacity;
- размер и scale;
- импорт изображения;
- свободное рисование;
- hotkey bindings;
- monitor/DPI настройки.

Изменения в редакторе обновляют runtime state и пересобирают render artifact, но overlay не должен запускать постоянный animation loop.

### 5.5 Profile Store

Формат: JSON. Он проще для UI, миграций и ручного редактирования.

Структура:

```json
{
  "version": 1,
  "activeProfileId": "classic-green",
  "profiles": [
    {
      "id": "classic-green",
      "name": "Classic Green",
      "type": "procedural",
      "enabled": true,
      "opacity": 0.95,
      "scale": 1.0,
      "hotkey": "Ctrl+Alt+1",
      "shape": {
        "dot": true,
        "dotSize": 2,
        "lines": true,
        "lineLength": 9,
        "lineThickness": 2,
        "gap": 5,
        "outline": true,
        "outlineThickness": 1,
        "color": "#35ff5a",
        "outlineColor": "#000000"
      },
      "asset": null
    }
  ],
  "hotkeys": {
    "toggleVisible": "Ctrl+Alt+X",
    "nextProfile": "Ctrl+Alt+Right",
    "previousProfile": "Ctrl+Alt+Left",
    "increaseOpacity": "Ctrl+Alt+Up",
    "decreaseOpacity": "Ctrl+Alt+Down"
  }
}
```

Файловая структура:

```text
CrosshairOverlay/
  config.json
  profiles/
    classic-green.json
  assets/
    imported/
      my-crosshair.png
    cache/
      classic-green@1x.png
      classic-green@2x.png
```

## 6. Минимальное потребление ресурсов

Модель работы:

1. При запуске загрузить config и активный профиль.
2. Скомпилировать профиль в готовый маленький surface/texture.
3. Показать прозрачное окно поверх экрана.
4. После первой отрисовки не запускать таймер кадров.
5. Перерисовывать только по событиям:
   - смена профиля;
   - изменение размера/opacity;
   - изменение DPI/монитора;
   - show/hide;
   - импорт/редактирование.

Практические меры:

- не использовать polling для поиска окна игры;
- не использовать screen capture;
- не делать постоянный `requestAnimationFrame`/animation loop;
- кешировать SVG/raster в нужных DPI;
- держать overlay window маленьким, если возможна точная позиция центра экрана; если нужно покрывать весь экран, repaint region должен быть ограничен областью прицела;
- отключить лишние Qt модули и плагины в дистрибутиве;
- lazy-load редактор: в tray/overlay режиме UI редактора можно не держать открытым.

## 7. Совместимость с DirectX, OpenGL, Vulkan

Рекомендуемый подход: **не интегрироваться с графическим backend игры вообще**.

Приложение создает отдельное системное окно поверх borderless-windowed игры. Это совместимо с DirectX/OpenGL/Vulkan потому, что игра и overlay рендерятся оконным менеджером/композитором ОС, а не одним swap chain.

Ограничения:

- exclusive fullscreen может скрывать внешние overlay-окна или переводить игру в режим, где compositor не смешивает окна;
- поэтому основной поддерживаемый режим - borderless windowed;
- на некоторых играх с античитом topmost/click-through overlay может быть запрещен политикой игры;
- HDR, variable refresh rate и multi-monitor DPI требуют отдельного тестирования.

Почему не injection/hooking:

- DirectX/OpenGL/Vulkan hook повышает риск детекта античитом;
- ломает совместимость между играми;
- требует поддержки множества API и версий;
- прямо противоречит цели "не влиять на FPS".

## 8. Реализация редактора прицелов

### 8.1 Процедурный редактор

Параметры:

- color;
- outline color;
- opacity;
- center dot on/off;
- dot size;
- line count/form: cross, T, circle, chevron;
- line length;
- line thickness;
- center gap;
- rotation;
- scale;
- anti-aliasing on/off;
- per-profile hotkey.

Каждый параметр меняет модель профиля. После изменения вызывается `CrosshairCompiler`, который строит новый render artifact. Предпросмотр обновляется сразу, overlay - с debounce 16-50 мс, чтобы слайдеры не создавали лишнюю нагрузку.

### 8.2 SVG import

Pipeline:

1. Пользователь выбирает SVG.
2. Приложение проверяет размер файла и запрещает внешние ссылки/скрипты.
3. SVG загружается через безопасный renderer (`QSvgRenderer` или отдельный sanitizer + rasterizer).
4. Нормализуется viewBox и центр.
5. Рендерится в `QImage` нужного размера с alpha channel.
6. Кешируется PNG/texture для каждого DPI scale.

Прямое использование SVG допустимо в редакторе/preview, но для overlay лучше raster cache: меньше CPU, стабильнее предсказуемость и проще маска прозрачности.

### 8.3 PNG/JPG import и alpha mask

PNG:

- сохранять исходный alpha channel;
- при необходимости применять global opacity;
- нормализовать размер и anchor point.

JPG:

- alpha отсутствует, поэтому нужен один из режимов:
  - chroma key по выбранному цвету фона;
  - luminance mask;
  - ручной threshold;
  - "treat black/white as transparent".

Маска:

```text
source image -> RGBA image -> alpha normalization -> optional recolor -> premultiplied alpha -> cached texture
```

Для четкости тонких линий нужно учитывать device pixel ratio и использовать premultiplied alpha.

### 8.4 Свободное рисование

Редактирование не должно происходить в click-through overlay-окне. Нужен отдельный режим:

- пользователь открывает editor canvas;
- canvas принимает ввод пера/мыши;
- strokes сохраняются как векторные команды: точки, pressure, color, width;
- при сохранении strokes компилируются в bitmap layer;
- overlay получает готовый layer как часть профиля.

Если нужен режим "рисовать прямо поверх игры", приложение временно отключает click-through и показывает явный editing mode. По умолчанию этот режим выключен, чтобы не мешать игре.

## 9. Безопасность и античит

Почему подход относительно безопасен:

- нет DLL injection;
- нет чтения/записи памяти процесса игры;
- нет перехвата swap chain;
- нет анализа изображения игры;
- нет автоматизации ввода;
- overlay показывает статичную пользовательскую графику, аналогично системным overlay/OSD.

Риски:

- некоторые античиты блокируют или ограничивают любые topmost overlay;
- low-level keyboard hooks могут выглядеть подозрительно;
- screen capture, OCR, object detection или автоматическая реакция на картинку резко повышают риск классификации как cheat tooling;
- попытки работать в exclusive fullscreen могут потребовать техник, похожих на hook/injection.

Минимизация рисков:

- публично документировать, что приложение не внедряется в игру и не читает игровые данные;
- не использовать kernel drivers;
- не использовать process injection;
- не использовать memory scanning;
- не использовать screen capture;
- горячие клавиши сначала делать через официальные OS APIs;
- подписывать бинарники code signing certificate;
- хранить понятный config и логировать только состояние приложения;
- сделать allowlist/denylist режимов для игр, если конкретная игра запрещает overlay.

Важно: нельзя гарантировать совместимость с каждым античитом. Решение должно быть честно позиционировано как внешний визуальный overlay для игр, где такие overlay разрешены правилами игры.

## 10. План разработки MVP

### Итерация 1: базовый overlay

Цель: доказать, что topmost transparent click-through окно работает поверх borderless-windowed игры без падения FPS.

Задачи:

1. Создать C++/Qt приложение с tray icon.
2. Создать frameless transparent overlay window.
3. Включить always-on-top, no-focus, click-through.
4. Нарисовать простой процедурный crosshair через QPainter.
5. Загрузить параметры из `config.json`.
6. Добавить show/hide из tray menu.
7. Замерить:
   - RAM;
   - CPU idle;
   - CPU при смене параметров;
   - поведение поверх 3-5 игр в borderless windowed.

Критерий готовности:

- прицел виден поверх borderless-windowed игры;
- клики проходят в игру;
- idle CPU около 0%;
- нет постоянного repaint loop.

Оценка: 1-2 недели.

### Итерация 2: редактор и импорт

Цель: дать пользователю создавать и импортировать прицелы.

Задачи:

1. Окно editor UI.
2. Процедурные параметры: цвет, opacity, dot, length, gap, thickness, outline.
3. Live preview.
4. PNG/JPG import.
5. SVG import через безопасный raster cache.
6. Freehand canvas в редакторе.
7. Сохранение профиля и asset cache.
8. Debounced live update overlay.

Критерий готовности:

- пользователь создает профиль без ручного JSON;
- импортированные изображения корректно отображаются с alpha;
- SVG не требует постоянного runtime-рендеринга в overlay.

Оценка: 2-4 недели.

### Итерация 3: профили, hotkeys, автозагрузка

Цель: сделать приложение удобным в реальном игровом сценарии.

Задачи:

1. Список профилей и active profile.
2. Глобальные hotkeys:
   - toggle visible;
   - next/previous profile;
   - opacity +/-;
   - size +/-.
3. Windows autostart setting.
4. Monitor selection и DPI handling.
5. Export/import profile pack.
6. Crash-safe config write: atomic save через temp file + rename.
7. Code signing для Windows build.
8. Минимальный updater или ручная проверка версии.

Критерий готовности:

- профили переключаются без открытия редактора;
- настройки переживают перезапуск;
- автозагрузка опциональна;
- работа hotkeys не конфликтует с игрой в типовых сценариях.

Оценка: 2-3 недели.

## 11. Оценка сложности и рисков

Сложность по областям:

- Windows overlay basics: средняя.
- Click-through/no-focus поведение: средняя, нужны edge-case тесты.
- Linux/Wayland поддержка: высокая.
- SVG/image import: средняя.
- Freehand editor: средняя.
- Global hotkeys: средняя на Windows, высокая для Wayland/macOS permissions.
- Попадание в < 30 МБ дистрибутива с Qt: средняя/высокая, зависит от сборки.
- Античит-совместимость: высокая неопределенность, нужна честная матрица совместимости.

Главные риски:

1. Exclusive fullscreen не поддержит внешний overlay.
   Решение: официально поддерживать borderless windowed.

2. Античит конкретной игры блокирует overlay.
   Решение: не обходить блокировки, документировать ограничения.

3. Wayland не разрешит нужное позиционирование/topmost.
   Решение: Linux MVP через X11/XWayland, Steam Deck исследовать отдельно.

4. Qt-дистрибутив превысит 30 МБ.
   Решение: минимальная сборка, исключение лишних plugins/translations, оценить Rust-native overlay как fallback.

5. Постоянный repaint начнет потреблять ресурсы.
   Решение: event-driven rendering, cached bitmap, repaint only dirty region.

## 12. Рекомендуемая структура репозитория

```text
crosshair-overlay/
  CMakeLists.txt
  src/
    main.cpp
    app/
      Application.cpp
      TrayController.cpp
    overlay/
      OverlayWindow.cpp
      OverlayWindow.h
      RenderWidget.cpp
      RenderWidget.h
    platform/
      WindowAdapter.h
      windows/WindowsWindowAdapter.cpp
      linux/X11WindowAdapter.cpp
      mac/MacWindowAdapter.mm
    hotkeys/
      HotkeyManager.h
      windows/WindowsHotkeyManager.cpp
    profiles/
      ProfileStore.cpp
      CrosshairProfile.h
      ProfileMigrator.cpp
    render/
      CrosshairCompiler.cpp
      RenderArtifact.h
      SvgImporter.cpp
      ImageMask.cpp
    editor/
      EditorWindow.cpp
      PreviewWidget.cpp
      FreehandCanvas.cpp
  resources/
    icons/
  packaging/
    windows/
    linux/
    macos/
  docs/
    compatibility.md
    anticheat-policy.md
```

## 13. Практический MVP API

Минимальные интерфейсы:

```cpp
struct CrosshairProfile {
    QString id;
    QString name;
    float opacity;
    float scale;
    CrosshairShape shape;
    std::optional<QString> assetPath;
};

class ProfileStore {
public:
    Config load();
    void saveAtomic(const Config& config);
};

class CrosshairCompiler {
public:
    RenderArtifact compile(const CrosshairProfile& profile, qreal devicePixelRatio);
};

class OverlayWindow {
public:
    void showOverlay();
    void hideOverlay();
    void setArtifact(const RenderArtifact& artifact);
    void setClickThrough(bool enabled);
    void setAlwaysOnTop(bool enabled);
};

class HotkeyManager {
public:
    bool registerHotkey(QString action, QString sequence);
    void unregisterAll();
};
```

## 14. Источники и проверенные API

- Electron BrowserWindow: `transparent`, `setAlwaysOnTop`, `setIgnoreMouseEvents`, platform notes for Wayland: https://www.electronjs.org/docs/latest/api/browser-window
- Qt Window flags: `Qt::WindowStaysOnTopHint`, `Qt::WindowTransparentForInput`: https://doc.qt.io/qt-6/qt.html
- Qt Widget attribute `WA_TranslucentBackground`: https://doc.qt.io/qt-6/qt.html

## 15. Итог

Для требований легковесности, низкого влияния на FPS и минимального риска со стороны античитов оптимальная стратегия - нативный внешний overlay без внедрения в игру. Production-реализацию стоит строить на C++/Qt 6 с thin platform adapters для Windows/Linux/macOS. Прицел должен компилироваться в cached bitmap/texture и перерисовываться только при изменениях. Основной поддерживаемый игровой режим - borderless windowed; exclusive fullscreen не должен быть целевым сценарием MVP.
