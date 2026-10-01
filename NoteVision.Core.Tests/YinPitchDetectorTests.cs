namespace NoteVision.Core.Tests;

public class YinPitchDetectorTests
{
    private const int SampleRate = 44100;
    private const int BlockSize = 2048; // about 46 ms of audio
    private const double ToleranceCents = 5;

    private readonly YinPitchDetector _detector = new(SampleRate);

    [Theory]
    [InlineData(61.74)]    // B1, just above the 60 Hz limit
    [InlineData(82.41)]    // E2, low guitar string
    [InlineData(110.00)]   // A2
    [InlineData(196.00)]   // G3
    [InlineData(261.63)]   // C4, middle C
    [InlineData(440.00)]   // A4
    [InlineData(443.70)]   // between notes: A4 + ~14.5 cents
    [InlineData(659.25)]   // E5
    [InlineData(880.00)]   // A5
    [InlineData(1046.50)]  // C6
    [InlineData(1174.66)]  // D6, just below the 1.2 kHz limit
    public void Sine_wave_is_detected_within_5_cents(double frequency)
    {
        var samples = TestSignals.Sine(frequency, SampleRate, BlockSize);

        var result = _detector.Detect(samples);

        AssertPitch(frequency, result);
    }

    [Theory]
    [InlineData(65.41)]    // C2
    [InlineData(82.41)]
    [InlineData(146.83)]   // D3
    [InlineData(261.63)]
    [InlineData(440.00)]
    [InlineData(783.99)]   // G5
    [InlineData(1174.66)]
    public void Harmonic_rich_wave_is_detected_at_its_fundamental(double frequency)
    {
        // The overtones line up every half, third, … period; YIN must still pick the whole period.
        var samples = TestSignals.Harmonic(frequency, SampleRate, BlockSize);

        var result = _detector.Detect(samples);

        AssertPitch(frequency, result);
    }

    [Theory]
    [InlineData(0.3)]
    [InlineData(1.7)]
    [InlineData(4.0)]
    public void Starting_phase_does_not_matter(double phase)
    {
        var samples = TestSignals.Sine(440, SampleRate, BlockSize, phase: phase);

        AssertPitch(440, _detector.Detect(samples));
    }

    [Fact]
    public void Quiet_tone_is_still_detected()
    {
        var samples = TestSignals.Sine(440, SampleRate, BlockSize, amplitude: 0.01); // about -43 dB

        AssertPitch(440, _detector.Detect(samples));
    }

    [Fact]
    public void Other_sample_rate_works()
    {
        var detector = new YinPitchDetector(48000);
        var samples = TestSignals.Sine(329.63, 48000, BlockSize); // E4

        AssertPitch(329.63, detector.Detect(samples));
    }

    [Fact]
    public void Detected_frequency_maps_to_the_right_note()
    {
        var samples = TestSignals.Harmonic(261.63, SampleRate, BlockSize);

        var note = NoteMapper.FromFrequency(_detector.Detect(samples).Frequency);

        Assert.Equal("C4", note.ToString());
    }

    [Fact]
    public void Silence_has_no_pitch()
    {
        var result = _detector.Detect(new float[BlockSize]);

        Assert.False(result.HasPitch);
        Assert.Equal(PitchResult.None, result);
    }

    [Fact]
    public void Tone_below_the_silence_level_has_no_pitch()
    {
        var samples = TestSignals.Sine(440, SampleRate, BlockSize, amplitude: 0.0005);

        Assert.False(_detector.Detect(samples).HasPitch);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(2024)]
    public void White_noise_has_no_pitch(int seed)
    {
        var samples = TestSignals.WhiteNoise(BlockSize, seed: seed);

        Assert.False(_detector.Detect(samples).HasPitch);
    }

    [Theory]
    [InlineData(40)]    // below 60 Hz: its period does not fit in the lags searched
    [InlineData(2000)]  // above 1.2 kHz: must not be reported as 1 kHz (an octave lower)
    public void Tone_outside_the_range_has_no_pitch(double frequency)
    {
        var samples = TestSignals.Sine(frequency, SampleRate, BlockSize);

        Assert.False(_detector.Detect(samples).HasPitch);
    }

    [Fact]
    public void Too_few_samples_is_rejected()
    {
        var samples = new float[_detector.MinimumSampleCount - 1];

        Assert.Throws<ArgumentException>(() => _detector.Detect(samples));
    }

    [Fact]
    public void Minimum_sample_count_holds_two_periods_of_the_lowest_frequency()
    {
        // 44100 / 60 = 735 samples per period, plus one for interpolation, times two.
        Assert.Equal(1472, _detector.MinimumSampleCount);
    }

    [Theory]
    [InlineData(0, 60, 1200, 0.15)]       // no sample rate
    [InlineData(44100, 0, 1200, 0.15)]    // no minimum frequency
    [InlineData(44100, 500, 400, 0.15)]   // minimum above maximum
    [InlineData(44100, 60, 30000, 0.15)]  // maximum above Nyquist (22050 Hz)
    [InlineData(44100, 60, 1200, 0)]      // threshold too low
    [InlineData(44100, 60, 1200, 1)]      // threshold too high
    public void Invalid_settings_are_rejected(int sampleRate, double minFrequency, double maxFrequency, double threshold)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new YinPitchDetector(sampleRate, minFrequency, maxFrequency, threshold));
    }

    private static void AssertPitch(double expectedFrequency, PitchResult result)
    {
        Assert.True(result.HasPitch, $"No pitch detected for {expectedFrequency} Hz.");
        double errorCents = 1200 * Math.Log2(result.Frequency / expectedFrequency);
        Assert.True(
            Math.Abs(errorCents) <= ToleranceCents,
            $"Expected {expectedFrequency} Hz, detected {result.Frequency:F3} Hz ({errorCents:+0.00;-0.00} cents).");
        Assert.InRange(result.Confidence, 0.8, 1);
    }
}
