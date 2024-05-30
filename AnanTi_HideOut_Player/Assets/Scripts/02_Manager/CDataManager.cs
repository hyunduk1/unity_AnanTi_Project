using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
[System.Serializable]
public class SavePosition
{
	public SavePosition(Vector3 ContentScale00, Vector3 ContentScale01, Vector3 ContentScale02, Vector3 ContentScale03, Vector3 ContentScale04)
    {
		Content_Scale = new Vector3[] { ContentScale00 , ContentScale01 , ContentScale02 , ContentScale03 , ContentScale04 };
    }
	public Vector3[] Content_Scale;
}

[System.Serializable]
public class SaveData
{
	public SaveData(Vector3 content00, Vector3 content01, Vector3 content02, Vector3 content03, Vector3 content04)
	{
		Content_Pos = new Vector3[] { content00, content01, content02, content03, content04 };

	}

	public Vector3[] Content_Pos;
}
public static class SaveSystem
{
	private static string SavePath => Application.persistentDataPath + "/Save/";

	public static void Save(SaveData saveData, string saveFileName)
	{
		if (!Directory.Exists(SavePath))
		{
			Directory.CreateDirectory(SavePath);
		}

		string saveJson = JsonUtility.ToJson(saveData);

		string saveFilePath = SavePath + saveFileName + ".json";
		File.WriteAllText(saveFilePath, saveJson);
		Debug.Log("Save Success: " + saveFilePath);
	}

	public static void SaveScale(SavePosition saveData, string saveFileName)
	{
		if (!Directory.Exists(SavePath))
		{
			Directory.CreateDirectory(SavePath);
		}

		string saveJson = JsonUtility.ToJson(saveData);

		string saveFilePath = SavePath + saveFileName + ".json";
		File.WriteAllText(saveFilePath, saveJson);
		Debug.Log("Save Success: " + saveFilePath);
	}

	public static SaveData Load(string saveFileName)
	{
		string saveFilePath = SavePath + saveFileName + ".json";

		if (!File.Exists(saveFilePath))
		{
			Debug.LogError("No such saveFile exists");
			return null;
		}

		string saveFile = File.ReadAllText(saveFilePath);
		SaveData saveData = JsonUtility.FromJson<SaveData>(saveFile);
		return saveData;
	}

	public static SavePosition LoadScale(string saveFileName)
	{
		string saveFilePath = SavePath + saveFileName + ".json";

		if (!File.Exists(saveFilePath))
		{
			Debug.LogError("No such saveFile exists");
			return null;
		}

		string saveFile = File.ReadAllText(saveFilePath);
		SavePosition saveData = JsonUtility.FromJson<SavePosition>(saveFile);
		return saveData;
	}
}