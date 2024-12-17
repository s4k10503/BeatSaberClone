namespace BeatSaberClone.Presentation
{
    public interface IAudioVisualEffecter
    {
        void Initialize();
        void UpdateEffect(float average, float[] spectrumData);
    }
}