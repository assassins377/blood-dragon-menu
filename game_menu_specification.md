# Спецификация системы меню для игры
**Движок:** Godot 4.6 (.NET / C#)  
**Язык:** C# (.NET 8)  
**Версия документа:** 1.3  
**Дата создания:** 2026

---

## 📋 Содержание

1. [Главное меню](#главное-меню)
2. [Меню "Справка и параметры"](#меню-справка-и-параметры)
3. [Функциональные требования](#функциональные-требования)
4. [Технические требования Godot](#технические-требования-godot)
5. [Адаптивность](#адаптивность)
6. [Анимации и эффекты](#анимации-и-эффекты)
7. [C# Архитектура](#c-архитектура)
8. [Структура проекта](#структура-проекта)

---

## 🎮 Главное меню

### Визуальный референс

Скриншот референса: тёмный фон (~#020d05), название игры в верхнем левом углу,
меню по левому краю экрана, активная кнопка — сплошная зелёная заливка.

### Расположение элементов

| Элемент | Расположение | Описание |
|---------|--------------|----------|
| **Название игры** | Верхний левый угол (отступ ~80px сверху, ~80px слева) | Крупный моноширинный текст в ВЕРХНЕМ РЕГИСТРЕ + мигающий зелёный курсор (█) |
| **Версия игры** | Рядом с названием, часть заголовка | Например: "BLOOD DRAGON 1.0 █" |
| **Кнопка "КАМПАНИЯ"** | Левая сторона экрана, вертикальное меню | Запуск/продолжение игры |
| **Кнопка "СПРАВКА И ПАРАМЕТРЫ"** | Левая сторона экрана, вертикальное меню | Открытие меню настроек |
| **Кнопка "ВЫЙТИ ИЗ ИГРЫ"** | Левая сторона экрана, вертикальное меню | Закрытие приложения |

### Дизайн кнопок

**Неактивная кнопка:**
- Фон: прозрачный
- Текст: светло-серый/белый (#cccccc), обычное начертание
- Регистр: ВЕРХНИЙ

**Активная / выбранная кнопка:**
- Фон: сплошной зелёный (#00ff00 или #39ff14), ширина ~500px
- Текст: тёмный (#0a0a0a), полужирный (bold)
- Регистр: ВЕРХНИЙ

**Переход при наведении:** мгновенная смена фона (без плавности) или быстрый fade 0.05s.

### Цветовая схема

| Параметр | Значение |
|----------|----------|
| Фон экрана | #020d05 (очень тёмный зелёный) |
| Акцентный цвет | #00ff00 / #39ff14 (неоновый зелёный) |
| Текст неактивный | #cccccc |
| Текст активный | #0a0a0a |
| Шрифт | Моноширинный — Courier New, Consolas или кастомный пиксельный |
| Регистр всего текста | ВЕРХНИЙ (uppercase через Theme или String.ToUpper()) |

### Эффекты главного меню

- Мигающий курсор █ рядом с названием (1s цикл, toggle через Timer)
- CRT scanlines поверх всего (ShaderMaterial на CanvasLayer)
- Лёгкое свечение (glow) активного элемента
- Виньетка по краям (опционально)

---

## ⚙️ Меню "Справка и параметры"

### Визуальный референс

Скриншот референса: двухколоночный layout. Левая колонка — категории, правая — настройки в зелёной рамке. Активная категория — сплошная зелёная заливка. Активная строка настройки — зелёная заливка + стрелки ◄ значение ►. Внизу кнопки ПРИНЯТЬ и НАЗАД.

### Общая структура

Двухколоночный интерфейс:
- **Левая колонка** (~35% ширины) — VBoxContainer с кнопками категорий без рамки
- **Правая колонка** (~65% ширины) — PanelContainer с **зелёной рамкой** (StyleBoxFlat, border_color=#00ff00, border_width=2px) + ScrollContainer внутри

При переключении категории правая колонка обновляется (hide/show панели, опционально slide 0.2s).

**Кнопки внизу экрана (правый нижний угол):**
- **ПРИНЯТЬ** — применить изменения и вернуться в главное меню
- **НАЗАД** — вернуться без сохранения (или без применения)

### UI строк настроек (кастомный формат)

Каждая настройка — это **SettingRow** (HBoxContainer):
- Левая часть: `Label` с названием настройки (ВЕРХНИЙ РЕГИСТР)
- Правая часть: значение

**Типы строк:**

| Тип | Внешний вид | Godot нода |
|-----|-------------|------------|
| Цикличный выбор | `◄  ЗНАЧЕНИЕ  ►` (стрелки видны при фокусе) | Кастомный `CycleOption` |
| Слайдер | горизонтальный ползунок | `HSlider` |
| Переключатель | `ВКЛ` / `ВЫКЛ` (как цикличный из 2 значений) | Кастомный `CycleOption` |

**Состояния строки:**
- **Обычная**: прозрачный фон, белый текст (#cccccc)
- **Активная/фокус**: зелёная заливка (#00ff00), тёмный текст (#0a0a0a), стрелки ◄ ► появляются
- **Недоступная (greyed out)**: текст серый (~#555555), не реагирует на ввод

> Пример: "ВЕРТ. СИНХРОНИЗАЦИЯ" серая, когда выбран режим совместимости или DirectX 9.

### Категории настроек

#### 1. 🖼️ ИЗОБРАЖЕНИЕ

Отображаемое название: **ИЗОБРАЖЕНИЕ**

| Настройка (label) | Тип строки | C# тип | Значения | По умолчанию |
|-------------------|-----------|---------|----------|--------------|
| РАЗРЕШЕНИЕ ЭКРАНА | CycleOption | `Vector2I` | 1280×720 / 1600×900 / 1920×1080 / 2560×1440 / 3840×2160 | 1600×900 |
| ОКОННЫЙ РЕЖИМ | CycleOption | `DisplayServer.WindowMode` | В окне / Полноэкранный | В окне |
| ВЕРТ. СИНХРОНИЗАЦИЯ | CycleOption | `bool` | ВКЛ / ВЫКЛ | ВЫКЛ *(серая, если DirectX 9)* |
| ФРЕЙМОВ В БУФЕРЕ GPU | CycleOption | `int` | 1 / 2 / 3 / 4 | 4 |
| FORMAT LETTERBOX | CycleOption | `bool` | ВКЛ / ВЫКЛ | ВКЛ |
| DIRECTX | CycleOption | `string` | DirectX 9 / DirectX 11 / DirectX 12 | DirectX 11 |
| MSAA СГЛАЖИВАНИЕ | CycleOption | `int` | ВЫКЛ / 2 / 4 / 8 | 8 |
| ALPHA TO COVERAGE | CycleOption | `AlphaToCoverage` (enum) | ВЫКЛ / СТАНДАРТНОЕ / РАСШИРЕННОЕ | РАСШИРЕННОЕ |
| МЕТОД SSAO | CycleOption | `SsaoMethod` (enum) | ВЫКЛ / SSAO / HBAO+ | SSAO |
| ПОЛЕ ЗРЕНИЯ : {value} | HSlider | `float` | 60–120 | 73.15f |

#### 2. 🔧 КАЛИБРОВКА

Отображаемое название: **КАЛИБРОВКА**

| Настройка (label) | Тип строки | C# тип | Диапазон | По умолчанию |
|-------------------|-----------|---------|----------|--------------|
| ЯРКОСТЬ : {value}% | HSlider | `float` | 0–100 | 50 |
| КОНТРАСТНОСТЬ : {value}% | HSlider | `float` | 0–100 | 100 |
| GAMMA : {value} | HSlider | `float` | 1.8–3.0 | 2.2 |

#### 3. 🎬 КАЧЕСТВО ВИДЕО

Отображаемое название: **КАЧЕСТВО ВИДЕО**

| Настройка (label) | Тип строки | C# тип | Значения | По умолчанию |
|-------------------|-----------|---------|----------|--------------|
| ОБЩЕЕ КАЧЕСТВО | CycleOption | `QualityLevel` | НИЗКОЕ / СРЕДНЕЕ / ВЫСОКОЕ | ВЫСОКОЕ |
| КАЧЕСТВО ТЕКСТУР | CycleOption | `QualityLevel` | НИЗКОЕ / СРЕДНЕЕ / ВЫСОКОЕ | ВЫСОКОЕ |
| ТЕНИ | CycleOption | `bool` | ВКЛ / ВЫКЛ | ВКЛ |
| КАЧЕСТВО ТЕНЕЙ | CycleOption | `QualityLevel` | НИЗКОЕ / СРЕДНЕЕ / ВЫСОКОЕ | ВЫСОКОЕ |
| ОСВЕЩЕНИЕ | CycleOption | `QualityLevel` | НИЗКОЕ / СРЕДНЕЕ / ВЫСОКОЕ | ВЫСОКОЕ |
| ПОСТ-ОБРАБОТКА | CycleOption | `bool` | ВКЛ / ВЫКЛ | ВКЛ |
| КАЧЕСТВО ВОДЫ | CycleOption | `QualityLevel` | НИЗКОЕ / СРЕДНЕЕ / ВЫСОКОЕ | СРЕДНЕЕ |
| ДАЛЬНОСТЬ ПРОРИСОВКИ | CycleOption | `DrawDistance` | НИЗКАЯ / СРЕДНЯЯ / ВЫСОКАЯ | СРЕДНЯЯ |

#### 4. 🎮 ЭЛЕМЕНТЫ УПРАВЛЕНИЯ

Отображаемое название: **ЭЛЕМЕНТЫ УПРАВЛЕНИЯ**

| Настройка (label) | Тип строки | C# тип | По умолчанию |
|-------------------|-----------|---------|--------------|
| ЧУВСТВИТЕЛЬНОСТЬ МЫШИ : {value}% | HSlider | `float` | 50 |
| ИНВЕРСИЯ МЫШИ ПО Y | CycleOption | `bool` | ВЫКЛ |
| ЧУВСТВИТЕЛЬНОСТЬ КОНТРОЛЛЕРА : {value}% | HSlider | `float` | 50 |

**Переназначение клавиш** (KeyBindingRow на каждое действие):

| Действие | InputAction name | По умолчанию |
|----------|-----------------|--------------|
| Движение вперёд | `move_forward` | W |
| Движение назад | `move_backward` | S |
| Движение влево | `move_left` | A |
| Движение вправо | `move_right` | D |
| Прыжок | `jump` | Space |
| Присесть | `crouch` | Ctrl |
| Бег | `sprint` | Shift |
| Стрельба | `shoot` | ЛКМ |
| Прицеливание | `aim` | ПКМ |
| Перезарядка | `reload` | R |
| Взаимодействие | `interact` | E |
| Инвентарь | `inventory` | I |
| Карта | `map` | M |
| Пауза | `pause` | Esc |

#### 5. 🕹️ ИГРОВОЙ ПРОЦЕСС

Отображаемое название: **ИГРОВОЙ ПРОЦЕСС**

Новая категория. Содержит настройки геймплея (заполнить по мере необходимости):

| Настройка (label) | Тип строки | C# тип | По умолчанию |
|-------------------|-----------|---------|--------------|
| СЛОЖНОСТЬ | CycleOption | `DifficultyLevel` (enum) | НОРМАЛЬНАЯ |
| ПОДСКАЗКИ | CycleOption | `bool` | ВКЛ |
| АВТОПРИЦЕЛИВАНИЕ | CycleOption | `bool` | ВЫКЛ |

#### 6. 🌐 ЯЗЫК ИГРЫ (категория: не видна на скриншоте — встроена в ИЗОБРАЖЕНИЕ или отдельная)

> **Примечание:** На скриншоте в панели ИЗОБРАЖЕНИЕ присутствует строка "ЯЗЫК СУБТИТРОВ". Возможно, языковые настройки частично вынесены в другие категории. Уточнить у разработчика. Ниже — стандартная отдельная категория.

Отображаемое название: **ЯЗЫК** *(если будет отдельной категорией)*

| Настройка (label) | Тип строки | C# тип | По умолчанию |
|-------------------|-----------|---------|--------------|
| ЯЗЫК ИНТЕРФЕЙСА | CycleOption | `string` (locale) | `"ru"` |
| ЯЗЫК ОЗВУЧКИ | CycleOption | `string` | `"ru"` |
| ЯЗЫК СУБТИТРОВ | CycleOption | `string` | ВЫКЛ |
| ШРИФТ | CycleOption | `FontSize` (enum) | СТАНДАРТНЫЙ |

Поддерживаемые локали: `ru`, `en`, `de`, `fr`, `es`, `it`, `ja`, `zh`.

#### 7. 🔊 ЗВУК

Отображаемое название: **ЗВУК**

| Настройка (label) | Тип строки | C# тип | AudioBus | По умолчанию |
|-------------------|-----------|---------|----------|--------------|
| ОБЩАЯ ГРОМКОСТЬ : {value}% | HSlider | `float` | `"Master"` | 80 |
| ГРОМКОСТЬ МУЗЫКИ : {value}% | HSlider | `float` | `"Music"` | 70 |
| ГРОМКОСТЬ ЭФФЕКТОВ : {value}% | HSlider | `float` | `"SFX"` | 90 |
| ГРОМКОСТЬ ДИАЛОГОВ : {value}% | HSlider | `float` | `"Dialogue"` | 85 |
| ГРОМКОСТЬ ОКРУЖЕНИЯ : {value}% | HSlider | `float` | `"Ambient"` | 75 |
| ДИНАМИЧЕСКИЙ ДИАПАЗОН | CycleOption | `DynamicRange` (enum) | — | СРЕДНИЙ |
| РЕЖИМ ВЫВОДА ЗВУКА | CycleOption | `AudioOutput` (enum) | — | СТЕРЕО |
| ОЗВУЧКА МЕНЮ | CycleOption | `bool` | — | ВКЛ |

#### 8. 📖 РУКОВОДСТВО

Отображаемое название: **РУКОВОДСТВО**

Статическая страница с руководством по игре (RichTextLabel, read-only, прокрутка). Содержит:
- Управление (краткий список)
- Механики игры
- Советы

#### 9. 📜 ТИТРЫ

Отображаемое название: **ТИТРЫ**

Информационная панель (RichTextLabel, read-only):

| Поле | Содержимое |
|------|------------|
| Название игры | BLOOD DRAGON |
| Версия | v1.0.0 |
| Разработчик | [Имя студии] |
| Движок | Godot 4.6 .NET |
| Год выпуска | 2026 |
| Лицензии | Список open-source компонентов |
| Контакты | Сайт, email, соцсети |

---

## ⚡ Функциональные требования

### Навигация

| Устройство | Управление |
|------------|------------|
| Мышь | Клик для выбора, скролл для прокрутки |
| Клавиатура | ↑↓ навигация, ←→ изменение значений, Enter — подтверждение, Esc — назад |
| Геймпад | D-Pad навигация, A/Cross — подтверждение, B/Circle — назад |

### Визуальная обратная связь

- Активная кнопка: зелёная заливка (#00ff00), тёмный текст
- Hover (мышь): та же зелёная заливка (нет разницы между hover и focus)
- Звук при наведении: короткий blip (0.05s) — опционально
- Звук при выборе: подтверждение (0.1s) — опционально

### Сохранение настроек

- Путь: `user://settings.cfg`
- Формат: `ConfigFile` (Godot native)
- Автосохранение при изменении любой настройки
- Загрузка при старте через `SettingsManager.Load()`

**Структура settings.cfg:**
```ini
[display]
resolution_x=1600
resolution_y=900
fullscreen=false
vsync=true
fps_limit=60
gpu_frames_in_flight=4
letterbox=true
directx="dx11"
msaa=8
alpha_to_coverage="extended"
ssao="ssao"
fov=73.15

[calibration]
brightness=0.5
contrast=1.0
gamma=2.2

[video_quality]
overall_quality="high"
texture_quality="high"
shadows=true
shadow_quality="high"
lighting="high"
antialiasing=4
post_processing=true
water_quality="medium"
draw_distance="medium"

[controls]
mouse_sensitivity=0.5
invert_y=false
controller_sensitivity=0.5
key_move_forward="W"
key_move_backward="S"
key_move_left="A"
key_move_right="D"
key_jump="SPACE"
key_crouch="CTRL"
key_sprint="SHIFT"
key_shoot="MOUSE_LEFT"
key_aim="MOUSE_RIGHT"
key_reload="R"
key_interact="E"
key_inventory="I"
key_map="M"
key_pause="ESCAPE"

[audio]
master_volume=0.8
music_volume=0.7
sfx_volume=0.9
dialogue_volume=0.85
ambient_volume=0.75
dynamic_range="medium"
audio_output="stereo"
menu_voice=true

[language]
ui_language="ru"
voice_language="ru"
subtitle_language="ru"
font="standard"
```

---

## 🖥️ Технические требования Godot

### Окружение

| Параметр | Значение |
|----------|----------|
| Godot версия | 4.6 .NET |
| .NET версия | .NET 8 |
| Язык | C# 12 |
| Рендер по умолчанию | Forward+ (Vulkan) |
| Fallback рендер | Compatibility (OpenGL 3.3) |

### Минимальные системные требования

| Компонент | Минимум | Рекомендуется |
|-----------|---------|---------------|
| ОС | Windows 10 / Ubuntu 20.04 / macOS 11 | Windows 11 |
| CPU | 4 ядра, 2.5 GHz | 8 ядер, 3.5 GHz |
| RAM | 8 GB | 16 GB |
| GPU | Vulkan 1.0 / OpenGL 3.3 | Vulkan 1.2, 4 GB VRAM |
| Хранилище | 2 GB | SSD, 5 GB |

### Используемые Godot API (C#)

| Godot класс | Назначение |
|-------------|------------|
| `ConfigFile` | Сохранение/загрузка настроек |
| `DisplayServer` | Окно, разрешение, VSync, WindowMode |
| `AudioServer` | Громкость шин (`SetBusVolumeDb`) |
| `InputMap` | Переназначение клавиш (`ActionEraseEvents`, `ActionAddEvent`) |
| `TranslationServer` | Смена языка (`SetLocale`) |
| `RenderingServer` | Качество графики |
| `Environment` | Post-processing, яркость, glow |
| `Tween` | Анимации появления/перехода |
| `Timer` | Мигающий курсор |
| `SceneTree` | Смена сцен, `Quit()` |

---

## 🎬 Анимации и эффекты

### Анимации

| Эффект | Реализация | Длительность |
|--------|------------|--------------|
| Появление меню | `Tween` + `CanvasItem.Modulate.A` (fade in) | 0.3s |
| Исчезновение меню | `Tween` fade out | 0.3s |
| Переход между категориями | `Tween` + `Control.Position` (slide) | 0.2s |
| Мигающий курсор █ | `Timer` + toggle `Label.Visible` | 1s цикл |
| Hover кнопки | Мгновенная смена цвета / scale 1.02 | 0.05s |

### Визуальные шейдеры (ShaderMaterial)

| Эффект | Тип | Параметры |
|--------|-----|-----------|
| CRT scanlines | Screen-space shader | `line_count=600`, `intensity=0.15` |
| Glow активных элементов | CanvasItem shader | `glow_color=#00ff00`, `strength=0.4` |
| Виньетка | Screen-space shader | `radius=0.7`, `softness=0.4` |
| Шум/зернистость | Screen-space shader | `intensity=0.05` |

### Звуковые эффекты

| Событие | Файл | Длительность |
|---------|------|--------------|
| Наведение на кнопку | `res://audio/sfx/menu_hover.wav` | 0.05s |
| Выбор кнопки | `res://audio/sfx/menu_select.wav` | 0.1s |
| Изменение значения | `res://audio/sfx/menu_tick.wav` | 0.03s |
| Фоновая музыка меню | `res://audio/music/menu_ambient.ogg` | loop |

---

## 🏗️ C# Архитектура

### Enums

```csharp
// res://scripts/enums/GameEnums.cs
namespace BloodDragon
{
    public enum QualityLevel    { Low, Medium, High }
    public enum DrawDistance    { Low, Medium, High }
    public enum DynamicRange    { Low, Medium, High }
    public enum AudioOutput     { Stereo, Surround51, Surround71 }
    public enum FontSize        { Standard, Large, Accessibility }
    public enum DirectXVersion  { DirectX9, DirectX11, DirectX12 }
    public enum SsaoMethod      { Off, Ssao, HbaoPlus }
    public enum AlphaToCoverage { Off, Standard, Extended }
    public enum DifficultyLevel { Easy, Normal, Hard, Nightmare }
}
```

### CycleOption — кастомный компонент строки настройки

```csharp
// res://scenes/ui_components/CycleOption.cs
// Строка: LABEL NAME     ◄  ЗНАЧЕНИЕ  ►
// Стрелки появляются только при фокусе
using Godot;
using System;
using System.Collections.Generic;

namespace BloodDragon
{
    public partial class CycleOption : Control
    {
        [Export] public string SettingLabel { get; set; } = "НАСТРОЙКА";

        [Signal] public delegate void ValueChangedEventHandler(int index);

        private Label          _nameLabel;
        private Label          _leftArrow;   // "◄"
        private Label          _valueLabel;
        private Label          _rightArrow;  // "►"
        private ColorRect      _background;

        private List<string>   _options = new();
        private int            _currentIndex = 0;
        private bool           _focused = false;
        private bool           _disabled = false;

        // Цвета
        private static readonly Color ColorActive   = new("#00ff00");
        private static readonly Color ColorNormal   = new(0, 0, 0, 0);   // прозрачный
        private static readonly Color ColorTextActive  = new("#0a0a0a");
        private static readonly Color ColorTextNormal  = new("#cccccc");
        private static readonly Color ColorTextDisabled = new("#555555");

        public override void _Ready()
        {
            _background  = GetNode<ColorRect>("Background");
            _nameLabel   = GetNode<Label>("HBox/NameLabel");
            _leftArrow   = GetNode<Label>("HBox/LeftArrow");
            _valueLabel  = GetNode<Label>("HBox/ValueLabel");
            _rightArrow  = GetNode<Label>("HBox/RightArrow");

            _nameLabel.Text = SettingLabel;
            UpdateVisuals();
        }

        public void SetOptions(List<string> options, int defaultIndex = 0)
        {
            _options = options;
            _currentIndex = Mathf.Clamp(defaultIndex, 0, options.Count - 1);
            UpdateVisuals();
        }

        public int CurrentIndex => _currentIndex;
        public string CurrentValue => _options.Count > 0 ? _options[_currentIndex] : "";

        public void SetDisabled(bool disabled)
        {
            _disabled = disabled;
            UpdateVisuals();
        }

        public void SetFocused(bool focused)
        {
            _focused = focused;
            UpdateVisuals();
        }

        private void CycleNext()
        {
            if (_disabled || _options.Count == 0) return;
            _currentIndex = (_currentIndex + 1) % _options.Count;
            UpdateVisuals();
            EmitSignal(SignalName.ValueChanged, _currentIndex);
        }

        private void CyclePrev()
        {
            if (_disabled || _options.Count == 0) return;
            _currentIndex = (_currentIndex - 1 + _options.Count) % _options.Count;
            UpdateVisuals();
            EmitSignal(SignalName.ValueChanged, _currentIndex);
        }

        private void UpdateVisuals()
        {
            if (_disabled)
            {
                _background.Color   = ColorNormal;
                _nameLabel.AddThemeColorOverride("font_color", ColorTextDisabled);
                _valueLabel.AddThemeColorOverride("font_color", ColorTextDisabled);
                _leftArrow.Visible  = false;
                _rightArrow.Visible = false;
            }
            else if (_focused)
            {
                _background.Color   = ColorActive;
                _nameLabel.AddThemeColorOverride("font_color", ColorTextActive);
                _valueLabel.AddThemeColorOverride("font_color", ColorTextActive);
                _leftArrow.Visible  = true;
                _rightArrow.Visible = true;
            }
            else
            {
                _background.Color   = ColorNormal;
                _nameLabel.AddThemeColorOverride("font_color", ColorTextNormal);
                _valueLabel.AddThemeColorOverride("font_color", ColorTextNormal);
                _leftArrow.Visible  = false;
                _rightArrow.Visible = false;
            }

            _valueLabel.Text = CurrentValue;
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (!_focused || _disabled) return;

            if (@event.IsActionPressed("ui_right") || @event.IsActionPressed("ui_accept"))
                CycleNext();
            else if (@event.IsActionPressed("ui_left"))
                CyclePrev();
        }

        // Клик мышью по строке
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb && mb.Pressed
                && mb.ButtonIndex == MouseButton.Left
                && GetGlobalRect().HasPoint(mb.GlobalPosition))
            {
                CycleNext();
            }
        }
    }
}
```

### GameSettings — модель данных

```csharp
// res://scripts/settings/GameSettings.cs
using Godot;
using System.Collections.Generic;

namespace BloodDragon
{
    public class GameSettings
    {
        // Display
        public Vector2I Resolution { get; set; } = new(1600, 900);
        public bool Fullscreen { get; set; } = false;
        public bool VSync { get; set; } = true;
        public int FpsLimit { get; set; } = 60;
        public int GpuFramesInFlight { get; set; } = 4;
        public bool Letterbox { get; set; } = false;
        public RenderApi RenderApi { get; set; } = RenderApi.Vulkan;
        public Viewport.Msaa Msaa { get; set; } = Viewport.Msaa.Msaa4X;
        public bool AlphaToCoverage { get; set; } = false;
        public SsaoMethod Ssao { get; set; } = SsaoMethod.Ssao;
        public float Fov { get; set; } = 73.15f;

        // Calibration
        public float Brightness { get; set; } = 0.5f;
        public float Contrast { get; set; } = 1.0f;
        public float Gamma { get; set; } = 2.2f;

        // Video Quality
        public QualityLevel OverallQuality { get; set; } = QualityLevel.High;
        public QualityLevel TextureQuality { get; set; } = QualityLevel.High;
        public bool Shadows { get; set; } = true;
        public QualityLevel ShadowQuality { get; set; } = QualityLevel.High;
        public QualityLevel Lighting { get; set; } = QualityLevel.High;
        public bool PostProcessing { get; set; } = true;
        public QualityLevel WaterQuality { get; set; } = QualityLevel.Medium;
        public DrawDistance DrawDistance { get; set; } = DrawDistance.Medium;

        // Controls
        public float MouseSensitivity { get; set; } = 0.5f;
        public bool InvertY { get; set; } = false;
        public float ControllerSensitivity { get; set; } = 0.5f;
        public Dictionary<string, string> KeyBindings { get; set; } = new()
        {
            ["move_forward"]  = "W",
            ["move_backward"] = "S",
            ["move_left"]     = "A",
            ["move_right"]    = "D",
            ["jump"]          = "SPACE",
            ["crouch"]        = "CTRL",
            ["sprint"]        = "SHIFT",
            ["shoot"]         = "MOUSE_LEFT",
            ["aim"]           = "MOUSE_RIGHT",
            ["reload"]        = "R",
            ["interact"]      = "E",
            ["inventory"]     = "I",
            ["map"]           = "M",
            ["pause"]         = "ESCAPE"
        };

        // Audio
        public float MasterVolume { get; set; } = 0.8f;
        public float MusicVolume { get; set; } = 0.7f;
        public float SfxVolume { get; set; } = 0.9f;
        public float DialogueVolume { get; set; } = 0.85f;
        public float AmbientVolume { get; set; } = 0.75f;
        public DynamicRange DynamicRange { get; set; } = DynamicRange.Medium;
        public AudioOutput AudioOutput { get; set; } = AudioOutput.Stereo;
        public bool MenuVoice { get; set; } = true;

        // Language
        public string UiLanguage { get; set; } = "ru";
        public string VoiceLanguage { get; set; } = "ru";
        public string SubtitleLanguage { get; set; } = "ru";
        public FontSize FontSize { get; set; } = FontSize.Standard;
    }
}
```

### SettingsManager — Singleton (Autoload)

```csharp
// res://scripts/settings/SettingsManager.cs
// Добавить в Project → Project Settings → Autoload как "SettingsManager"
using Godot;

namespace BloodDragon
{
    public partial class SettingsManager : Node
    {
        public static SettingsManager Instance { get; private set; }
        public GameSettings Current { get; private set; } = new();

        private const string SavePath = "user://settings.cfg";

        public override void _Ready()
        {
            Instance = this;
            Load();
        }

        public void Save()
        {
            var cfg = new ConfigFile();

            cfg.SetValue("display", "resolution_x",       Current.Resolution.X);
            cfg.SetValue("display", "resolution_y",       Current.Resolution.Y);
            cfg.SetValue("display", "fullscreen",         Current.Fullscreen);
            cfg.SetValue("display", "vsync",              Current.VSync);
            cfg.SetValue("display", "fps_limit",          Current.FpsLimit);
            cfg.SetValue("display", "fov",                Current.Fov);

            cfg.SetValue("calibration", "brightness",     Current.Brightness);
            cfg.SetValue("calibration", "contrast",       Current.Contrast);
            cfg.SetValue("calibration", "gamma",          Current.Gamma);

            cfg.SetValue("audio", "master_volume",        Current.MasterVolume);
            cfg.SetValue("audio", "music_volume",         Current.MusicVolume);
            cfg.SetValue("audio", "sfx_volume",           Current.SfxVolume);
            cfg.SetValue("audio", "dialogue_volume",      Current.DialogueVolume);
            cfg.SetValue("audio", "ambient_volume",       Current.AmbientVolume);
            cfg.SetValue("audio", "menu_voice",           Current.MenuVoice);

            cfg.SetValue("language", "ui_language",       Current.UiLanguage);
            cfg.SetValue("language", "voice_language",    Current.VoiceLanguage);
            cfg.SetValue("language", "subtitle_language", Current.SubtitleLanguage);

            foreach (var kv in Current.KeyBindings)
                cfg.SetValue("controls", "key_" + kv.Key, kv.Value);
            cfg.SetValue("controls", "mouse_sensitivity",      Current.MouseSensitivity);
            cfg.SetValue("controls", "invert_y",               Current.InvertY);
            cfg.SetValue("controls", "controller_sensitivity", Current.ControllerSensitivity);

            cfg.Save(SavePath);
        }

        public void Load()
        {
            var cfg = new ConfigFile();
            if (cfg.Load(SavePath) != Error.Ok) { ApplyAll(); return; }

            Current.Resolution = new Vector2I(
                (int)cfg.GetValue("display", "resolution_x", 1600),
                (int)cfg.GetValue("display", "resolution_y", 900));
            Current.Fullscreen  = (bool)cfg.GetValue("display",  "fullscreen", false);
            Current.VSync       = (bool)cfg.GetValue("display",  "vsync",      true);
            Current.FpsLimit    = (int)cfg.GetValue("display",   "fps_limit",  60);
            Current.Fov         = (float)cfg.GetValue("display", "fov",        73.15f);

            Current.Brightness  = (float)cfg.GetValue("calibration", "brightness", 0.5f);
            Current.Contrast    = (float)cfg.GetValue("calibration", "contrast",   1.0f);
            Current.Gamma       = (float)cfg.GetValue("calibration", "gamma",      2.2f);

            Current.MasterVolume   = (float)cfg.GetValue("audio", "master_volume",   0.8f);
            Current.MusicVolume    = (float)cfg.GetValue("audio", "music_volume",    0.7f);
            Current.SfxVolume      = (float)cfg.GetValue("audio", "sfx_volume",      0.9f);
            Current.DialogueVolume = (float)cfg.GetValue("audio", "dialogue_volume", 0.85f);
            Current.AmbientVolume  = (float)cfg.GetValue("audio", "ambient_volume",  0.75f);
            Current.MenuVoice      = (bool)cfg.GetValue("audio",  "menu_voice",      true);

            Current.UiLanguage       = (string)cfg.GetValue("language", "ui_language",       "ru");
            Current.VoiceLanguage    = (string)cfg.GetValue("language", "voice_language",    "ru");
            Current.SubtitleLanguage = (string)cfg.GetValue("language", "subtitle_language", "ru");

            Current.MouseSensitivity      = (float)cfg.GetValue("controls", "mouse_sensitivity",      0.5f);
            Current.InvertY               = (bool)cfg.GetValue("controls",  "invert_y",               false);
            Current.ControllerSensitivity = (float)cfg.GetValue("controls", "controller_sensitivity", 0.5f);

            foreach (var action in Current.KeyBindings.Keys)
            {
                var key = (string)cfg.GetValue("controls", "key_" + action, Current.KeyBindings[action]);
                Current.KeyBindings[action] = key;
            }

            ApplyAll();
        }

        public void ApplyAll()
        {
            ApplyDisplay();
            ApplyAudio();
            ApplyLanguage();
        }

        public void ApplyDisplay()
        {
            DisplayServer.WindowSetSize(Current.Resolution);
            DisplayServer.WindowSetMode(Current.Fullscreen
                ? DisplayServer.WindowMode.Fullscreen
                : DisplayServer.WindowMode.Windowed);
            DisplayServer.WindowSetVsyncMode(Current.VSync
                ? DisplayServer.VSyncMode.Enabled
                : DisplayServer.VSyncMode.Disabled);
            Engine.MaxFps = Current.FpsLimit;
        }

        public void ApplyAudio()
        {
            SetBus("Master",   Current.MasterVolume);
            SetBus("Music",    Current.MusicVolume);
            SetBus("SFX",      Current.SfxVolume);
            SetBus("Dialogue", Current.DialogueVolume);
            SetBus("Ambient",  Current.AmbientVolume);
        }

        private static void SetBus(string name, float linear)
        {
            int idx = AudioServer.GetBusIndex(name);
            if (idx >= 0)
                AudioServer.SetBusVolumeDb(idx, Mathf.LinearToDb(linear));
        }

        public void ApplyLanguage()
        {
            TranslationServer.SetLocale(Current.UiLanguage);
        }
    }
}
```

### MainMenu

```csharp
// res://scenes/main_menu/MainMenu.cs
using Godot;

namespace BloodDragon
{
    public partial class MainMenu : Control
    {
        [Export] private Label             _titleLabel;   // "BLOOD DRAGON 1.0"
        [Export] private Label             _cursorLabel;  // "█"
        [Export] private Timer             _cursorTimer;
        [Export] private Button            _btnCampaign;
        [Export] private Button            _btnSettings;
        [Export] private Button            _btnQuit;
        [Export] private AudioStreamPlayer _hoverSound;
        [Export] private AudioStreamPlayer _selectSound;

        public override void _Ready()
        {
            _cursorTimer.Timeout += () => _cursorLabel.Visible = !_cursorLabel.Visible;

            _btnCampaign.Pressed += OnCampaignPressed;
            _btnSettings.Pressed += OnSettingsPressed;
            _btnQuit.Pressed     += OnQuitPressed;

            foreach (Button btn in new[] { _btnCampaign, _btnSettings, _btnQuit })
                btn.MouseEntered += () => _hoverSound?.Play();

            AnimateFadeIn();
        }

        private void AnimateFadeIn()
        {
            Modulate = new Color(1, 1, 1, 0);
            CreateTween().TweenProperty(this, "modulate:a", 1.0f, 0.3f);
        }

        private void OnCampaignPressed()
        {
            _selectSound?.Play();
            GetTree().ChangeSceneToFile("res://scenes/game/Game.tscn");
        }

        private void OnSettingsPressed()
        {
            _selectSound?.Play();
            GetTree().ChangeSceneToFile("res://scenes/settings_menu/SettingsMenu.tscn");
        }

        private void OnQuitPressed()
        {
            _selectSound?.Play();
            GetTree().Quit();
        }
    }
}
```

### SettingsMenu

```csharp
// res://scenes/settings_menu/SettingsMenu.cs
using Godot;

namespace BloodDragon
{
    public partial class SettingsMenu : Control
    {
        // Левая колонка — кнопки категорий (порядок как на скриншоте)
        [Export] private Button _btnDisplay;        // ИЗОБРАЖЕНИЕ
        [Export] private Button _btnCalibration;    // КАЛИБРОВКА
        [Export] private Button _btnVideoQuality;   // КАЧЕСТВО ВИДЕО
        [Export] private Button _btnControls;       // ЭЛЕМЕНТЫ УПРАВЛЕНИЯ
        [Export] private Button _btnGameplay;       // ИГРОВОЙ ПРОЦЕСС
        [Export] private Button _btnAudio;          // ЗВУК
        [Export] private Button _btnManual;         // РУКОВОДСТВО
        [Export] private Button _btnCredits;        // ТИТРЫ

        // Кнопки внизу экрана
        [Export] private Button _btnAccept;         // ПРИНЯТЬ
        [Export] private Button _btnBack;           // НАЗАД

        // Правая колонка — панели (PanelContainer с зелёной рамкой)
        [Export] private Control _panelDisplay;
        [Export] private Control _panelCalibration;
        [Export] private Control _panelVideoQuality;
        [Export] private Control _panelControls;
        [Export] private Control _panelGameplay;
        [Export] private Control _panelAudio;
        [Export] private Control _panelManual;
        [Export] private Control _panelCredits;

        private Control _activePanel;
        private GameSettings _pendingSettings;  // Копия до нажатия ПРИНЯТЬ

        public override void _Ready()
        {
            // Клонируем настройки, изменения применяем только по ПРИНЯТЬ
            _pendingSettings = SettingsManager.Instance.Current.Clone();

            _btnDisplay.Pressed      += () => SwitchPanel(_panelDisplay);
            _btnCalibration.Pressed  += () => SwitchPanel(_panelCalibration);
            _btnVideoQuality.Pressed += () => SwitchPanel(_panelVideoQuality);
            _btnControls.Pressed     += () => SwitchPanel(_panelControls);
            _btnGameplay.Pressed     += () => SwitchPanel(_panelGameplay);
            _btnAudio.Pressed        += () => SwitchPanel(_panelAudio);
            _btnManual.Pressed       += () => SwitchPanel(_panelManual);
            _btnCredits.Pressed      += () => SwitchPanel(_panelCredits);
            _btnAccept.Pressed       += OnAcceptPressed;
            _btnBack.Pressed         += OnBackPressed;

            SwitchPanel(_panelDisplay);
        }

        private void SwitchPanel(Control target)
        {
            _activePanel?.Hide();
            _activePanel = target;
            _activePanel.Show();
        }

        private void OnAcceptPressed()
        {
            // Применить pending настройки и сохранить
            SettingsManager.Instance.ApplyFromPending(_pendingSettings);
            SettingsManager.Instance.Save();
            GetTree().ChangeSceneToFile("res://scenes/main_menu/MainMenu.tscn");
        }

        private void OnBackPressed()
        {
            // Вернуться без применения
            GetTree().ChangeSceneToFile("res://scenes/main_menu/MainMenu.tscn");
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
                OnBackPressed();
        }
    }
}
```

### Базовый класс панели настроек

```csharp
// res://scenes/settings_menu/categories/BaseSettingsPanel.cs
using Godot;

namespace BloodDragon
{
    public abstract partial class BaseSettingsPanel : Control
    {
        protected GameSettings Settings => SettingsManager.Instance.Current;

        // Вызывать после каждого изменения значения
        protected void OnSettingChanged()
        {
            SettingsManager.Instance.Save();
            SettingsManager.Instance.ApplyAll();
        }

        public abstract void LoadValues();
    }
}
```

### Пример: DisplaySettingsPanel

```csharp
// res://scenes/settings_menu/categories/DisplaySettingsPanel.cs
using Godot;

namespace BloodDragon
{
    public partial class DisplaySettingsPanel : BaseSettingsPanel
    {
        [Export] private OptionButton _resolutionOption;
        [Export] private CheckButton  _fullscreenToggle;
        [Export] private CheckButton  _vsyncToggle;
        [Export] private SpinBox      _fpsLimitSpinBox;
        [Export] private HSlider      _fovSlider;
        [Export] private Label        _fovValueLabel;

        private static readonly Vector2I[] Resolutions =
        {
            new(1280, 720), new(1600, 900), new(1920, 1080),
            new(2560, 1440), new(3840, 2160)
        };

        public override void _Ready()
        {
            foreach (var r in Resolutions)
                _resolutionOption.AddItem($"{r.X} x {r.Y}");

            _resolutionOption.ItemSelected += idx =>
                { Settings.Resolution = Resolutions[idx]; OnSettingChanged(); };

            _fullscreenToggle.Toggled += val =>
                { Settings.Fullscreen = val; OnSettingChanged(); };

            _vsyncToggle.Toggled += val =>
                { Settings.VSync = val; OnSettingChanged(); };

            _fpsLimitSpinBox.ValueChanged += val =>
                { Settings.FpsLimit = (int)val; OnSettingChanged(); };

            _fovSlider.ValueChanged += val =>
            {
                Settings.Fov = (float)val;
                _fovValueLabel.Text = $"{val:F0}";
                OnSettingChanged();
            };

            LoadValues();
        }

        public override void LoadValues()
        {
            for (int i = 0; i < Resolutions.Length; i++)
                if (Resolutions[i] == Settings.Resolution)
                    _resolutionOption.Select(i);

            _fullscreenToggle.ButtonPressed = Settings.Fullscreen;
            _vsyncToggle.ButtonPressed      = Settings.VSync;
            _fpsLimitSpinBox.Value          = Settings.FpsLimit;
            _fovSlider.Value                = Settings.Fov;
            _fovValueLabel.Text             = $"{Settings.Fov:F0}";
        }
    }
}
```

### CRT Shader (scanlines + виньетка + шум)

```glsl
// res://shaders/crt_effect.gdshader
shader_type canvas_item;

uniform float scanline_count     : hint_range(100.0, 1200.0) = 600.0;
uniform float scanline_intensity : hint_range(0.0, 1.0)      = 0.15;
uniform float vignette_radius    : hint_range(0.0, 1.0)      = 0.7;
uniform float vignette_softness  : hint_range(0.0, 1.0)      = 0.4;
uniform float noise_intensity    : hint_range(0.0, 0.2)      = 0.05;

float rand(vec2 co) {
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

void fragment() {
    vec4 color = texture(TEXTURE, UV);

    // Scanlines
    float line = sin(UV.y * scanline_count * PI) * 0.5 + 0.5;
    color.rgb -= line * scanline_intensity;

    // Виньетка
    vec2 centered = UV - 0.5;
    float vignette = smoothstep(vignette_radius,
                                vignette_radius - vignette_softness,
                                length(centered));
    color.rgb *= vignette;

    // Шум
    color.rgb += (rand(UV + TIME * 0.1) - 0.5) * noise_intensity;

    COLOR = color;
}
```

---

## 📱 Адаптивность

**Поддерживаемые разрешения:**

| Разрешение | Формат | Соотношение |
|------------|--------|-------------|
| 1280×720 | HD | 16:9 |
| 1600×900 | HD+ | 16:9 |
| 1920×1080 | Full HD | 16:9 |
| 2560×1440 | 2K | 16:9 |
| 3840×2160 | 4K | 16:9 |
| 2560×1080 | UW-FHD | 21:9 |
| 3440×1440 | UW-QHD | 21:9 |
| 1920×1200 | WUXGA | 16:10 |

**Настройки Godot Project Settings:**
- `display/window/stretch/mode` = `canvas_items`
- `display/window/stretch/aspect` = `expand`
- Базовый размер: 1920×1080

---

## 📁 Структура проекта

```
project.godot
BloodDragon.csproj

scripts/
├── enums/
│   └── GameEnums.cs                      # QualityLevel, DrawDistance, AudioOutput, ...
├── settings/
│   ├── GameSettings.cs                   # Модель данных (все настройки)
│   └── SettingsManager.cs                # Autoload singleton

scenes/
├── main_menu/
│   ├── MainMenu.tscn
│   └── MainMenu.cs
│
├── settings_menu/
│   ├── SettingsMenu.tscn
│   ├── SettingsMenu.cs
│   └── categories/
│       ├── BaseSettingsPanel.cs
│       ├── DisplaySettingsPanel.tscn
│       ├── DisplaySettingsPanel.cs
│       ├── CalibrationSettingsPanel.tscn
│       ├── CalibrationSettingsPanel.cs
│       ├── VideoQualitySettingsPanel.tscn
│       ├── VideoQualitySettingsPanel.cs
│       ├── InputSettingsPanel.tscn
│       ├── InputSettingsPanel.cs
│       ├── GameplaySettingsPanel.tscn    # ИГРОВОЙ ПРОЦЕСС (новая)
│       ├── GameplaySettingsPanel.cs
│       ├── AudioSettingsPanel.tscn
│       ├── AudioSettingsPanel.cs
│       ├── ManualPanel.tscn              # РУКОВОДСТВО (новая)
│       ├── ManualPanel.cs
│       ├── CreditsSettingsPanel.tscn
│       └── CreditsSettingsPanel.cs
│
└── ui_components/
    ├── MenuButton.tscn                   # Кнопка категории: зелёная заливка при hover/focus
    ├── CycleOption.tscn                  # Строка: LABEL  ◄ ЗНАЧЕНИЕ ► (кастомный контрол)
    ├── CycleOption.cs
    ├── SliderRow.tscn                    # Строка: LABEL : {value}  [====slider====]
    ├── SliderRow.cs
    └── KeyBindingRow.tscn                # Строка переназначения клавиши
    └── KeyBindingRow.cs

shaders/
├── crt_effect.gdshader                   # Scanlines + виньетка + шум
└── glow_button.gdshader                  # Свечение активного элемента

audio/
├── sfx/
│   ├── menu_hover.wav
│   ├── menu_select.wav
│   └── menu_tick.wav
└── music/
    └── menu_ambient.ogg

fonts/
└── monospace_pixel.ttf
```
