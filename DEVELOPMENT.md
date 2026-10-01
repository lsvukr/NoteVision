# NoteVision - Development Steps

Step-by-step plan for building the MVP described in [README.md](README.md).  
Each step is one feature branch and one pull request into `main` (squash merge).  
Mark a step `[x]` when its PR is merged.

---

## 🔹 Workflow
1. Create a branch from `main`: `feature/<step-name>`.
2. Implement the step and its tests.
3. Open a PR; the PR title becomes the commit title on `main`.
4. Squash merge — the branch is deleted automatically.

---

## 🔹 Steps

### Step 0 — Project setup ✅
- [x] .NET 10 MAUI project, Android only, app ID `com.lsvukr.notevision`
- [x] `RECORD_AUDIO` permission in the Android manifest
- [x] Git repository, GitHub `lsvukr/NoteVision`, `main` protected, squash-only merges

### Step 1 — Core library and note mapping ✅
Branch: `feature/note-mapping`
- [x] Add class library `NoteVision.Core` (plain .NET, no MAUI) for all music/audio logic.
- [x] Add test project `NoteVision.Core.Tests` (xUnit).
- [x] `Note` model: MIDI number, name (C, C♯, D…), octave, cents deviation.
- [x] Frequency → note: `n = 12 · log2(f / 440) + 69`.
- [x] Tests: 440 Hz → A4, 261.63 Hz → C4, 82.41 Hz → E2, cents for off-pitch input.

**Done when:** tests pass; the MAUI app references `NoteVision.Core`.

### Step 2 — YIN pitch detection ✅
Branch: `feature/pitch-detection`
- [x] `IPitchDetector` interface and `YinPitchDetector` implementation.
- [x] Range ~60 Hz – 1.2 kHz; returns frequency + confidence, or "no pitch" for silence/noise.
- [x] Tests with generated sine and harmonic-rich waves at several frequencies; silence returns no pitch.

**Done when:** detected frequency is within ±5 cents of the generated tone in tests.

### Step 3 — Note smoothing
Branch: `feature/note-smoothing`
- [ ] `NoteStabilizer`: median filter over recent detections.
- [ ] A note is shown only after being stable ~50–100 ms; short gaps don't clear it immediately.
- [ ] Tests: vibrato around one note stays on that note; a real note change switches.

**Done when:** no flicker in tests with simulated vibrato and noise.

### Step 4 — Android audio capture
Branch: `feature/audio-capture`
- [ ] `IAudioSource` interface in Core; Android implementation with `AudioRecord` (44.1 kHz, mono, 16-bit).
- [ ] Runtime microphone permission request; clear message if denied.
- [ ] Capture runs on a background thread; start/stop without leaks.

**Done when:** on a real phone, audio buffers arrive continuously (verified via debug output).

### Step 5 — Live note name on screen (first end-to-end test)
Branch: `feature/live-note-text`
- [ ] Pipeline: audio capture → YIN → stabilizer → view model.
- [ ] Main page shows the current note name (e.g. `E4`) and frequency as text; Start/Stop button.
- [ ] UI updated on the main thread only when the note changes.

**Done when:** playing or singing a note on a phone shows the correct name with low delay.

### Step 6 — Staff drawing
Branch: `feature/staff-drawing`
- [ ] `GraphicsView` drawable with two staves: treble on top, bass below (grand staff).
- [ ] Draw treble and bass clefs.
- [ ] Draw a note head at a given position (static test note).
- [ ] Scales correctly for different screen sizes and orientations.

**Done when:** a fixed note (e.g. C4, G4, F3) is drawn in the correct place.

### Step 7 — Live note on the staff
Branch: `feature/live-note-staff`
- [ ] Map note → staff position; C4 and above on treble, below C4 on bass.
- [ ] Ledger lines for notes above/below a staff; ♯ accidental where needed.
- [ ] Note name shown next to the staff.
- [ ] Replace the text-only view from Step 5 with the staff view.

**Done when:** notes across the range (~E2 to C6) appear in the correct staff position on a phone.

### Step 8 — MVP polish
Branch: `feature/mvp-polish`
- [ ] Layout, colors, app icon and splash screen for NoteVision.
- [ ] Handle permission denied, app going to background, and resume.
- [ ] Test latency and accuracy on at least 2–3 real devices; tune buffer size.

**Done when:** the app is usable end to end without crashes.

### Step 9 — MVP release
Branch: `feature/release-setup`
- [ ] Release build settings and signing key (key stored outside the repository).
- [ ] Build `.aab`; upload to Google Play **internal testing**.
- [ ] Store listing: description, screenshots, privacy policy (microphone use, no data collected).

**Done when:** the app installs from Google Play internal testing.

---

## 🔹 After MVP (from README Next Steps)
- [ ] **Note history** — recent notes shown in sequence on the staff.
- [ ] **Tuning indicator** — cents sharp/flat.
- [ ] **Sharps / flats** notation switch.
- [ ] **Learning mode** — app shows a target note, user plays or sings it, correct notes highlighted.

## 🔹 Out of Scope
- Polyphony (multiple notes at once).
