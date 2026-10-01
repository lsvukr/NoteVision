namespace NoteVision.Core;

/// <summary>
/// Converts between frequencies in hertz and notes, using equal temperament with A4 = 440 Hz.
/// </summary>
public static class NoteMapper
{
    public const double A4Frequency = 440.0;
    public const int A4MidiNumber = 69;

    /// <summary>
    /// Finds the note nearest to <paramref name="frequency"/> and how many cents the frequency is off from it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The frequency is not a positive number, or is outside the MIDI range (about 8 Hz to 12.5 kHz).
    /// </exception>
    public static Note FromFrequency(double frequency)
    {
        if (!double.IsFinite(frequency) || frequency <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frequency), frequency, "Frequency must be a positive number of hertz.");
        }

        // n = 12 · log2(f / 440) + 69 — every octave doubles the frequency and has 12 semitones.
        double exactMidiNumber = 12 * Math.Log2(frequency / A4Frequency) + A4MidiNumber;
        int midiNumber = (int)Math.Round(exactMidiNumber, MidpointRounding.AwayFromZero);
        double cents = (exactMidiNumber - midiNumber) * 100;

        return new Note(midiNumber, cents);
    }

    /// <summary>The exact frequency in hertz of the note with the given MIDI number.</summary>
    public static double ToFrequency(int midiNumber) =>
        A4Frequency * Math.Pow(2, (midiNumber - A4MidiNumber) / 12.0);
}
