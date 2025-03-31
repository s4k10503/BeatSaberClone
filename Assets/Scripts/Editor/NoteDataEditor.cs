using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using BeatSaberClone.Domain;

namespace BeatSaberClone.Editor
{
    public class NoteDataEditor : EditorWindow
    {
        private List<NoteInfo> notes = new();
        private Vector2 scrollPosition;
        private float time = 0f;
        private int lineIndex = 0;
        private int lineLayer = 0;
        private int type = 0;
        private int cutDirection = 0;

        // タイムライン表示用の設定
        private float timelineStart = 0f;
        private float timelineEnd = 300f;
        private float laneWidth = 100f;
        private float zoomLevel = 1f;
        private float timeScale = 1f;
        private bool isDragging = false;
        private NoteInfo selectedNote = null;

        [MenuItem("Beat Saber Clone/Note Data Editor")]
        public static void ShowWindow()
        {
            GetWindow<NoteDataEditor>("Note Data Editor");
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawTimeline();
            DrawNoteProperties();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Save", EditorStyles.toolbarButton)) SaveToJson();
            if (GUILayout.Button("Load", EditorStyles.toolbarButton)) LoadFromJson();

            GUILayout.FlexibleSpace();

            // タイムラインの長さを調整
            EditorGUILayout.LabelField("Duration:", GUILayout.Width(60));
            if (GUILayout.Button("-30s", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                timelineEnd = Mathf.Max(60f, timelineEnd - 30f);
            }
            EditorGUILayout.LabelField($"{timelineEnd:F0}s", GUILayout.Width(40));
            if (GUILayout.Button("+30s", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                timelineEnd = Mathf.Min(600f, timelineEnd + 30f); // 最大10分まで
            }

            GUILayout.Space(10);

            // ズームコントロール
            EditorGUILayout.LabelField("Zoom:", GUILayout.Width(50));
            if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                zoomLevel = Mathf.Max(0.1f, zoomLevel - 0.1f);
            }
            EditorGUILayout.LabelField($"{zoomLevel:F1}x", GUILayout.Width(50));
            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                zoomLevel = Mathf.Min(5f, zoomLevel + 0.1f);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTimeline()
        {
            // タイムラインのスクロールビュー
            float contentWidth = (timelineEnd - timelineStart) * laneWidth * zoomLevel;
            Rect viewRect = EditorGUILayout.GetControlRect(false, 400f);
            Rect contentRect = new Rect(0, 0, contentWidth, viewRect.height);

            scrollPosition = GUI.BeginScrollView(viewRect, scrollPosition, contentRect, false, false);

            // タイムライン本体
            Rect timelineRect = new Rect(0, 0, contentWidth, viewRect.height);
            GUI.Box(timelineRect, "");

            // 時間軸の目盛りを描画
            timeScale = laneWidth * zoomLevel;
            float majorTickInterval = 1.0f;
            float minorTickInterval = 0.25f;

            // メジャー目盛り（1秒単位）
            for (float t = timelineStart; t <= timelineEnd; t += majorTickInterval)
            {
                float x = (t - timelineStart) * timeScale;
                Rect labelRect = new Rect(x - 15, 0, 30, 20);
                GUI.Label(labelRect, t.ToString("F0"));

                // メジャー目盛りの線
                Handles.DrawLine(
                    new Vector3(x, 0, 0),
                    new Vector3(x, viewRect.height, 0)
                );
            }

            // マイナー目盛り（0.25秒単位）
            for (float t = timelineStart; t <= timelineEnd; t += minorTickInterval)
            {
                float x = (t - timelineStart) * timeScale;
                float tickHeight = 10f;
                Handles.DrawLine(
                    new Vector3(x, 0, 0),
                    new Vector3(x, tickHeight, 0)
                );
            }

            // レーン区切り線を描画
            float laneHeight = viewRect.height / 4;
            for (int lane = 0; lane <= 4; lane++)
            {
                float y = lane * laneHeight;
                Handles.DrawLine(
                    new Vector3(0, y, 0),
                    new Vector3(contentWidth, y, 0)
                );
            }

            // ノーツを描画
            float baseNoteSize = 20f;
            float noteYOffset = (laneHeight - baseNoteSize) / 2; // ノーツをレーンの中央に配置するためのオフセット
            float layerOffset = baseNoteSize * 0.8f; // レイヤー間の高さ差

            // ノーツを時間順にソート
            var sortedNotes = new List<NoteInfo>(notes);
            sortedNotes.Sort((a, b) =>
            {
                // まず時間でソート
                int timeCompare = a._time.CompareTo(b._time);
                if (timeCompare != 0) return timeCompare;

                // 時間が同じ場合はレイヤーでソート（レイヤーが小さいほど先に描画）
                return a._lineLayer.CompareTo(b._lineLayer);
            });

            foreach (var note in sortedNotes)
            {
                float x = (note._time - timelineStart) * timeScale;
                float y = note._lineIndex * laneHeight + noteYOffset - note._lineLayer * layerOffset;

                // レイヤーに応じてノーツのサイズを調整（レイヤーが大きいほど大きい）
                float noteSize = baseNoteSize * (1.0f + (note._lineLayer * 0.15f));
                float noteXOffset = (baseNoteSize - noteSize) / 2; // サイズ変更による位置調整

                Rect noteRect = new Rect(
                    x - noteSize / 2,
                    y,
                    noteSize,
                    noteSize
                );

                // ノーツの種類に応じて色を変更（レイヤーに応じて明度を変更）
                Color noteColor = note._type == 0 ? Color.red : Color.blue;
                float brightness = 0.5f + (note._lineLayer * 0.25f); // レイヤーが上がるほど明るく
                noteColor.r = Mathf.Min(1f, noteColor.r * brightness);
                noteColor.g = Mathf.Min(1f, noteColor.g * brightness);
                noteColor.b = Mathf.Min(1f, noteColor.b * brightness);

                // ノーツの縁取りを描画
                Color outlineColor = note == selectedNote ? Color.yellow : Color.white;
                float outlineWidth = note == selectedNote ? 2f : 1f;
                EditorGUI.DrawRect(new Rect(noteRect.x - outlineWidth, noteRect.y - outlineWidth,
                    noteRect.width + outlineWidth * 2, noteRect.height + outlineWidth * 2), outlineColor);

                // ノーツ本体を描画
                EditorGUI.DrawRect(noteRect, noteColor);

                // マウスイベントの処理
                Event e = Event.current;
                if (e.type == EventType.MouseDown && noteRect.Contains(e.mousePosition))
                {
                    selectedNote = note;
                    isDragging = true;
                    e.Use();
                }
            }

            // ドラッグ処理
            if (isDragging && selectedNote != null)
            {
                Event e = Event.current;
                if (e.type == EventType.MouseDrag)
                {
                    float newTime = (e.mousePosition.x + scrollPosition.x) / timeScale + timelineStart;
                    float newLine = Mathf.Floor((e.mousePosition.y) / laneHeight);

                    selectedNote._time = Mathf.Max(0, newTime);
                    selectedNote._lineIndex = Mathf.Clamp((int)newLine, 0, 3);

                    e.Use();
                    Repaint();
                }
                else if (e.type == EventType.MouseUp)
                {
                    isDragging = false;
                    e.Use();
                }
            }

            GUI.EndScrollView();
        }

        private void DrawNoteProperties()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Note Properties", EditorStyles.boldLabel);

            if (selectedNote != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Selected Note", EditorStyles.boldLabel);
                if (GUILayout.Button("Deselect", GUILayout.Width(80)))
                {
                    selectedNote = null;
                }
                EditorGUILayout.EndHorizontal();

                selectedNote._time = EditorGUILayout.FloatField("Time", selectedNote._time);
                selectedNote._lineIndex = EditorGUILayout.IntSlider("Line Index", selectedNote._lineIndex, 0, 3);
                selectedNote._lineLayer = EditorGUILayout.IntSlider("Line Layer", selectedNote._lineLayer, 0, 2);
                selectedNote._type = EditorGUILayout.IntSlider("Type", selectedNote._type, 0, 1);
                selectedNote._cutDirection = EditorGUILayout.IntSlider("Cut Direction", selectedNote._cutDirection, 0, 7);

                if (GUILayout.Button("Delete Note"))
                {
                    notes.Remove(selectedNote);
                    selectedNote = null;
                }
            }
            else
            {
                // 新規ノーツ作成用の入力フィールド
                GUILayout.Label("New Note", EditorStyles.boldLabel);
                time = EditorGUILayout.FloatField("Time", time);
                lineIndex = EditorGUILayout.IntSlider("Line Index", lineIndex, 0, 3);
                lineLayer = EditorGUILayout.IntSlider("Line Layer", lineLayer, 0, 2);
                type = EditorGUILayout.IntSlider("Type", type, 0, 1);
                cutDirection = EditorGUILayout.IntSlider("Cut Direction", cutDirection, 0, 7);

                if (GUILayout.Button("Add Note"))
                {
                    notes.Add(new NoteInfo
                    {
                        _time = time,
                        _lineIndex = lineIndex,
                        _lineLayer = lineLayer,
                        _type = type,
                        _cutDirection = cutDirection
                    });
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void SaveToJson()
        {
            var container = new NotesContainer { NotesList = notes };
            string json = JsonUtility.ToJson(container, true);

            string path = EditorUtility.SaveFilePanel(
                "Save Note Data",
                "Assets/Resources",
                "NotesData.json",
                "json");

            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, json);
                AssetDatabase.Refresh();
            }
        }

        private void LoadFromJson()
        {
            string path = EditorUtility.OpenFilePanel(
                "Load Note Data",
                "Assets/Resources",
                "json");

            if (!string.IsNullOrEmpty(path))
            {
                string json = System.IO.File.ReadAllText(path);
                var container = JsonUtility.FromJson<NotesContainer>(json);
                notes = container.NotesList;
            }
        }
    }
}
