namespace NoteVision.Core;

/// <summary>
/// Pitch detection with the YIN algorithm (de Cheveigné and Kawahara, 2002).
/// </summary>
/// <remarks>
/// YIN looks for the smallest delay (lag) at which the signal best matches a shifted copy of itself;
/// that lag is one period, and the frequency is the sample rate divided by it.
/// An instance reuses internal buffers, so it is not thread-safe: use one detector per audio stream.
/// </remarks>
public sealed class YinPitchDetector : IPitchDetector
{
    public const double DefaultMinFrequency = 60;
    public const double DefaultMaxFrequency = 1200;

    /// <summary>
    /// Default dip in the normalized difference that counts as a period; lower is stricter.
    /// The YIN paper uses 0.1–0.15.
    /// </summary>
    public const double DefaultThreshold = 0.15;

    /// <summary>Blocks quieter than this RMS level (about -60 dB) are treated as silence.</summary>
    public const double SilenceLevel = 0.001;

    private readonly double _threshold;
    private readonly int _maxLag;
    private readonly double[] _difference;
    private readonly double[] _normalized;

    public YinPitchDetector(
        int sampleRate,
        double minFrequency = DefaultMinFrequency,
        double maxFrequency = DefaultMaxFrequency,
        double threshold = DefaultThreshold)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minFrequency);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxFrequency, minFrequency);
        // Above half the sample rate (the Nyquist frequency) a tone cannot be represented at all.
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(maxFrequency, sampleRate / 2.0);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(threshold);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(threshold, 1);

        SampleRate = sampleRate;
        MinFrequency = minFrequency;
        MaxFrequency = maxFrequency;
        _threshold = threshold;

        // The lowest frequency has the longest period. One extra lag gives the interpolation
        // in step 4 a right-hand neighbour even at that lowest frequency.
        _maxLag = (int)Math.Ceiling(sampleRate / minFrequency) + 1;
        _difference = new double[_maxLag + 1];
        _normalized = new double[_maxLag + 1];
    }

    public int SampleRate { get; }

    public double MinFrequency { get; }

    public double MaxFrequency { get; }

    /// <summary>
    /// Every lag up to the longest period is compared over a window at least that long,
    /// so the block must hold two of the longest periods.
    /// </summary>
    public int MinimumSampleCount => 2 * _maxLag;

    public PitchResult Detect(ReadOnlySpan<float> samples)
    {
        if (samples.Length < MinimumSampleCount)
        {
            throw new ArgumentException(
                $"At least {MinimumSampleCount} samples are needed, got {samples.Length}.", nameof(samples));
        }

        if (RootMeanSquare(samples) < SilenceLevel)
        {
            return PitchResult.None;
        }

        int window = samples.Length - _maxLag;

        // Step 1 — difference function: d(τ) = Σ (x[j] − x[j+τ])².
        // It is near zero when shifting the signal by τ samples lines it up with itself.
        _difference[0] = 0;
        for (int lag = 1; lag <= _maxLag; lag++)
        {
            double sum = 0;
            for (int j = 0; j < window; j++)
            {
                double delta = samples[j] - samples[j + lag];
                sum += delta * delta;
            }

            _difference[lag] = sum;
        }

        // Step 2 — cumulative mean normalized difference: d'(τ) = d(τ) / (mean of d(1..τ)).
        // This makes the values independent of loudness (about 1 for noise, near 0 at a period)
        // and stops tiny lags, where d is always small, from looking like a period.
        _normalized[0] = 1;
        double runningSum = 0;
        for (int lag = 1; lag <= _maxLag; lag++)
        {
            runningSum += _difference[lag];
            _normalized[lag] = runningSum > 0 ? _difference[lag] * lag / runningSum : 1;
        }

        // Step 3 — absolute threshold: take the first dip below the threshold, then slide down
        // to the bottom of that dip. Taking the first (smallest) lag avoids octave errors,
        // because two or three periods line up just as well as one.
        // The search starts at lag 2, not at the shortest allowed period, so that a tone that is
        // too high is reported as out of range instead of being mistaken for a lower octave.
        int bestLag = -1;
        for (int lag = 2; lag < _maxLag; lag++)
        {
            if (_normalized[lag] < _threshold)
            {
                while (lag + 1 < _maxLag && _normalized[lag + 1] < _normalized[lag])
                {
                    lag++;
                }

                bestLag = lag;
                break;
            }
        }

        if (bestLag < 0)
        {
            return PitchResult.None;
        }

        // Step 4 — parabolic interpolation: fit a parabola through the dip and its two neighbours
        // to find the period between whole samples. At 1 kHz a period is only ~44 samples, so
        // whole lags alone would be up to ~20 cents off.
        double period = bestLag + ParabolaVertexOffset(
            _normalized[bestLag - 1], _normalized[bestLag], _normalized[bestLag + 1]);
        double frequency = SampleRate / period;

        if (frequency < MinFrequency || frequency > MaxFrequency)
        {
            return PitchResult.None;
        }

        double confidence = Math.Clamp(1 - _normalized[bestLag], 0, 1);
        return new PitchResult(frequency, confidence);
    }

    private static double RootMeanSquare(ReadOnlySpan<float> samples)
    {
        double sum = 0;
        foreach (float sample in samples)
        {
            sum += sample * sample;
        }

        return Math.Sqrt(sum / samples.Length);
    }

    /// <summary>
    /// Where the lowest point of the parabola through (−1, left), (0, middle), (1, right) lies,
    /// between −0.5 and 0.5.
    /// </summary>
    private static double ParabolaVertexOffset(double left, double middle, double right)
    {
        double curvature = left - 2 * middle + right;
        return curvature > 0 ? 0.5 * (left - right) / curvature : 0;
    }
}
