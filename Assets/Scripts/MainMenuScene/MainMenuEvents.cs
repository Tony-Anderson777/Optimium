using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine;
using SFB;

public class MainMenuEvents : MonoBehaviour
{
    private Button _button_generate_map;
    private Button _button_select_file;
    private UIDocument _document;
    public UIDocument uiDocument;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();

        _button_select_file = _document.rootVisualElement.Q("SelectFile") as Button;
        _button_generate_map = _document.rootVisualElement.Q("GenerateMap") as Button;
        _button_select_file.RegisterCallback<ClickEvent>(OnSelectFileClick);
        _button_generate_map.RegisterCallback<ClickEvent>(OnGenerateMapClick);
    }

    private void Onsable()
    {
        _button_select_file.UnregisterCallback<ClickEvent>(OnSelectFileClick);
        _button_generate_map.UnregisterCallback<ClickEvent>(OnGenerateMapClick);
    }

    private void OnSelectFileClick(ClickEvent evt)
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Select File", "", "", false);
        if (paths.Length > 0)
        {
            Debug.Log("Fichier sélectionné : " + paths[0]);

            VisualElement root = uiDocument.rootVisualElement;
            Label pathLabel = root.Q<Label>("Path");
            pathLabel.text = paths[0];
        }
        DataSessionManager.instance.SetFile(paths[0]);
    }

    private void OnGenerateMapClick(ClickEvent evt)
    {
        if (DataSessionManager.instance.GetFile() == null)
        {
            Debug.Log("No File Selected");
        } else {
            Debug.Log("Button Clicked, charging scene...");
            StartCoroutine(LoadSceneAndConvert());
        }
    }

    private IEnumerator LoadSceneAndConvert()
    {
        var asyncLoad = SceneManager.LoadSceneAsync("SampleSceneLouis");

        while (!asyncLoad.isDone)
        {
            Debug.Log($"{asyncLoad.progress}");
            yield return null;
        }
    }
}
