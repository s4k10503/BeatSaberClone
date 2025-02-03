using UnityEngine;

namespace BeatSaberClone.Presentation
{
    public sealed class ScaleCubesWithAudio : MonoBehaviour
    {
        [SerializeField] private GameObject[] cubes;
        [SerializeField] private float scaleMultiplier = 10.0f;
        [SerializeField] private float lerpSpeed = 1.0f;

        private void OnDestroy()
        {
            cubes = null;
        }


        public void UpdateEffect(float[] spectrumData)
        {
            if (spectrumData != null && spectrumData.Length > 0)
            {
                for (int i = 0; i < cubes.Length && i < spectrumData.Length; i++)
                {
                    float intensity = spectrumData[i] * scaleMultiplier;

                    // Change only the Y-axis scale
                    float newScaleY = Mathf.Lerp(
                        cubes[i].transform.localScale.y,
                        intensity,
                        lerpSpeed * Time.deltaTime);

                    cubes[i].transform.localScale = new Vector3(
                        cubes[i].transform.localScale.x,
                        newScaleY,
                        cubes[i].transform.localScale.z);
                }
            }
        }
    }
}
