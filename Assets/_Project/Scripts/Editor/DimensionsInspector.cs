using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Transform))]
public class DimensionsInspector : Editor
{
    public override void OnInspectorGUI()
    {
        // Affiche l'Inspector normal
        DrawDefaultInspector();

        Transform t = (Transform)target;

        Renderer[] renderers = t.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            EditorGUILayout.HelpBox("Aucun Renderer trouvé sur cet objet.", MessageType.Info);
            return;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Vector3 size = bounds.size;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dimensions du modèle", EditorStyles.boldLabel);

        EditorGUILayout.Vector3Field("Taille (X,Y,Z)", size);
    }
}