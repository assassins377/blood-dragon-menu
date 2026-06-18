#!/usr/bin/env python3
"""Procedurally generate the menu SFX/music WAV assets (no external deps)."""
import math
import struct
import wave

RATE = 44100


def write_wav(path, samples):
    with wave.open(path, "w") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        frames = bytearray()
        for s in samples:
            v = int(max(-1.0, min(1.0, s)) * 32767)
            frames += struct.pack("<h", v)
        w.writeframes(frames)


def tone(freq, dur, vol=0.5, attack=0.005, decay=0.04, wave_fn=math.sin):
    n = int(RATE * dur)
    out = []
    for i in range(n):
        t = i / RATE
        # simple AD envelope
        if t < attack:
            env = t / attack
        else:
            env = math.exp(-(t - attack) / decay)
        out.append(vol * env * wave_fn(2 * math.pi * freq * t))
    return out


def square(x):
    return 1.0 if math.sin(x) >= 0 else -1.0


def mix(a, b):
    n = max(len(a), len(b))
    out = []
    for i in range(n):
        out.append((a[i] if i < len(a) else 0.0) + (b[i] if i < len(b) else 0.0))
    return out


# Hover: short bright blip.
write_wav("audio/sfx/menu_hover.wav", tone(880, 0.05, 0.35, decay=0.03, wave_fn=square))

# Select: two-tone confirm (rising).
sel = tone(660, 0.06, 0.4, decay=0.05, wave_fn=square)
sel += tone(990, 0.09, 0.4, decay=0.06, wave_fn=square)
write_wav("audio/sfx/menu_select.wav", sel)

# Tick: very short click.
write_wav("audio/sfx/menu_tick.wav", tone(1300, 0.025, 0.3, attack=0.001, decay=0.012, wave_fn=square))

# Ambient: low, slowly detuned drone loop (~6s, designed to loop seamlessly).
DUR = 6.0
n = int(RATE * DUR)
amb = []
for i in range(n):
    t = i / RATE
    # base drone + fifth, with slow tremolo; period-matched to loop length
    base = math.sin(2 * math.pi * 55.0 * t)
    fifth = 0.5 * math.sin(2 * math.pi * 82.4 * t)
    shimmer = 0.18 * math.sin(2 * math.pi * 110.0 * t + math.sin(2 * math.pi * 0.5 * t))
    trem = 0.6 + 0.4 * math.sin(2 * math.pi * (1.0 / DUR) * t)  # full cycles over loop
    amb.append(0.22 * trem * (base + fifth + shimmer))
write_wav("audio/music/menu_ambient.wav", amb)

print("generated:")
for p in ("audio/sfx/menu_hover.wav", "audio/sfx/menu_select.wav",
          "audio/sfx/menu_tick.wav", "audio/music/menu_ambient.wav"):
    print(" ", p)
