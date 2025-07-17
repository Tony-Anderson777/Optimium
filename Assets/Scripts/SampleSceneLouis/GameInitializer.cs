using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        ExcelConversionTrigger trigger = new ExcelConversionTrigger(DataSessionManager.instance.GetFile());
        trigger.ConvertExcelFile();
    }
}
