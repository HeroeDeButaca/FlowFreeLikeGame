using System.IO;
using UnityEngine;

public static class JsonCreator
{
    private static readonly string CRYPTO_DATA_KEY = "X7mQ2vK9pL4nR8cT1yW6jH3sD5uF0zBq";

    public static void EncryptSaveData<T>(T data, string saveFolder, string fileName)
    {
        string json = JsonUtility.ToJson(data, true);
        Rijndael crypto = new Rijndael();

        byte[] soup = crypto.Encrypt(json, CRYPTO_DATA_KEY);

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        File.WriteAllBytes(saveFolder + fileName, soup);
    }

    public static T LoadEncryptedData<T>(string filePath)
    {
        Rijndael crypto = new Rijndael();
        
        byte[] soupBackIn = File.ReadAllBytes(filePath);
        string jsonFromFile = crypto.Decrypt(soupBackIn, CRYPTO_DATA_KEY);

        T data = JsonUtility.FromJson<T>(jsonFromFile);

        return data;
    }

    public static T SaveData<T>(T data, string saveFolder, string fileName)
    {
        string json = JsonUtility.ToJson(data, true);

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        File.WriteAllText(saveFolder + fileName, json);

        return data;
    }

    public static T LoadData<T>(string filePath)
    {
        string jsonText = File.ReadAllText(filePath);
        T data = JsonUtility.FromJson<T>(jsonText);

        return data;
    }
}
