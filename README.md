# NoteVision - Plan for Android App (MAUI)

## 🔹 App Identity
**Name:** NoteVision  
**Concept:** A real-time music learning app for reading notes on the staff.  
**Purpose:** Helps musicians and learners learn where notes are located on the musical staff by instantly showing the note they play or sing.  
**Audience:** Music students, hobbyists, and anyone learning to read sheet music.  
**Unique Value:** Displays live notes on both treble and bass clefs, bridging sound and notation seamlessly.

---

## 🔹 Goal
The main goal of NoteVision is to **teach where notes are located on the musical staff**.  
An Android app built with .NET MAUI listens to the note currently being played or sung and displays it on the staff in real time, together with its name (e.g. `E4`).  
The screen shows two staves: the **treble clef** on top and the **bass clef** below.

---

## 🔹 Implementation Steps

1. **Audio Capture**
   - Use Android `AudioRecord` (via MAUI platform code) to read raw PCM samples in real time.
   - Request microphone permission (`RECORD_AUDIO`) at runtime.
   - Typical settings: 44.1 kHz, mono, 16-bit, buffer of ~2048 samples.

2. **Pitch Detection**
   - Single-note (monophonic) detection only.
   - Algorithm: **YIN** (custom C# implementation, no external libraries required).
   - Detection range: ~60 Hz – 1.2 kHz (covers voice and common instruments).
   - Ignore silence / low-confidence frames.
   - Output: frequency in Hz (e.g., 440 Hz → A4).

3. **Frequency-to-Note Mapping**
   - Formula (MIDI note number):

     \[
     n = 12 \cdot \log_2\left(\frac{f}{440}\right) + 69
     \]

     where \(f\) = frequency, 69 = MIDI number of A4.
   - Round \(n\) to the nearest integer to get the note; the fractional part gives the deviation in cents.
   - Convert to note name (C, C♯, D…) and octave (MIDI 60 = C4, middle C).

4. **Smoothing**
   - Voice and some instruments are unstable (vibrato, drift).
   - Apply a median filter over the last few detections.
   - Require a note to be stable for a short time (~50–100 ms) before displaying it, to avoid flicker.

5. **Visualization on Staff**
   - UI: two staves (treble and bass clefs) drawn with `GraphicsView`.
   - Notes from **C4 (middle C) and above** are shown on the treble staff, notes below on the bass staff.
   - Draw ledger lines for notes outside the staff and accidentals (♯) where needed.
   - Show the note name next to the staff so the learner connects sound, position and name.
   - In runtime mode, display only the note currently being played.

6. **Performance Optimization**
   - Run audio capture and analysis on a dedicated background thread / `Task`.
   - Update the UI on the main thread only when the displayed note changes.
   - Minimize latency between sound input and visual output; test on several real devices.

---

## 🔹 Minimum Viable Product (MVP)
- App listens to microphone input.  
- Detects the pitch of a single note.  
- Displays the note on the correct staff (treble/bass clef) with its name.  

---

## 🔹 Next Steps
- Add **note history** (sequence of recent notes on the staff).  
- Add **learning mode** — the app shows a target note on the staff and the user plays or sings it; correct notes are highlighted.  
- Add **tuning indicator** — show how many cents the pitch is sharp or flat.  
- Option to switch between **sharps and flats** notation.  

---

## 🔹 Out of Scope (for now)
- **Polyphony** (detecting multiple notes at once) — not planned; the app focuses on single notes for learning note positions.
