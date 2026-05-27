using System.IO;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    private Data _data;

    public static PlayerData Instance;

    void Awake()
    {
        if(Instance != this && Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (File.Exists(Application.persistentDataPath + "/player_data.save"))
        {
            // Cargar datos
        }
        else
        {
            // Cargar modo de creación de datos
        }
    }

    public void CreateDataFile(string username)
    {
        _data = new Data(username);
        // Asignar icono numero 0 y aplicar nombre puesto
    }
}
