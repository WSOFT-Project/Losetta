using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace AliceScript.NameSpaces
{
    class Alice_IO_Intiter
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.IO");

            space.Add(new file_existsFunc());
            space.Add(new file_moveFunc());
            space.Add(new file_copyFunc());
            space.Add(new file_deleteFunc());
            space.Add(new file_encryptFunc());
            space.Add(new file_decrypt());
            space.Add(new file_read_dataFunc());
            space.Add(new file_read_textFunc());
            space.Add(new file_write_dataFunc());
            space.Add(new file_write_textFunc());
            space.Add(new file_append_textFunc());

            space.Add(new directory_copyFunc());
            space.Add(new directory_moveFunc());
            space.Add(new directory_deleteFunc());
            space.Add(new directory_existsFunc());
            space.Add(new directory_createFunc());
            space.Add(new directory_getfilesFunc());
            space.Add(new directory_getdirectoriesFunc());
            space.Add(new directory_currentdirectoryFunc());

            space.Add(new path_ChangeExtensionFunc());
            space.Add(new path_CombineFunc());
            space.Add(new path_EndsInDirectorySeparatorFunc());
            space.Add(new path_get_DirectoryNameFunc());
            space.Add(new path_get_ExtensionFunc());
            space.Add(new path_get_FileNameFunc());
            space.Add(new path_get_FileNameWithoutExtensionFunc());
            space.Add(new path_get_FullPathFunc());
            space.Add(new path_get_GetRelativePathFunc());
            space.Add(new path_get_PathRootFunc());
            space.Add(new path_get_RandomFileNameFunc());
            space.Add(new path_get_TempFileNameFunc());
            space.Add(new path_get_TempPathFunc());
            space.Add(new path_HasExtensionFunc());
            space.Add(new path_IsPathFullyQualifiedFunc());
            space.Add(new path_IsPathRootedFunc());
            space.Add(new path_JoinFunc());
            space.Add(new path_TrimEndingDirectorySeparatorFunc());

            NameSpaceManerger.Add(space);
        }
    }
    class path_ChangeExtensionFunc : FunctionBase
    {
        public path_ChangeExtensionFunc()
        {
            this.Name = "path_ChangeExtension";
            this.MinimumArgCounts = 2;
            this.Run += Path_ChangeExtensionFunc_Run;
        }

        private void Path_ChangeExtensionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.ChangeExtension(e.Args[0].AsString(),e.Args[1].AsString()));
        }
    }
    class path_CombineFunc : FunctionBase
    {
        public path_CombineFunc()
        {
            this.Name = "path_Combine";
            this.MinimumArgCounts = 2;
            this.Run += Path_CombineFunc_Run;
        }

        private void Path_CombineFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            List<string> vs = new List<string>();
            foreach(Variable v in e.Args)
            {
                vs.Add(v.AsString());
            }
            e.Return = new Variable(Path.Combine(vs.ToArray()));
        }
    }
    class path_EndsInDirectorySeparatorFunc : FunctionBase
    {
        public path_EndsInDirectorySeparatorFunc()
        {
            this.Name = "path_EndsInDirectorySeparator";
            this.MinimumArgCounts = 1;
            this.Run += Path_EndsInDirectorySeparatorFunc_Run;
        }

        private void Path_EndsInDirectorySeparatorFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.EndsInDirectorySeparator(e.Args[0].AsString()));
        }
    }
    class path_get_DirectoryNameFunc : FunctionBase
    {
        public path_get_DirectoryNameFunc()
        {
            this.Name = "path_get_DirectoryName";
            this.MinimumArgCounts = 1;
            this.Run += Path_get_DirectoryNameFunc_Run;
        }

        private void Path_get_DirectoryNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.GetDirectoryName(e.Args[0].AsString()));
        }
    }
    class path_get_ExtensionFunc : FunctionBase
    {
        public path_get_ExtensionFunc()
        {
            this.Name = "path_get_Extension";
            this.MinimumArgCounts = 1;
            this.Run += Path_get_ExtensionFunc_Run;
        }

        private void Path_get_ExtensionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.GetExtension(e.Args[0].AsString()));
        }
    }
    class path_get_FileNameFunc : FunctionBase
    {
        public path_get_FileNameFunc()
        {
            this.Name = "path_get_FileName";
            this.MinimumArgCounts = 1;
            this.Run += Path_get_FileNameFunc_Run;
        }

        private void Path_get_FileNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.GetFileName(e.Args[0].AsString()));
        }
    }
    class path_get_FileNameWithoutExtensionFunc : FunctionBase
    {
        public path_get_FileNameWithoutExtensionFunc()
        {
            this.Name = "path_get_FileNameWithoutExtension";
            this.MinimumArgCounts = 1;
            this.Run += Path_get_FileNameWithoutExtensionFunc_Run;
        }

        private void Path_get_FileNameWithoutExtensionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.GetFileNameWithoutExtension(e.Args[0].AsString()));
        }
    }
    class path_get_FullPathFunc : FunctionBase
    {
        public path_get_FullPathFunc()
        {
            this.Name = "path_get_FullPath";
            this.MinimumArgCounts = 1;
            this.Run += Path_get_FullPathFunc_Run;
        }

        private void Path_get_FullPathFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 1)
            {
                e.Return = new Variable(Path.GetFullPath(e.Args[0].AsString(),e.Args[1].AsString()));
            }
            else
            {
                e.Return = new Variable(Path.GetFullPath(e.Args[0].AsString()));
            }
        }
    }
    class path_get_PathRootFunc : FunctionBase
    {
        public path_get_PathRootFunc()
        {
            this.Name = "path_get_PathRoot";
            this.MinimumArgCounts = 1;
            this.Run += Path_get_PathRootFunc_Run;
        }

        private void Path_get_PathRootFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.GetPathRoot(e.Args[0].AsString()));
        }
    }
    class path_get_RandomFileNameFunc : FunctionBase
    {
        public path_get_RandomFileNameFunc()
        {
            this.Name = "path_get_RandomFileName";
            this.MinimumArgCounts = 0;
            this.Run += Path_get_RandomFileNameFunc_Run;
        }

        private void Path_get_RandomFileNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.GetRandomFileName());
        }
    }
    class path_get_GetRelativePathFunc : FunctionBase
    {
        public path_get_GetRelativePathFunc()
        {
            this.Name = "path_get_RelativePath";
            this.MinimumArgCounts = 2;
            this.Run += Path_get_GetRelativePathFunc_Run;
        }

        private void Path_get_GetRelativePathFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.GetRelativePath(e.Args[0].AsString(),e.Args[1].AsString()));
        }
    }
    class path_get_TempFileNameFunc : FunctionBase
    {
        public path_get_TempFileNameFunc()
        {
            this.Name = "path_get_TempFileName";
            this.Run += Path_get_TempFileNameFunc_Run;
        }

        private void Path_get_TempFileNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.GetTempFileName());
        }
    }
    class path_get_TempPathFunc : FunctionBase
    {
        public path_get_TempPathFunc()
        {
            this.Name = "path_get_TempPath";
            this.Run += Path_get_TempPathFunc_Run;
        }

        private void Path_get_TempPathFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.GetTempPath());
        }
    }
    class path_HasExtensionFunc : FunctionBase
    {
        public path_HasExtensionFunc()
        {
            this.Name = "path_HasExtension";
            this.MinimumArgCounts = 1;
            this.Run += Path_HasExtensionFunc_Run;
        }

        private void Path_HasExtensionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.HasExtension(e.Args[0].AsString()));
        }
    }
    class path_IsPathFullyQualifiedFunc : FunctionBase
    {
        public path_IsPathFullyQualifiedFunc()
        {
            this.Name = "path_IsPathFullyQualified";
            this.MinimumArgCounts = 1;
            this.Run += Path_IsPathFullyQualifiedFunc_Run;
        }

        private void Path_IsPathFullyQualifiedFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.IsPathFullyQualified(e.Args[0].AsString()));
        }
    }
    class path_IsPathRootedFunc : FunctionBase
    {
        public path_IsPathRootedFunc()
        {
            this.Name = "path_IsPathRooted";
            this.MinimumArgCounts = 1;
            this.Run += Path_IsPathRootedFunc_Run;
        }

        private void Path_IsPathRootedFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.IsPathRooted(e.Args[0].AsString()));
        }
    }
    class path_JoinFunc : FunctionBase
    {
        public path_JoinFunc()
        {
            this.Name = "path_Join";
            this.MinimumArgCounts = 2;
            this.Run += Path_JoinFunc_Run;
        }

        private void Path_JoinFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            List<string> vs = new List<string>();
            foreach (Variable v in e.Args)
            {
                vs.Add(v.AsString());
            }
            e.Return = new Variable(Path.Join(vs.ToArray()));
        }
    }
    class path_TrimEndingDirectorySeparatorFunc : FunctionBase
    {
        public path_TrimEndingDirectorySeparatorFunc()
        {
            this.Name = "path_TrimEndingDirectorySeparator";
            this.MinimumArgCounts = 1;
            this.Run += Path_TrimEndingDirectorySeparatorFunc_Run;
        }

        private void Path_TrimEndingDirectorySeparatorFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Path.TrimEndingDirectorySeparator(e.Args[0].AsString()));
        }
    }
    class file_read_textFunc : FunctionBase
    {
        public file_read_textFunc()
        {
            this.Name = "file_read_text";
            this.MinimumArgCounts = 1;
            this.Run += File_read_textFunc_Run;
        }

        private void File_read_textFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count < 2)
            {
                e.Return = new Variable(SafeReader.ReadAllText(e.Args[0].AsString(),out _));
            }
            else
            {
                if (e.Args[1].Type == Variable.VarType.STRING)
                {
                    e.Return = new Variable(File.ReadAllText(e.Args[0].AsString(), Encoding.GetEncoding(e.Args[1].AsString())));
                }
                else if(e.Args[1].Type==Variable.VarType.NUMBER)
                {
                    e.Return = new Variable(File.ReadAllText(e.Args[0].AsString(), Encoding.GetEncoding(e.Args[1].AsInt())));
                }
            }
        }
    }
    class file_read_dataFunc : FunctionBase
    {
        public file_read_dataFunc()
        {
            this.Name = "file_read_data";
            this.MinimumArgCounts = 1;
            this.Run += File_read_textFunc_Run;
        }

        private void File_read_textFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(File.ReadAllBytes(e.Args[0].AsString()));
        }
    }
  
    class file_write_textFunc : FunctionBase
    {
        public file_write_textFunc()
        {
            this.Name = "file_write_text";
            this.MinimumArgCounts = 2;
            this.Run += File_write_textFunc_Run;
        }

        private void File_write_textFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count < 3)
            {
                File.WriteAllText(e.Args[0].AsString(), e.Args[1].AsString());
            }
            else
            {
                if (e.Args[1].Type == Variable.VarType.STRING)
                {
                    File.WriteAllText(e.Args[0].AsString(), e.Args[1].AsString(), Encoding.GetEncoding(e.Args[2].AsString()));
                }else if (e.Args[1].Type == Variable.VarType.NUMBER)
                {
                    File.WriteAllText(e.Args[0].AsString(), e.Args[1].AsString(), Encoding.GetEncoding(e.Args[2].AsInt()));
                }
            }
        }
    }
    class file_append_textFunc : FunctionBase
    {
        public file_append_textFunc()
        {
            this.Name = "file_append_text";
            this.MinimumArgCounts = 2;
            this.Run += File_write_textFunc_Run;
        }

        private void File_write_textFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count < 3)
            {
                File.AppendAllText(e.Args[0].AsString(), e.Args[1].AsString());
            }
            else
            {
                if (e.Args[1].Type == Variable.VarType.STRING)
                {
                    File.AppendAllText(e.Args[0].AsString(), e.Args[1].AsString(), Encoding.GetEncoding(e.Args[2].AsString()));
                }
                else if (e.Args[1].Type == Variable.VarType.NUMBER)
                {
                    File.AppendAllText(e.Args[0].AsString(), e.Args[1].AsString(), Encoding.GetEncoding(e.Args[2].AsInt()));
                }
            }
        }
    }
    class file_write_dataFunc : FunctionBase
    {
        public file_write_dataFunc()
        {
            this.Name = "file_write_data";
            this.MinimumArgCounts = 2;
            this.Run += File_write_textFunc_Run;
        }

        private void File_write_textFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            File.WriteAllBytes(e.Args[0].AsString(), e.Args[1].AsByteArray());
        }
    }
    class file_copyFunc : FunctionBase
    {
        public file_copyFunc()
        {
            this.Name = "file_copy";
            this.MinimumArgCounts = 2;
            this.Run += File_copyFunc_Run;
        }

        private void File_copyFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count < 3)
            {
                File.Copy(e.Args[0].AsString(),e.Args[1].AsString());
            }
            else
            {
                File.Copy(e.Args[0].AsString(), e.Args[1].AsString(),e.Args[2].AsBool());
            }
        }
    }
    class file_moveFunc : FunctionBase
    {
        public file_moveFunc()
        {
            this.Name = "file_move";
            this.MinimumArgCounts = 2;
            this.Run += File_copyFunc_Run;
        }

        private void File_copyFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            
                File.Move(e.Args[0].AsString(), e.Args[1].AsString());
           
        }
    }
    class file_existsFunc : FunctionBase
    {
        public file_existsFunc()
        {
            this.Name = "file_exists";
            this.MinimumArgCounts = 1;
            this.Run += File_exists_Run;
        }

        private void File_exists_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(File.Exists(e.Args[0].AsString()));
        }
    }
    class file_deleteFunc : FunctionBase
    {
        public file_deleteFunc()
        {
            this.Name = "file_delete";
            this.MinimumArgCounts = 1;
            this.Run += File_copyFunc_Run;
        }

        private void File_copyFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            
                File.Delete(e.Args[0].AsString());
            
           
        }
    }
    class file_encryptFunc:FunctionBase
    {
        public file_encryptFunc()
        {
            this.Name = "file_encrypt";
            this.MinimumArgCounts = 3;
            this.Run += File_encrypt_Run;
        }

        private void File_encrypt_Run(object sender, FunctionBaseEventArgs e)
        {
            FileEncrypter.FileEncrypt(e.Args[0].AsString(),e.Args[1].AsString(),e.Args[2].AsString());
        }
    }
    class file_decrypt : FunctionBase
    {
        public file_decrypt()
        {
            this.Name = "file_decrypt";
            this.MinimumArgCounts = 3;
            this.Run += File_encrypt_Run;
        }

        private void File_encrypt_Run(object sender, FunctionBaseEventArgs e)
        {
            FileEncrypter.FileDecrypt(e.Args[0].AsString(), e.Args[1].AsString(), e.Args[2].AsString());
        }
    }
    internal static class FileEncrypter
    {
        internal static bool FileDecrypt(string FilePath,string OutFilePath, string Password)
        {
            int len;
            byte[] buffer = new byte[4096];

           

            using (FileStream outfs = new FileStream(OutFilePath, FileMode.Create, FileAccess.Write))
            {
                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    using (AesManaged aes = new AesManaged())
                    {
                        aes.BlockSize = 128;              // BlockSize = 16bytes
                        aes.KeySize = 128;                // KeySize = 16bytes
                        aes.Mode = CipherMode.CBC;        // CBC mode
                        aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                        // salt
                        byte[] salt = new byte[16];
                        fs.Read(salt, 0, 16);

                        // Initilization Vector
                        byte[] iv = new byte[16];
                        fs.Read(iv, 0, 16);
                        aes.IV = iv;

                        /*
                        // パスワード文字列が大きい場合は、切り詰め、16バイトに満たない場合は0で埋めます
                        byte[] bufferKey = new byte[16];
                        byte[] bufferPassword = Encoding.UTF8.GetBytes(Password);
                        for (i = 0; i < bufferKey.Length; i++)
                        {
                            if (i < bufferPassword.Length)
                            {
                                bufferKey[i] = bufferPassword[i];
                            }
                            else
                            {
                                bufferKey[i] = 0;
                            }
                        */

                        // ivをsaltにしてパスワードを擬似乱数に変換
                        Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(Password, salt);
                        byte[] bufferKey = deriveBytes.GetBytes(16);    // 16バイトのsaltを切り出してパスワードに変換
                        aes.Key = bufferKey;

                        //Decryption interface.
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                        using (CryptoStream cse = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                        {
                            using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Decompress))   //解凍
                            {
                                while ((len = ds.Read(buffer, 0, 4096)) > 0)
                                {
                                    outfs.Write(buffer, 0, len);
                                }
                            }
                        }
                    }
                }
            }
           
            return (true);
        }

        internal static bool FileEncrypt(string FilePath, string OutFilePath, string Password)
        {

            int len;
            byte[] buffer = new byte[4096];



            using (FileStream outfs = new FileStream(OutFilePath, FileMode.Create, FileAccess.Write))
            {
                using (AesManaged aes = new AesManaged())
                {
                    aes.BlockSize = 128;              // BlockSize = 16bytes
                    aes.KeySize = 128;                // KeySize = 16bytes
                    aes.Mode = CipherMode.CBC;        // CBC mode
                    aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                    //入力されたパスワードをベースに擬似乱数を新たに生成
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(Password, 16);
                    byte[] salt = new byte[16]; // Rfc2898DeriveBytesが内部生成したなソルトを取得
                    salt = deriveBytes.Salt;
                    // 生成した擬似乱数から16バイト切り出したデータをパスワードにする
                    byte[] bufferKey = deriveBytes.GetBytes(16);

                    /*
                    // パスワード文字列が大きい場合は、切り詰め、16バイトに満たない場合は0で埋めます
                    byte[] bufferKey = new byte[16];
                    byte[] bufferPassword = Encoding.UTF8.GetBytes(Password);
                    for (i = 0; i < bufferKey.Length; i++)
                    {
                        if (i < bufferPassword.Length)
                        {
                            bufferKey[i] = bufferPassword[i];
                        }
                        else
                        {
                            bufferKey[i] = 0;
                        }
                    */

                    aes.Key = bufferKey;
                    // IV ( Initilization Vector ) は、AesManagedにつくらせる
                    aes.GenerateIV();

                    //Encryption interface.
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (CryptoStream cse = new CryptoStream(outfs, encryptor, CryptoStreamMode.Write))
                    {
                        outfs.Write(salt, 0, 16);     // salt をファイル先頭に埋め込む
                        outfs.Write(aes.IV, 0, 16); // 次にIVもファイルに埋め込む
                        using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Compress)) //圧縮
                        {
                            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                            {
                                while ((len = fs.Read(buffer, 0, 4096)) > 0)
                                {
                                    ds.Write(buffer, 0, len);
                                }
                            }
                        }

                    }

                }
            }


            return (true);
        }
    }
    class directory_createFunc : FunctionBase
    {
        public directory_createFunc()
        {
            this.Name = "directory_create";
            this.MinimumArgCounts = 1;
            this.Run += Directory_create_Run;
        }

        private void Directory_create_Run(object sender, FunctionBaseEventArgs e)
        {
            Directory.CreateDirectory(e.Args[0].AsString());
        }
    }
    class directory_deleteFunc : FunctionBase
    {
        public directory_deleteFunc()
        {
            this.Name = "directory_delete";
            this.MinimumArgCounts = 1;
            this.Run += Directory_create_Run;
        }

        private void Directory_create_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 1)
            {
                Directory.Delete(e.Args[0].AsString(),e.Args[1].AsBool());
            }
            else
            {
                Directory.Delete(e.Args[0].AsString());
            }
        }
    }
    class directory_moveFunc : FunctionBase
    {
        public directory_moveFunc()
        {
            this.Name = "directory_move";
            this.MinimumArgCounts = 2;
            this.Run += File_copyFunc_Run;
        }

        private void File_copyFunc_Run(object sender, FunctionBaseEventArgs e)
        {
           
                Directory.Move(e.Args[0].AsString(), e.Args[1].AsString());
           
        }
    }
    class directory_existsFunc : FunctionBase
    {
        public directory_existsFunc()
        {
            this.Name = "directory_exists";
            this.MinimumArgCounts = 1;
            this.Run += File_exists_Run;
        }

        private void File_exists_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Directory.Exists(e.Args[0].AsString()));
        }
    }
    class directory_currentdirectoryFunc : FunctionBase
    {
        public directory_currentdirectoryFunc()
        {
            this.Name = "directory_current";
            this.MinimumArgCounts = 0;
            this.Run += File_exists_Run;
        }

        private void File_exists_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0)
            {
                Directory.SetCurrentDirectory(e.Args[0].AsString());
            }
            e.Return = new Variable(e.Script.PWD);
        }
    }
    class directory_getdirectoriesFunc : FunctionBase
    {
        public directory_getdirectoriesFunc()
        {
            this.Name = "directory_getdirectories";
            this.MinimumArgCounts = 1;
            this.Run += Directory_getdirectoriesFunc_Run;
        }

        private void Directory_getdirectoriesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 1)
            {
                Variable vb = new Variable(Variable.VarType.ARRAY_STR);
                foreach(string dn in Directory.GetDirectories(e.Args[0].AsString()))
                {
                    vb.Tuple.Add(new Variable(dn));
                }
                e.Return = vb;
            }else if (e.Args.Count == 2)
            {
                Variable vb = new Variable(Variable.VarType.ARRAY_STR);
                foreach (string dn in Directory.GetDirectories(e.Args[0].AsString(),e.Args[1].AsString()))
                {
                    vb.Tuple.Add(new Variable(dn));
                }
                e.Return = vb;
            }
            else if (e.Args.Count >= 3)
            {
                Variable vb = new Variable(Variable.VarType.ARRAY_STR);
                SearchOption so = SearchOption.TopDirectoryOnly;
                if (e.Args[2].AsBool())
                {
                    so = SearchOption.AllDirectories;
                }
                foreach (string dn in Directory.GetDirectories(e.Args[0].AsString(), e.Args[1].AsString(),so))
                {
                    vb.Tuple.Add(new Variable(dn));
                }
                e.Return = vb;
            }
        }
    }
    class directory_getfilesFunc : FunctionBase
    {
        public directory_getfilesFunc()
        {
            this.Name = "directory_getfiles";
            this.MinimumArgCounts = 1;
            this.Run += Directory_getdirectoriesFunc_Run;
        }

        private void Directory_getdirectoriesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 1)
            {
                Variable vb = new Variable(Variable.VarType.ARRAY_STR);
                foreach (string dn in Directory.GetFiles(e.Args[0].AsString()))
                {
                    vb.Tuple.Add(new Variable(dn));
                }
                e.Return = vb;
            }
            else if (e.Args.Count == 2)
            {
                Variable vb = new Variable(Variable.VarType.ARRAY_STR);
                foreach (string dn in Directory.GetFiles(e.Args[0].AsString(), e.Args[1].AsString()))
                {
                    vb.Tuple.Add(new Variable(dn));
                }
                e.Return = vb;
            }
            else if (e.Args.Count >= 3)
            {
                Variable vb = new Variable(Variable.VarType.ARRAY_STR);
                SearchOption so = SearchOption.TopDirectoryOnly;
                if (e.Args[2].AsBool())
                {
                    so = SearchOption.AllDirectories;
                }
                foreach (string dn in Directory.GetFiles(e.Args[0].AsString(), e.Args[1].AsString(), so))
                {
                    vb.Tuple.Add(new Variable(dn));
                }
                e.Return = vb;
            }
        }
    }
    class directory_getdirectoryrootFunc : FunctionBase
    {
        public directory_getdirectoryrootFunc()
        {
            this.Name = "directory_getdirectoryroot";
            this.MinimumArgCounts = 1;
            this.Run += File_exists_Run;
        }

        private void File_exists_Run(object sender, FunctionBaseEventArgs e)
        {
           
            e.Return = new Variable(Directory.GetDirectoryRoot(e.Args[0].AsString()));
        }
    }
    class directory_copyFunc : FunctionBase
    {
        public directory_copyFunc()
        {
            this.Name = "directory_copy";
            this.MinimumArgCounts = 2;
            this.Run += Directory_copyFunc_Run;
        }

        private void Directory_copyFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            DirectoryCopy(e.Args[0].AsString(),e.Args[1].AsString(),(Utils.GetSafeInt(e.Args,2,1)==1));
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new ArgumentException(sourceDirName + " directory doesn't exist");
            }
            if (sourceDirName.Equals(destDirName, StringComparison.InvariantCultureIgnoreCase))
            {
                //throw new ArgumentException(sourceDirName + ": directories are same");
                string addPath = Path.GetFileName(sourceDirName);
                destDirName = Path.Combine(destDirName, addPath);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                File.Copy(file.FullName, tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
