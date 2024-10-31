using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class JsonDecryptor
{
    private string key = "A1b2C3d4E5f6G7h8"; // 暗号化に使用したのと同じ鍵

    // 暗号化されたJSONファイルを読み込んで復号化し、内容を出力する
    public string ReadAndDecryptJson(string filePath)
    {
        try
        {
            // ファイルの内容を読み込む
            string encryptedContent = File.ReadAllText(filePath);

            // AESで復号化
            string decryptedJson = Decrypt(encryptedContent, key);

            // 復号化された内容をデバッグに出力
            //Debug.Log("復号化されたJSON内容: " + decryptedJson);

            // 復号化されたJSONを返す
            return decryptedJson;
        }
        catch (Exception e)
        {
            Debug.LogError("復号化中にエラーが発生しました: " + e.Message);
            return null;
        }
    }

    // AESで復号化
    public static string Decrypt(string cipherText, string key)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }
}