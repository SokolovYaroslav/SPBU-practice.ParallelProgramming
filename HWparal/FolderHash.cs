using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace HWparal
{
    public static class FolderHash
    {
        public static string FolderGetHash(string path) {
            using (var md5 = MD5.Create()) {
                var stringBuilder = new StringBuilder();
                var folderName = GetName(path);
                var folders = Directory.GetDirectories(path);
                var files = Directory.GetFiles(path);

                stringBuilder.Append(StringMD5Hash(folderName));


                List<Task<string>> tasks = new List<Task<string>>();
                foreach (var folder in folders) {
                    tasks.Add(Task<string>.Run(() => FolderGetHash(folder)));
                }
                
                foreach (var file in files) {
                    using (var stream = File.OpenRead(file)) {
                        stringBuilder.Append(md5.ComputeHash(stream));
                    }
                }
                
                try {
                    Task.WaitAll(tasks.ToArray());
                }
                catch (AggregateException e) {
                    Console.WriteLine(e);
                    throw;
                }

                foreach (var task in tasks) {
                    try {
                        stringBuilder.Append(task.Result);
                    }
                    catch (AggregateException e) {
                        Console.WriteLine(e);
                        throw;
                    }   
                }

                return StringMD5Hash(stringBuilder.ToString());
            }
        }
        
        public static string StringMD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }

        private static string GetName(string path) {
            return path.Split('/').Last();
        }
    }
}