using UnityEngine;

namespace BeatSaberClone.Domain
{
    [CreateAssetMenu(fileName = "BoxMoveSpeed", menuName = "BeatSaberClone/BoxMoveSpeed")]
    public class BoxMoveSpeed : ScriptableObject
    {
        [SerializeField][Range(0f, 20f)] private float _speed;
        public float Speed { get => _speed; }
    }
}