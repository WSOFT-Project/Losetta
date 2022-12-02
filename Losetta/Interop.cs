using System;
using System.Collections.Generic;


namespace AliceScript.Interop
{
    internal class NetLibraryLoader
    {
        public static void LoadLibrary(string path)
        {
            try
            {
                string ipluginName = typeof(ILibrary).FullName;
                //アセンブリとして読み込む
                System.Reflection.Assembly asm =
                    System.Reflection.Assembly.LoadFrom(path);
                foreach (Type t in asm.GetTypes())
                {
                    try
                    {
                        //アセンブリ内のすべての型について、
                        //プラグインとして有効か調べる
                        if (t.IsClass && t.IsPublic && !t.IsAbstract &&
                            t.GetInterface(ipluginName) != null)
                        {
                            if (Loadeds.Contains(asm.GetHashCode()))
                            {
                                ThrowErrorManerger.OnThrowError("そのライブラリはすでに読み込まれています", Exceptions.LIBRARY_ALREADY_LOADED);
                            }
                            else
                            {
                                Loadeds.Add(asm.GetHashCode());
                                ((ILibrary)asm.CreateInstance(t.FullName)).Main();
                            }
                        }
                    }
                    catch(Exception ex) { ThrowErrorManerger.OnThrowError(ex.Message,Exceptions.LIBRARY_EXCEPTION); }
                }
            }
            catch (Exception ex) { ThrowErrorManerger.OnThrowError(ex.Message, Exceptions.LIBRARY_EXCEPTION); }

        }
        public static void LoadLibrary(byte[] rawassembly)
        {
                try
                {
                    string ipluginName = typeof(ILibrary).FullName;
                    //アセンブリとして読み込む
                    System.Reflection.Assembly asm =
                        System.Reflection.Assembly.Load(rawassembly);
                    foreach (Type t in asm.GetTypes())
                    {
                        try
                        {
                            //アセンブリ内のすべての型について、
                            //プラグインとして有効か調べる
                            if (t.IsClass && t.IsPublic && !t.IsAbstract &&
                                t.GetInterface(ipluginName) != null)
                            {
                                if (Loadeds.Contains(asm.GetHashCode()))
                                {
                                    ThrowErrorManerger.OnThrowError("そのライブラリはすでに読み込まれています", Exceptions.LIBRARY_ALREADY_LOADED);
                                }
                                else
                                {
                                    Loadeds.Add(asm.GetHashCode());
                                    ((ILibrary)asm.CreateInstance(t.FullName)).Main();
                                }
                            }
                        }
                        catch (Exception ex) { ThrowErrorManerger.OnThrowError(ex.Message, Exceptions.LIBRARY_EXCEPTION); }
                }
                }
                catch (Exception ex) { ThrowErrorManerger.OnThrowError(ex.Message, Exceptions.LIBRARY_EXCEPTION); }

        }
        private static List<int> Loadeds = new List<int>();
    }
    public static class GCManerger
    {
        public static bool CollectAfterExecute = false;
    }
    public interface ILibrary
    {
        string Name { get; }
        void Main();
    }
}
