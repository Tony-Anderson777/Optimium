using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine;

public class MainMenuEvents : MonoBehaviour
{
    private Button _button_generate_map;
    private Button _button_select_file;
    private UIDocument _document;

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
        DataSessionManager.instance.SetFile("Extraction salles du 14 au 18 avril 2025.xlsx");
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
