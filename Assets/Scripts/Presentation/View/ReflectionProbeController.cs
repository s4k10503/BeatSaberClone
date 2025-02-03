using UnityEngine;

namespace BeatSaberClone.Presentation
{
    public sealed class ReflectionProbeController : MonoBehaviour
    {
        ReflectionProbe _probe;

        void Awake()
        {
            _probe = GetComponent<ReflectionProbe>();
        }

        public void UpdatePosition()
        {
            _probe.transform.position = new Vector3(
                Camera.main.transform.position.x,
                Camera.main.transform.position.y * -1,
                Camera.main.transform.position.z
            );

            _probe.RenderProbe();
        }
    }
}
