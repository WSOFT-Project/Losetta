using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Packaging;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Packaging
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Packaging");

            space.Add(new Package_CreateFromZipFileFunc());
            space.Add(new Package_GetManifestFromXmlFunc());

            space.Add(new PackageManifestObject());

            NameSpaceManager.Add(space);
        }
    }
    internal sealed class AlicePackageObject : ObjectBase
    {
        public AlicePackageObject(AlicePackage package)
        {
            Package = package;
            Name = "AlicePackage";
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Manifest));
        }
        public AlicePackage Package { get; set; }
        private class AlicePackageObjectProperty : PropertyBase
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

            private void AlicePackageObjectProperty_Getting(object sender, PropertyBaseEventArgs e)
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
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Name));
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Version));
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Description));
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Publisher));
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.ScriptPath));
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Script));
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.UseInlineScript));
        }
        public PackageManifestObject()
        {
            Name = "PackageManifest";
            Constructor = new AlicePackageObjectConstractor();
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Name));
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Version));
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Description));
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Publisher));
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.ScriptPath));
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Script));
            AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.UseInlineScript));
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
        private class AlicePackageObjectProperty : PropertyBase
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

            private void AlicePackageObjectProperty_Getting(object sender, PropertyBaseEventArgs e)
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
    internal sealed class Package_GetManifestFromXmlFunc : FunctionBase
    {
        public Package_GetManifestFromXmlFunc()
        {
            Name = "Package_GetManifestFromXml";
            MinimumArgCounts = 1;
            Run += Interpreter_GetManifestFromXmlFunc_Run;
        }

        private void Interpreter_GetManifestFromXmlFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(new PackageManifestObject(AlicePackage.GetManifest(e.Args[0].AsString())));
        }
    }
    internal sealed class Package_CreateFromZipFileFunc : FunctionBase
    {
        public Package_CreateFromZipFileFunc()
        {
            Name = "Package_CreateFromZipFile";
            MinimumArgCounts = 2;
            Run += Package_CreateFromZipFileFunc_Run;
        }

        private void Package_CreateFromZipFileFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            byte[] controlCode = null;
            if (e.Args.Count > 2 && e.Args[2].Type == Variable.VarType.BYTES)
            {
                controlCode = e.Args[2].ByteArray;
            }
            AlicePackage.CreateEncodingPackage(e.Args[0].AsString(), e.Args[1].AsString(), controlCode);
        }
    }
}