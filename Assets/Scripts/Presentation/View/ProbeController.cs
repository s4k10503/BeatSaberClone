using UnityEngine;

namespace BeatSaberClone.Presentation
{
    public class ProbeController : MonoBehaviour
    {
        ReflectionProbe probe;

        void Start()
        {
            this.probe = GetComponent<ReflectionProbe>();
        }

        void Update()
        {
            this.probe.transform.position = new Vector3(
                Camera.main.transform.position.x,
                Camera.main.transform.position.y * -1,
                Camera.main.transform.position.z
            );

            probe.RenderProbe();
        }
    }
}
