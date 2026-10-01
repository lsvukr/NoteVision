namespace NoteVision.Core.Tests;

public class NoteMapperTests
{
    [Theory]
    [InlineData(440.00, 69, "A4")]
    [InlineData(261.63, 60, "C4")]
    [InlineData(82.41, 40, "E2")]
    [InlineData(277.18, 61, "C♯4")]
    [InlineData(246.94, 59, "B3")]
    [InlineData(1046.50, 84, "C6")]
    public void Standard_frequency_maps_to_its_note(double frequency, int midiNumber, string text)
    {
        var note = NoteMapper.FromFrequency(frequency);

        Assert.Equal(midiNumber, note.MidiNumber);
        Assert.Equal(text, note.ToString());
        // The frequencies above are rounded to 0.01 Hz, so they are a tiny fraction of a cent off.
        Assert.InRange(note.Cents, -1, 1);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(-20)]
    [InlineData(45)]
    [InlineData(-45)]
    public void Off_pitch_frequency_keeps_the_note_and_reports_cents(double cents)
    {
        // A cent is 1/1200 of an octave, so shifting by c cents multiplies the frequency by 2^(c/1200).
        double frequency = NoteMapper.A4Frequency * Math.Pow(2, cents / 1200);

        var note = NoteMapper.FromFrequency(frequency);

        Assert.Equal(69, note.MidiNumber);
        Assert.Equal(cents, note.Cents, precision: 6);
    }

    [Fact]
    public void More_than_50_cents_sharp_becomes_the_next_note_flat()
    {
        double frequency = NoteMapper.A4Frequency * Math.Pow(2, 60 / 1200.0);

        var note = NoteMapper.FromFrequency(frequency);

        Assert.Equal("A♯4", note.ToString());
        Assert.Equal(-40, note.Cents, precision: 6);
    }

    [Theory]
    [InlineData(69, 440.0)]
    [InlineData(81, 880.0)]
    [InlineData(57, 220.0)]
    [InlineData(60, 261.6256)]
    public void Midi_number_maps_to_its_frequency(int midiNumber, double frequency)
    {
        Assert.Equal(frequency, NoteMapper.ToFrequency(midiNumber), precision: 4);
    }

    [Fact]
    public void Every_midi_note_survives_a_round_trip()
    {
        for (int midiNumber = Note.MinMidiNumber; midiNumber <= Note.MaxMidiNumber; midiNumber++)
        {
            var note = NoteMapper.FromFrequency(NoteMapper.ToFrequency(midiNumber));

            Assert.Equal(midiNumber, note.MidiNumber);
            Assert.Equal(0, note.Cents, precision: 6);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-440)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(5)]       // below MIDI 0 (C-1, about 8.18 Hz)
    [InlineData(20000)]   // above MIDI 127 (G9, about 12.5 kHz)
    public void Invalid_or_out_of_range_frequency_is_rejected(double frequency)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NoteMapper.FromFrequency(frequency));
    }
}
