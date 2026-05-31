using UnityEngine;
using System.IO;
using System;
using System.Text;
using Infrastructure.SaveSystem.Data;

namespace Infrastructure.SaveSystem.Core
{
    public class FileDataHandler : IDataHandler
    {
        private string dataDirPath = "";
        private string dataFileName = "";

        private string encryptionCodeWord = "esland";

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
                catch (Exception)
                {
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
                
                //

                if (string.IsNullOrEmpty(dataToStore) || dataToStore == "{}")
                {
                }

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
            catch (Exception)
            {
            }
        }

        private string EncryptDecrypt(string data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append((char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]));
            }
            return sb.ToString();
        }
    }
}


