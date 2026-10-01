namespace NoteVision.Core;

/// <summary>
/// A musical note: the nearest equal-tempered pitch to a sound, plus how far the sound is from it.
/// </summary>
public readonly record struct Note
{
    public const int MinMidiNumber = 0;
    public const int MaxMidiNumber = 127;

    private static readonly string[] Names = ["C", "C♯", "D", "D♯", "E", "F", "F♯", "G", "G♯", "A", "A♯", "B"];

    public Note(int midiNumber, double cents = 0)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(midiNumber, MinMidiNumber);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(midiNumber, MaxMidiNumber);

        MidiNumber = midiNumber;
        Cents = cents;
    }

    /// <summary>MIDI note number: 60 = C4 (middle C), 69 = A4.</summary>
    public int MidiNumber { get; }

    /// <summary>Deviation from the exact pitch in cents (1/100 of a semitone), between -50 and +50.</summary>
    public double Cents { get; }

    /// <summary>Note name without octave, e.g. "C♯".</summary>
    public string Name => Names[MidiNumber % 12];

    /// <summary>Octave in scientific pitch notation, where C4 is middle C.</summary>
    public int Octave => MidiNumber / 12 - 1;

    /// <summary>Note name with octave, e.g. "A4".</summary>
    public override string ToString() => $"{Name}{Octave}";
}
