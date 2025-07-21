using UnityEngine;

public class DataSessionManager : MonoBehaviour
{
    public static DataSessionManager instance;
    public static string file = null;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetFile(string path){
        file = path;
    }

    public string GetFile(){
        return file;
    }

}
