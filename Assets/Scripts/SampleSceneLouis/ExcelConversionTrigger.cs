using UnityEngine;
using System.IO;

public class ExcelConversionTrigger {
    private string excelFileName = "Extraction salles du 14 au 18 avril 2025.xlsx";
    private string csvFileName = "ConvertedData.csv";
    private int worksheetToConvert = 1;

    public ExcelConversionTrigger(string excelFileName)
    {
        this.excelFileName = excelFileName;
    }

    public void ConvertExcelFile()
    {
        string excelPath = Path.Combine(Application.streamingAssetsPath, excelFileName);
        Debug.Log($"Success 1");
        string csvPath = Path.Combine(Application.streamingAssetsPath, csvFileName);
        Debug.Log($"Success 2");

        Debug.Log($"Tentative de conversion de {excelPath} vers {csvPath}...");

        Data.ExcelToCsv(excelPath, csvPath, worksheetToConvert);

        Debug.Log("Conversion Excel->CSV terminée. Mise à jour des objets Cube_Color...");

        CubeColor[] allCubeColorScripts = Object.FindObjectsByType<CubeColor>(FindObjectsSortMode.None);

        foreach (CubeColor scriptInstance in allCubeColorScripts)
        {
            scriptInstance.onClick();
        }

        GameManager.Instance.setup();

        Debug.Log($"Mise à jour demandée pour {allCubeColorScripts.Length} objets Cube_Color.");
    }
}