# Blood Dragon Menu

Система главного меню и экрана **«Справка и параметры»** в стиле Far Cry 3: Blood Dragon.

Движок: **Godot 4.8-dev2 .NET**. Язык: **C# / .NET 8**. Автор репозитория: [assassins377](https://github.com/assassins377).

Это не полная игра, а рабочий UI-каркас: навигация, сохранение настроек, CRT-оверлей, звук меню и заглушка 3D-уровня, чтобы сразу проверить графику и управление.

Полная постановка: [`game_menu_specification.md`](game_menu_specification.md). Лицензия: [MIT](LICENSE).

---

## Скриншоты

Реальные кадры из запущенного проекта (1920×1080, CRT scanlines).

### Главное меню

![Главное меню Blood Dragon: заголовок BLOOD DRAGON 1.0, кнопки Кампания, Справка и параметры, Выйти из игры](screenshots/main_menu.png)

Тёмный фон `#020d05`, неоновый акцент `#00ff00`. Заголовок с мигающим курсором `█`. Активная кнопка — сплошная зелёная плашка, текст тёмный. Неактивные пункты — серый моноширинный текст без рамки.

### Справка и параметры

![Меню настроек: категории слева, панель Изображение справа в зелёной рамке, кнопки Принять и Назад](screenshots/settings_menu.png)

Две колонки: слева список категорий, справа панель в зелёной рамке. **Принять** записывает буфер в `user://settings.cfg` и применяет к движку. **Назад** выходит без применения.

### Заглушка кампании

![Тестовая 3D-сцена: зелёные кубы, подсказки WASD / Shift / пробел / Esc](screenshots/game.png)

Кампания открывает простой 3D-полигон: движение, бег, прыжок, пауза. Нужен, чтобы настройки (FOV, MSAA, тени, громкость, бинды) было куда применять.

---

## Что умеет

| Экран | Поведение |
|--------|-----------|
| Главное меню | Кампания / справка / выход, CRT, эмбиент, hover/select SFX |
| Справка и параметры | Категории, буфер изменений, Принять / Назад, Esc = назад |
| Игра | First-person заглушка + применение графики через `GraphicsController` |

### Категории настроек

| Категория | Содержание |
|-----------|------------|
| Изображение | разрешение, окно/полный экран, VSync, GPU frames, letterbox, DirectX, MSAA, A2C, SSAO, FOV |
| Калибровка | яркость, контраст, гамма (шейдер) |
| Качество видео | общее / текстуры / тени / свет / пост / вода / дальность |
| Элементы управления | чувствительность мыши и геймпада, инверсия Y, переназначение клавиш |
| Игровой процесс | сложность, подсказки, автоприцеливание |
| Язык | UI, озвучка, субтитры, размер шрифта |
| Звук | Master / Music / SFX / Dialogue / Ambient, динамический диапазон, вывод, озвучка меню |
| Руководство | статичный текст |
| Титры | версия, движок, лицензии |

Навигация: мышь, стрелки / Enter / Esc, геймпад. Геймпад игнорируется, пока окно не в фокусе.

Кнопки главного меню заезжают слева через **Control offset transforms**: анимация визуальная, hitbox остаётся на месте layout.

---

## Требования и запуск

- Godot **4.8-dev2 .NET** (сборка `Godot_v4.8-dev2_mono_linux`)
- **.NET 8** SDK

1. Клонируйте репозиторий и откройте папку в Godot 4.8-dev2 (.NET).
2. Дождитесь восстановления NuGet и сборки C#.
3. Запустите проект: главная сцена — `scenes/main_menu/MainMenu.tscn`.

### Linux и виртуальные машины

На Linux проект явно ставит **GL Compatibility** (`rendering/renderer/rendering_method.linuxbsd`). Это не «даунгрейд ради VM», а рекомендация [документации Godot](https://docs.godotengine.org/en/4.7/tutorials/rendering/renderers.html) для 2D и простого 3D: OpenGL 3.3, низкая базовая стоимость кадра.

Почему не Vulkan/Forward+ в госте Linux:

- С Godot 4.4 движок падает на OpenGL, **только если Vulkan совсем нет**. Mesa **Lavapipe/llvmpipe — валидный Vulkan 1.4**, поэтому Godot выбирает его и рисует на CPU. Fallback до Compatibility не срабатывает ([GH-97142](https://github.com/godotengine/godot/pull/97142), [Ubuntu/Godot #2072335](https://bugs.launchpad.net/bugs/2072335)).
- VMware SVGA II в госте даёт **OpenGL 3.3/4.x через vmwgfx**. Гостевой Vulkan на SVGA обычно и есть lavapipe. 3D Acceleration на хосте включает именно GL, не отдельный GPU Vulkan в госте ([Broadcom Workstation](https://techdocs.broadcom.com/us/en/vmware-cis/desktop-hypervisors/workstation-pro/26H1/using-vmware-workstation-pro/configuring-and-managing-virtual-machines/configure-display-settings-for-a-virtual-machine/prepare-the-host-system-to-use-accelerated-3d-graphics.html)).

Проверка: в логе должно быть `OpenGL 3` / `Compatibility`, не `Vulkan … llvmpipe`. Принудительно:

```bash
godot --rendering-method gl_compatibility --rendering-driver opengl3
```

Если адаптер всё равно llvmpipe, в коде режутся MSAA/SSAO/SSIL/glow/шум CRT. SSIL на Compatibility нет вообще. Windows остаётся Forward+ / D3D12.

Autoload: `SettingsManager`, `AudioManager`.

---

## Архитектура (кратко)

| Часть | Где |
|--------|-----|
| Модель настроек | `scripts/settings/GameSettings.cs` |
| Сохранение / применение | `scripts/settings/SettingsManager.cs` → `user://settings.cfg` |
| Главное меню | `scripts/menus/MainMenu.cs` |
| Экран параметров | `scripts/menus/SettingsMenu.cs` + панели в `scenes/settings_menu/categories/` |
| Строки UI | `CycleOption`, `SliderRow`, `KeyBindingRow` |
| Тема | `scripts/ui/MenuTheme.cs` (палитра, моноширинный шрифт) |
| Графика в игре | `scripts/game/GraphicsController.cs` |
| CRT / калибровка | `shaders/` |

```
scenes/     главное меню, настройки, заглушка уровня
scripts/    C#: меню, настройки, аудио, игрок
shaders/    CRT, glow, калибровка
audio/      эмбиент меню и UI-звуки
screenshots/  кадры для README
```

---

## Лицензия

MIT. Godot Engine — MIT.

Визуальный язык вдохновлён Far Cry 3: Blood Dragon; это фанатский UI-прототип, не связанный с Ubisoft.
