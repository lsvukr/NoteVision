namespace NoteVision.Core;

/// <summary>
/// Finds the fundamental frequency of a short block of audio.
/// </summary>
public interface IPitchDetector
{
    /// <summary>Samples per second of the audio passed to <see cref="Detect"/>.</summary>
    int SampleRate { get; }

    /// <summary>The smallest block of samples <see cref="Detect"/> accepts.</summary>
    int MinimumSampleCount { get; }

    /// <summary>
    /// Detects the pitch of <paramref name="samples"/>: mono audio with values from -1 to 1.
    /// </summary>
    /// <exception cref="ArgumentException">Fewer than <see cref="MinimumSampleCount"/> samples.</exception>
    PitchResult Detect(ReadOnlySpan<float> samples);
}
