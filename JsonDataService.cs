using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonDataService : IDataService
{
    public bool SaveData<T>(string RelativePath, T Data, bool Encrypted=false,bool AddToFile=false)
    {
        string path=Application.persistentDataPath +  RelativePath;
        try
        {
            if (AddToFile == false)
            {
                if(File.Exists(path))
                {
                    File.Delete(path);
                }
                using FileStream stream=File.Create(path);
                stream.Close();
                File.WriteAllText(path,JsonConvert.SerializeObject(Data));
            }
            else
            {
                File.AppendAllText(path, JsonConvert.SerializeObject(Data));
            }
            return true;
        }catch(Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
    }
    public T LoadData<T>(string RelativePath, bool Encrypted)
    {
        string path=Application.persistentDataPath+RelativePath;
        if(!File.Exists(path))
        {
            Debug.LogError("File not there");
            return default;
        }
        try
        {
            T data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            return data;
        }catch(Exception e)
        {
            Debug.LogError(e.Message);
            throw e;
        }
    }

}
