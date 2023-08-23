namespace AliceScript.NameSpaces
{
    internal sealed class  Alice_Packaging_Initer
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Packaging");

            space.Add(new Package_CreateFromZipFileFunc());
            space.Add(new Package_GetManifestFromXmlFunc());

            space.Add(new PackageManifestObject());

            NameSpaceManerger.Add(space);
        }
    }
    internal sealed class AlicePackageObject : ObjectBase
    {
        public AlicePackageObject(AlicePackage package)
        {
            this.Package = package;
            this.Name = "AlicePackage";
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Manifest));
        }
        public AlicePackage Package { get; set; }
        private class AlicePackageObjectProperty : PropertyBase
        {
            public AlicePackageObjectProperty(AlicePackageObject host, AlicePackageObjectPropertyMode mode)
            {
                Host = host;
                Mode = mode;
                this.Name = Mode.ToString();
                this.HandleEvents = true;
                this.CanSet = false;
                this.Getting += AlicePackageObjectProperty_Getting;
            }

            private void AlicePackageObjectProperty_Getting(object sender, PropertyGettingEventArgs e)
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
            this.Name = "PackageManifest";
            Manifest = manifest;
            this.Constructor = new AlicePackageObjectConstractor();
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Name));
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Version));
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Description));
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Publisher));
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.ScriptPath));
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Script));
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.UseInlineScript));
        }
        public PackageManifestObject()
        {
            this.Name = "PackageManifest";
            this.Constructor = new AlicePackageObjectConstractor();
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Name));
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Version));
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Description));
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Publisher));
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.ScriptPath));
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Script));
            this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.UseInlineScript));
        }
        public PackageManifest Manifest { get; set; }
        private class AlicePackageObjectConstractor : FunctionBase
        {
            public AlicePackageObjectConstractor()
            {
                this.MinimumArgCounts = 1;
                this.Run += AlicePackageObjectConstractor_Run;
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
                this.Name = Mode.ToString();
                this.HandleEvents = true;
                this.CanSet = false;
                this.Getting += AlicePackageObjectProperty_Getting;
            }

            private void AlicePackageObjectProperty_Getting(object sender, PropertyGettingEventArgs e)
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
            this.Name = "Package_GetManifestFromXml";
            this.MinimumArgCounts = 1;
            this.Run += Interpreter_GetManifestFromXmlFunc_Run;
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
            this.Name = "Package_CreateFromZipFile";
            this.MinimumArgCounts = 2;
            this.Run += Package_CreateFromZipFileFunc_Run;
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