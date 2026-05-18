using UnityEngine;
using System.IO;
using System;
using Infrastructure.SaveSystem.Data;

namespace Infrastructure.SaveSystem.Core
{
    public class FileDataHandler : IDataHandler
    {
        private string dataDirPath = "";
        private string dataFileName = "";

        private readonly string encryptionCodeWord = "Esland_CodeWord";

        public FileDataHandler(string dataDirPath, string dataFileName)
        {
            this.dataDirPath = dataDirPath;
            this.dataFileName = dataFileName;
        }

        public GameData Load()
        {
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            GameData loadedData = null;

            if (File.Exists(fullPath))
            {
                try
                {
                    string dataToLoad = "";
                    using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }

                    // GIẢI MÃ XOR
                    dataToLoad = EncryptDecrypt(dataToLoad);

                    // Chuyển JSON sang GameData
                    loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
                }
                catch (Exception e)
                {
                    Debug.LogError("[FileDataHandler] Lỗi khi đọc file save: " + fullPath + "\n" + e);
                }
            }
            return loadedData;
        }

        public void Save(GameData data)
        {
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                
                // Chuyển sang JSON (có format đẹp để debug)
                string dataToStore = JsonUtility.ToJson(data, true);
                
                // MÃ HÓA XOR trước khi ghi
                dataToStore = EncryptDecrypt(dataToStore);

                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(dataToStore);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[FileDataHandler] Lỗi khi ghi file save: " + fullPath + "\n" + e);
            }
        }

        private string EncryptDecrypt(string data)
        {
            string modifiedData = "";
            for (int i = 0; i < data.Length; i++)
            {
                modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
            }
            return modifiedData;
        }
    }
}
