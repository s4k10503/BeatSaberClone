using System;
using System.Collections.Generic;
using UnityEngine;
using BeatSaberClone.Domain;
using Zenject;

namespace BeatSaberClone.Infrastructure
{
    public sealed class JsonNotesRepository : INotesRepository
    {

        [Inject]
        public JsonNotesRepository()
        {
        }

        public void Dispose()
        {
        }

        public List<NoteInfo> LoadNotesData()
        {
            try
            {
                TextAsset notesData = Resources.Load<TextAsset>("NotesData");
                if (notesData == null)
                {
                    throw new InfrastructureException("Notes data not found in Resources!");
                }

                var container = JsonUtility.FromJson<NotesContainer>(notesData.text);
                if (container == null || container.NotesList == null)
                {
                    throw new InfrastructureException("Notes data is invalid or empty.");
                }
                return container.NotesList;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Failed to load notes: ", ex);
            }
        }
    }
}
