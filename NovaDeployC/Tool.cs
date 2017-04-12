using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Net;

namespace NovaDeployC
{
    public class Tool
    {
        public static void Unzip(string zipFile, string dir)
        {
            //ZipFile.ExtractToDirectory(zipFile, dir);

            ZipArchive archive = ZipFile.Open(zipFile, ZipArchiveMode.Read);
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(dir, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (!string.IsNullOrEmpty(file.Name))
                    file.ExtractToFile(completeFileName, true);
            }
        }
        public static void ZipDir2File(string dir, string zipFile)
        {
            string dstDir = Path.GetDirectoryName(zipFile);
            if (!string.IsNullOrEmpty(dstDir))
                Directory.CreateDirectory(dstDir);

            if (File.Exists(zipFile))
                File.Delete(zipFile);
            ZipFile.CreateFromDirectory(dir, zipFile);
        }

        public static string tempDir = "_temp_";
        public static void CreateTempDir() { Directory.CreateDirectory(tempDir); }
        public static string SaveToTempFile(string fileNamePrefix, string ext, byte[] data)
        {
            CreateTempDir();
            string fileName = tempDir + "\\" + fileNamePrefix + DateTime.Now.Ticks.ToString() + ext;
            File.WriteAllBytes(fileName, data);
            return fileName;
        }

        public static string ObjectToBase64(object obj)
        {
            string text = JsonFx.Json.JsonWriter.Serialize(obj);
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            string base64Text = Convert.ToBase64String(bytes);
            return base64Text;
        }
        public static T Base64ToObject<T>(string base64Text)
        {
            byte[] bytes = Convert.FromBase64String(base64Text);
            string text = Encoding.UTF8.GetString(bytes);
            T list = JsonFx.Json.JsonReader.Deserialize<T>(text);
            return list;
        }

    }
}
