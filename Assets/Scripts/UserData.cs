using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

namespace ColorBath
{
    public class UserData
    {
        string token;

        public string GetGeminiToken()
        {
            string path = Path.Combine(Application.persistentDataPath, "GeminiToken.json");

            if (!File.Exists(path))
            {
                Debug.LogWarning("ユーザデータが見つかりませんでした");
                return null;
            }

            string encryptedJson = File.ReadAllText(path);
            string json = CryptoHelper.Decrypt(encryptedJson);
            GeminiTokenJson data = JsonUtility.FromJson<GeminiTokenJson>(json);
            return data.Token;
        }

        public void SetGeiminiToken(string token)
        {
            GeminiTokenJson geminiToken = new GeminiTokenJson();
            geminiToken.Token = token;
            string json = JsonUtility.ToJson(geminiToken);
            string encrypted = CryptoHelper.Encrypt(json);

            string path = Path.Combine(Application.persistentDataPath, "GeminiToken.json");
            File.WriteAllText(path, encrypted);
        }

        [System.Serializable]
        public class GeminiTokenJson
        {
            public string Token;
        }
    }
}
