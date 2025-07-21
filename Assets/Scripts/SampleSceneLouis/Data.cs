using System.Collections.Generic;
using ExcelDataReader;
using System.Text;
using UnityEngine;
using System.IO;

public class Data
{
    public static void ExcelToCsv(string excelFilePath, string csvFilePath, int worksheetNumber = 1)
    {
        try
        {
            if (!System.IO.File.Exists(excelFilePath))
            {
                Debug.LogError("Fichier Excel introuvable : " + excelFilePath);
                return;
            }

            using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    using (var writer = new StreamWriter(csvFilePath, false, Encoding.UTF8))
                    {
                        if (worksheetNumber > 1)
                        {
                            Debug.LogWarning("ExcelDataReader lit la première feuille par défaut. Gestion de worksheetNumber > 1 non implémentée dans cet exemple simple.");
                        }

                        while (reader.Read())
                        {
                            string[] cells = new string[reader.FieldCount];
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string cellValue = reader.GetValue(i)?.ToString() ?? "";

                                if (cellValue.Contains(',') || cellValue.Contains('"'))
                                {
                                    cellValue = "\"" + cellValue.Replace("\"", "\"\"") + "\"";
                                }
                                cells[i] = cellValue;
                            }
                            writer.WriteLine(string.Join(";", cells));
                        }
                    }
                }
            }
            Debug.Log("Fichier Excel converti en CSV : " + csvFilePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Erreur lors de la conversion Excel vers CSV : {ex.Message}\n{ex.StackTrace}");
        }
    }

    public static List<string> ReadCsvAndGetColumns(string csvFilePath)
    {
        List<string> col = new List<string>();
        try
        {
            if (!File.Exists(csvFilePath))
            {
                Debug.LogError($"Fichier CSV introuvable : {csvFilePath}");
                return col;
            }

            string firstLine = File.ReadAllLines(csvFilePath, Encoding.UTF8)[0];
            string[] cellsOfFirstLine = firstLine.Split(';');
            foreach (string cell in cellsOfFirstLine)
            {
                col.Add(cell);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Erreur lors de la lecture du fichier CSV '{csvFilePath}': {ex.Message}\n{ex.StackTrace}");
        }

        return col;
    }

    public static List<string[]> ReadCsvAndGetData(string csvFilePath, string targetValue, int searchIndex)
    {
        List<string[]> datas = new List<string[]>();
        try
        {
            if (!File.Exists(csvFilePath))
            {
                Debug.LogError($"Fichier CSV introuvable : {csvFilePath}");
                return datas;
            }

            string[] lines = File.ReadAllLines(csvFilePath, Encoding.UTF8);

            foreach (string line in lines)
            {
                string[] values = line.Split(';');

                if (values[searchIndex].Trim().Equals(targetValue, System.StringComparison.OrdinalIgnoreCase))
                {
                    string[] trimmedValues = new string[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        trimmedValues[i] = values[i].Trim();
                    }
                    datas.Add(trimmedValues);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Erreur lors de la lecture du fichier CSV '{csvFilePath}': {ex.Message}\n{ex.StackTrace}");
        }

        return datas;
    }
}