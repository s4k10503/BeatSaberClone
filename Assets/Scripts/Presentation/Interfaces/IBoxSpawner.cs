using System;
using System.Threading;
using UnityEngine;
using BeatSaberClone.Domain;
using Cysharp.Threading.Tasks;

namespace BeatSaberClone.Presentation
{
    public interface IBoxSpawner
    {
        Transform PlayerPoint { get; }
        Transform[] SpawnPoints { get; }
        IObservable<BoxView> BoxViewCreated { get; }
        MovementSettings MovementSettings { get; }

        UniTask SpawnNote(NoteInfo note, CancellationToken ct);
    }
}
