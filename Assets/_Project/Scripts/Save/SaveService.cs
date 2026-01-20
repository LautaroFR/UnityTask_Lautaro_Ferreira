using System.IO;
using UnityEngine;

namespace Project.Save
{
    public static class SaveService
    {
        private const string FileName = "save.json";

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static void Save(PlayerSaveData data)
        {
            if (data == null) return;

            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(FilePath, json);
        }

        public static bool TryLoad(out PlayerSaveData data)
        {
            data = null;

            if (!File.Exists(FilePath))
                return false;

            try
            {
                string json = File.ReadAllText(FilePath);
                data = JsonUtility.FromJson<PlayerSaveData>(json);
                return data != null;
            }
            catch
            {
                data = null;
                return false;
            }
        }

        public static void Delete()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        public static string GetDebugPath() => FilePath;
    }
}
