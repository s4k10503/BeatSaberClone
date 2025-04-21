namespace BeatSaberClone.Domain
{
    public sealed class AudioAsset
    {
        public string Id { get; }
        public float Length { get; }
        public object NativeAsset { get; }

        public AudioAsset(string id, float length, object nativeAsset)
        {
            Id = id;
            Length = length;
            NativeAsset = nativeAsset;
        }
    }
}
