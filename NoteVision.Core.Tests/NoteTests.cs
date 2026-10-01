namespace NoteVision.Core.Tests;

public class NoteTests
{
    [Theory]
    [InlineData(60, "C", 4, "C4")]   // middle C
    [InlineData(61, "C♯", 4, "C♯4")]
    [InlineData(69, "A", 4, "A4")]   // concert A
    [InlineData(59, "B", 3, "B3")]   // last note of octave 3, just below middle C
    [InlineData(40, "E", 2, "E2")]   // low E of a guitar
    [InlineData(0, "C", -1, "C-1")]  // lowest MIDI note
    [InlineData(127, "G", 9, "G9")]  // highest MIDI note
    public void Name_octave_and_text_come_from_midi_number(int midiNumber, string name, int octave, string text)
    {
        var note = new Note(midiNumber);

        Assert.Equal(name, note.Name);
        Assert.Equal(octave, note.Octave);
        Assert.Equal(text, note.ToString());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(128)]
    public void Midi_number_outside_0_to_127_is_rejected(int midiNumber)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Note(midiNumber));
    }
}
