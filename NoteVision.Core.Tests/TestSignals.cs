namespace NoteVision.Core.Tests;

/// <summary>Generated audio for tests, as mono samples from -1 to 1.</summary>
internal static class TestSignals
{
    public static float[] Sine(double frequency, int sampleRate, int count, double amplitude = 0.5, double phase = 0)
    {
        var samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            samples[i] = (float)(amplitude * Math.Sin(2 * Math.PI * frequency * i / sampleRate + phase));
        }

        return samples;
    }

    /// <summary>
    /// A fundamental plus overtones at 2×, 3×, … its frequency, each quieter than the last (1/k),
    /// like a plucked string or a voice. Scaled so the peak is <paramref name="amplitude"/>.
    /// </summary>
    public static float[] Harmonic(double frequency, int sampleRate, int count, int harmonics = 6, double amplitude = 0.5)
    {
        var raw = new double[count];
        for (int k = 1; k <= harmonics; k++)
        {
            for (int i = 0; i < count; i++)
            {
                raw[i] += Math.Sin(2 * Math.PI * k * frequency * i / sampleRate) / k;
            }
        }

        double peak = raw.Max(Math.Abs);
        return raw.Select(value => (float)(amplitude * value / peak)).ToArray();
    }

    /// <summary>Random samples with no pitch. The fixed seed makes every test run identical.</summary>
    public static float[] WhiteNoise(int count, double amplitude = 0.5, int seed = 1234)
    {
        var random = new Random(seed);
        var samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            samples[i] = (float)(amplitude * (2 * random.NextDouble() - 1));
        }

        return samples;
    }
}
