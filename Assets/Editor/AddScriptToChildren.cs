using UnityEngine;
using UnityEditor;

public class AddScriptToChildren
{
    [MenuItem("Tools/Add Script To Selected Parent's Children")]
    static void AddScript()
    {
        string[] exclure = { "2e étage", "Objet vide" };

        foreach (GameObject parent in Selection.gameObjects)
        {
            foreach (Transform child in parent.transform)
            {
                if (System.Array.Exists(exclure, nom => nom == child.name))
                {
                    Debug.Log($"Exclusion: {child.name}");
                    continue;
                }
                if (child.GetComponent<Cube_Color>() == null)
                {
                    child.gameObject.AddComponent<Cube_Color>();
                    Debug.Log($"Script ajouté à: {child.name}");
                }
            }
        }
    }
}
