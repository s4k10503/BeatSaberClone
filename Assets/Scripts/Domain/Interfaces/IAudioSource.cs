namespace BeatSaberClone.Domain
{
    public interface IAudioSource
    {
        float Volume { get; set; }
        bool IsPlaying { get; }
        int TimeSamples { get; }
        int Samples { get; }
        void SetClip(AudioAsset asset);
        void Play();
        void PlayOneShot(AudioAsset asset);
        void Pause();
        void UnPause();
        void GetSpectrumData(float[] samples, int channel, FFTWindowType window);
    }
}
