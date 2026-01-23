using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EarthquakePlayer))]
public class DateJump : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EarthquakePlayer player = (EarthquakePlayer)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Date Jump", EditorStyles.boldLabel);

        GUI.enabled = Application.isPlaying;

        if (GUILayout.Button("Jump To Date"))
        {
            // ★ 正解：存在するメソッドを呼ぶ
            player.JumpToDate();
        }

        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Playモード中のみ日付ジャンプできます。",
                MessageType.Info
            );
        }
    }
}