using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Packaging;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Packaging
    {
        public static void Init()
        {
            Alice.RegisterFunctions<PackagingFunctions>();

            NameSpace space = new NameSpace("Alice.Packaging");
            space.Add(new PackageManifestObject());
            NameSpaceManager.Add(space);
        }
    }
    [AliceNameSpace(Name = "Alice.Packaging")]
    internal sealed class PackagingFunctions
    {
        public static void Package_CreateFromZipFile(string filepath, string outFilepath, byte[] controlCode = null, bool minify = false)
        {
            AlicePackage.CreateEncodingPackage(filepath, outFilepath, controlCode, minify);
        }
        public static PackageManifestObject Package_GetManifestFromXml(string xml)
        {
            return new PackageManifestObject(AlicePackage.GetManifest(xml));
        }
    }
    internal sealed class AlicePackageObject : ObjectBase
    {
        public AlicePackageObject(AlicePackage package)
        {
            Package = package;
            Name = "AlicePackage";
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Manifest));
        }
        public AlicePackage Package { get; set; }
        private class AlicePackageObjectProperty : ValueFunction
        {
            public AlicePackageObjectProperty(AlicePackageObject host, AlicePackageObjectPropertyMode mode)
            {
                Host = host;
                Mode = mode;
                Name = Mode.ToString();
                HandleEvents = true;
                CanSet = false;
                Getting += AlicePackageObjectProperty_Getting;
            }

            private void AlicePackageObjectProperty_Getting(object sender, ValueFunctionEventArgs e)
            {
                switch (Mode)
                {
                    case AlicePackageObjectPropertyMode.Manifest:
                        {
                            e.Value = new Variable(new PackageManifestObject(Host.Package.Manifest));
                            break;
                        }
                }
            }
            public enum AlicePackageObjectPropertyMode
            {
                Manifest
            }
            public AlicePackageObjectPropertyMode Mode { get; set; }
            public AlicePackageObject Host { get; set; }
        }
    }
    internal sealed class PackageManifestObject : ObjectBase
    {
        public PackageManifestObject(PackageManifest manifest)
        {
            Name = "PackageManifest";
            Manifest = manifest;
            Constructor = new AlicePackageObjectConstractor();
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Name));
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Version));
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Description));
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Publisher));
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.ScriptPath));
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Script));
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.UseInlineScript));
        }
        public PackageManifestObject()
        {
            Name = "PackageManifest";
            Constructor = new AlicePackageObjectConstractor();
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Name));
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Version));
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Description));
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Publisher));
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.ScriptPath));
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Script));
            AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.UseInlineScript));
        }
        public PackageManifest Manifest { get; set; }
        private class AlicePackageObjectConstractor : FunctionBase
        {
            public AlicePackageObjectConstractor()
            {
                MinimumArgCounts = 1;
                Run += AlicePackageObjectConstractor_Run;
            }

            private void AlicePackageObjectConstractor_Run(object sender, FunctionBaseEventArgs e)
            {
                var m = AlicePackage.GetManifest(e.Args[0].AsString());
                if (m != null)
                {
                    e.Return = new Variable(new PackageManifestObject(m));
                }
            }
        }
        private class AlicePackageObjectProperty : ValueFunction
        {
            public AlicePackageObjectProperty(PackageManifestObject host, AlicePackageObjectPropertyMode mode)
            {
                Host = host;
                Mode = mode;
                Name = Mode.ToString();
                HandleEvents = true;
                CanSet = false;
                Getting += AlicePackageObjectProperty_Getting;
            }

            private void AlicePackageObjectProperty_Getting(object sender, ValueFunctionEventArgs e)
            {
                switch (Mode)
                {
                    case AlicePackageObjectPropertyMode.Name:
                        {
                            e.Value = new Variable(Host.Manifest.Name);
                            break;
                        }
                    case AlicePackageObjectPropertyMode.Version:
                        {
                            e.Value = new Variable(Host.Manifest.Version);
                            break;
                        }
                    case AlicePackageObjectPropertyMode.Description:
                        {
                            e.Value = new Variable(Host.Manifest.Description);
                            break;
                        }
                    case AlicePackageObjectPropertyMode.Publisher:
                        {
                            e.Value = new Variable(Host.Manifest.Publisher);
                            break;
                        }
                    case AlicePackageObjectPropertyMode.Target:
                        {
                            e.Value = new Variable(Host.Manifest.Target);
                            break;
                        }
                    case AlicePackageObjectPropertyMode.ScriptPath:
                        {
                            e.Value = new Variable(Host.Manifest.ScriptPath);
                            break;
                        }
                    case AlicePackageObjectPropertyMode.Script:
                        {
                            e.Value = new Variable(Host.Manifest.Script);
                            break;
                        }
                    case AlicePackageObjectPropertyMode.UseInlineScript:
                        {
                            e.Value = new Variable(Host.Manifest.UseInlineScript);
                            break;
                        }
                }
            }

            public enum AlicePackageObjectPropertyMode
            {
                Name, Version, Description, Publisher, Target, ScriptPath, Script, UseInlineScript
            }
            public AlicePackageObjectPropertyMode Mode { get; set; }
            public PackageManifestObject Host { get; set; }
        }
    }
}