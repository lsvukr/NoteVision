namespace NoteVision.Core;

/// <summary>
/// The outcome of one pitch detection: the frequency that was heard and how sure the detector is,
/// or <see cref="None"/> when there is no clear pitch (silence, noise, or out of range).
/// </summary>
/// <param name="Frequency">Detected fundamental frequency in hertz; 0 when there is no pitch.</param>
/// <param name="Confidence">From 0 (unsure) to 1 (perfectly periodic sound).</param>
public readonly record struct PitchResult(double Frequency, double Confidence)
{
    /// <summary>No pitch was found.</summary>
    public static PitchResult None => default;

    /// <summary>True when a pitch was found.</summary>
    public bool HasPitch => Frequency > 0;
}
