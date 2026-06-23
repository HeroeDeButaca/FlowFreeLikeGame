using System.IO;
using UnityEngine;

public static class JsonCreator
{
    private static readonly string CRYPTO_DATA_KEY = "X7mQ2vK9pL4nR8cT1yW6jH3sD5uF0zBq";

    public static void SaveData<T>(T data, string saveFolder, string fileName)
    {
        string json = JsonUtility.ToJson(data, true);
        //Rijndael crypto = new Rijndael();

        //byte[] soup = crypto.Encrypt(json, CRYPTO_DATA_KEY);

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        //File.WriteAllBytes(saveFolder + fileName, soup);
        File.WriteAllText(saveFolder + fileName, json);

    }

    public static T LoadData<T>(string filePath)
    {
        //Rijndael crypto = new Rijndael();
        
        //byte[] soupBackIn = File.ReadAllBytes(filePath);
        //string jsonFromFile = crypto.Decrypt(soupBackIn, CRYPTO_DATA_KEY);

        string jsonText = File.ReadAllText(filePath);
        T data = JsonUtility.FromJson<T>(jsonText);
        //T data = JsonUtility.FromJson<T>(jsonFromFile);

        return data;
    }
}
