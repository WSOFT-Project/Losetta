using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript.NameSpaces
{
    static class Alice_Drawing_Initer
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Drawing");

            space.Add(new ColorObject(0,0,0));
            //メモリ消費が激しすぎるため一旦Colorsオブジェクトを無効化
          //  space.Add(new ColorsObject());

            NameSpaceManerger.Add(space);
        }
    }
   
    class ColorObject : ObjectBase
    {
        public void init()
        {
            Name = "Color";

            this.AddProperty(new ColorValueProperty(this, 0));
            this.AddProperty(new ColorValueProperty(this, 1));
            this.AddProperty(new ColorValueProperty(this, 2));
            this.AddProperty(new ColorValueProperty(this, 3));
            this.AddProperty(new ColorValueProperty(this, 4));
            this.AddProperty(new ColorValueProperty(this, 5));
            this.AddProperty(new ColorValueProperty(this, 6));
            this.AddProperty(new ColorValueProperty(this, 7));

            this.AddFunction(new FromArgbFunc());
            this.AddFunction(new FromNameFunc());
            this.AddFunction(new ToArgbFunc(this));
            this.AddFunction(new ToNameFunc(this));
            
        }
        public ColorObject(int r, int g, int b, int a = 255)
        {
            Color = new Color();

            Color.FromArgb(a, r, g, b);
            
            init();

        }
        public ColorObject(Color c)
        {
            Color = c;
            init();
        }
        public Color Color;
        private class ColorValueProperty : PropertyBase
        {
            public ColorValueProperty(ColorObject host,int mode)
            {
                Host = host;
                Mode = mode;
                this.HandleEvents = true;
                
                switch (mode)
                {
                    case 0:
                        {
                            this.Name = "A";
                            break;
                        }
                    case 1:
                        {
                            this.Name = "R";
                            break;
                        }
                    case 2:
                        {
                            this.Name = "G";
                            break;
                        }
                    case 3:
                        {
                            this.Name = "B";
                            break;
                        }
                    case 4:
                        {
                            this.Name = "IsNamedColor";
                            break;
                        }
                    case 5:
                        {
                            //HSLの中の色相
                            this.Name = "Hue";
                            break;
                        }
                    case 6:
                        {
                            //HSLの中の彩度
                            this.Name = "Saturation";
                            break;
                        }
                    case 7:
                        {
                            //HSLの中の輝度
                            this.Name = "Brightness";
                            break;
                        }
                }
                this.Getting += ColorValueProperty_Getting;
            }

            private void ColorValueProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                switch (Mode)
                {
                    case 0:
                        {
                            e.Value = new Variable(Host.Color.A);
                            break;
                        }
                    case 1:
                        {
                            e.Value = new Variable(Host.Color.R);
                            break;
                        }
                    case 2:
                        {
                            e.Value = new Variable(Host.Color.G);
                            break;
                        }
                    case 3:
                        {
                            e.Value = new Variable(Host.Color.B);
                            break;
                        }
                    case 4:
                        {
                            e.Value = new Variable(Host.Color.IsNamedColor);
                            break;
                        }
                    case 5:
                        {
                            e.Value = new Variable(Host.Color.GetHue());
                            break;
                        }
                    case 6:
                        {
                            e.Value = new Variable(Host.Color.GetSaturation());
                            break;
                        }
                    case 7:
                        {
                            e.Value = new Variable(Host.Color.GetBrightness());
                            break;
                        }
                }
            }

            private ColorObject Host;
            private int Mode = 0;
        }
        private class FromArgbFunc : FunctionBase
        {
            public FromArgbFunc()
            {
                this.Name = "FromArgb";
                this.MinimumArgCounts = 1;
                this.Run += FromArgbFunc_Run;
            }

            private void FromArgbFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                switch (e.Args.Count)
                {
                    case 1:
                        {
                            e.Return = new Variable(new ColorObject(Color.FromArgb(e.Args[0].AsInt())));
                            break;
                        }
                    case 2:
                        {
                            e.Return = new Variable(new ColorObject(Color.FromArgb(e.Args[0].AsInt(),((ColorObject)e.Args[1].Object).Color)));
                            break;
                        }
                    case 3:
                        {
                            e.Return = new Variable(new ColorObject(Color.FromArgb(e.Args[0].AsInt(),e.Args[1].AsInt(),e.Args[2].AsInt())));
                            break;
                        }
                    case 4:
                        {
                            e.Return = new Variable(new ColorObject(Color.FromArgb(e.Args[0].AsInt(), e.Args[1].AsInt(), e.Args[2].AsInt(),e.Args[3].AsInt())));
                            break;
                        }
                }
            }
        }
        private class FromNameFunc : FunctionBase
        {
            public FromNameFunc()
            {
                this.Name = "FromName";
                this.MinimumArgCounts = 1;
                this.Run += FromNameFunc_Run;
            }

            private void FromNameFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                e.Return = new Variable(new ColorObject(Color.FromName(e.Args[0].AsString())));
            }
        }
        private class ToArgbFunc : FunctionBase
        {
            public ToArgbFunc(ColorObject host)
            {
                this.Name = "ToArgb";
                this.Host = host;
                this.Run += ToArgbFunc_Run;
            }
            private ColorObject Host;
            private void ToArgbFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                e.Return = new Variable(Host.Color.ToArgb());
            }
        }
        private class ToNameFunc : FunctionBase
        {
            public ToNameFunc(ColorObject host)
            {
                this.Name = "ToName";
                this.Host = host;
                this.Run += ToArgbFunc_Run;
            }
            private ColorObject Host;
            private void ToArgbFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                e.Return = new Variable(Host.Color.ToKnownColor().ToString());
            }
        }

    }
    public class ColorsObject : ObjectBase
    {

        public ColorsObject()
        {
            Name = "Colors";

            this.AddProperty(new ColorProperty(Color.AliceBlue));
            this.AddProperty(new ColorProperty(Color.AntiqueWhite));
            this.AddProperty(new ColorProperty(Color.Aqua));
            this.AddProperty(new ColorProperty(Color.Aquamarine));
            this.AddProperty(new ColorProperty(Color.Azure));
            this.AddProperty(new ColorProperty(Color.Beige));
            this.AddProperty(new ColorProperty(Color.Bisque));
            this.AddProperty(new ColorProperty(Color.Black));
            this.AddProperty(new ColorProperty(Color.BlanchedAlmond));
            this.AddProperty(new ColorProperty(Color.Blue));
            this.AddProperty(new ColorProperty(Color.BlueViolet));
            this.AddProperty(new ColorProperty(Color.Brown));
            this.AddProperty(new ColorProperty(Color.BurlyWood));
            this.AddProperty(new ColorProperty(Color.CadetBlue));
            this.AddProperty(new ColorProperty(Color.Chartreuse));
            this.AddProperty(new ColorProperty(Color.Chocolate));
            this.AddProperty(new ColorProperty(Color.Coral));
            this.AddProperty(new ColorProperty(Color.CornflowerBlue));
            this.AddProperty(new ColorProperty(Color.Cornsilk));
            this.AddProperty(new ColorProperty(Color.Crimson));
            this.AddProperty(new ColorProperty(Color.Cyan));
            this.AddProperty(new ColorProperty(Color.DarkBlue));
            this.AddProperty(new ColorProperty(Color.DarkCyan));
            this.AddProperty(new ColorProperty(Color.DarkGoldenrod));
            this.AddProperty(new ColorProperty(Color.DarkGray));
            this.AddProperty(new ColorProperty(Color.DarkGreen));
            this.AddProperty(new ColorProperty(Color.DarkKhaki));
            this.AddProperty(new ColorProperty(Color.DarkMagenta));
            this.AddProperty(new ColorProperty(Color.DarkOliveGreen));
            this.AddProperty(new ColorProperty(Color.DarkOrange));
            this.AddProperty(new ColorProperty(Color.DarkOrchid));
            this.AddProperty(new ColorProperty(Color.DarkRed));
            this.AddProperty(new ColorProperty(Color.DarkSalmon));
            this.AddProperty(new ColorProperty(Color.DarkSeaGreen));
            this.AddProperty(new ColorProperty(Color.DarkSlateBlue));
            this.AddProperty(new ColorProperty(Color.DarkSlateGray));
            this.AddProperty(new ColorProperty(Color.DarkTurquoise));
            this.AddProperty(new ColorProperty(Color.DarkViolet));
            this.AddProperty(new ColorProperty(Color.DeepPink));
            this.AddProperty(new ColorProperty(Color.DeepSkyBlue));
            this.AddProperty(new ColorProperty(Color.DimGray));
            this.AddProperty(new ColorProperty(Color.DodgerBlue));
            this.AddProperty(new ColorProperty(Color.Firebrick));
            this.AddProperty(new ColorProperty(Color.DarkSeaGreen));
            this.AddProperty(new ColorProperty(Color.ForestGreen));
            this.AddProperty(new ColorProperty(Color.Fuchsia));
            this.AddProperty(new ColorProperty(Color.Gainsboro));
            this.AddProperty(new ColorProperty(Color.GhostWhite));
            this.AddProperty(new ColorProperty(Color.Gold));
            this.AddProperty(new ColorProperty(Color.DarkSeaGreen));
            this.AddProperty(new ColorProperty(Color.Gray));
            this.AddProperty(new ColorProperty(Color.Green));
            this.AddProperty(new ColorProperty(Color.GreenYellow));
            this.AddProperty(new ColorProperty(Color.HotPink));
            this.AddProperty(new ColorProperty(Color.IndianRed));
            this.AddProperty(new ColorProperty(Color.Indigo));
            this.AddProperty(new ColorProperty(Color.Ivory));
            this.AddProperty(new ColorProperty(Color.Khaki));
            this.AddProperty(new ColorProperty(Color.Lavender));
            this.AddProperty(new ColorProperty(Color.LavenderBlush));
            this.AddProperty(new ColorProperty(Color.HotPink));
            this.AddProperty(new ColorProperty(Color.LemonChiffon));
            this.AddProperty(new ColorProperty(Color.LightBlue));
            this.AddProperty(new ColorProperty(Color.LightCoral));
            this.AddProperty(new ColorProperty(Color.LightCyan));
            this.AddProperty(new ColorProperty(Color.LightGoldenrodYellow));
            this.AddProperty(new ColorProperty(Color.LightGray));
            this.AddProperty(new ColorProperty(Color.LightGreen));
            this.AddProperty(new ColorProperty(Color.LightPink));
            this.AddProperty(new ColorProperty(Color.LightSalmon));
            this.AddProperty(new ColorProperty(Color.LightSeaGreen));
            this.AddProperty(new ColorProperty(Color.LightSkyBlue));
            this.AddProperty(new ColorProperty(Color.LightSlateGray));
            this.AddProperty(new ColorProperty(Color.LightSteelBlue));
            this.AddProperty(new ColorProperty(Color.LightYellow));
            this.AddProperty(new ColorProperty(Color.Lime));
            this.AddProperty(new ColorProperty(Color.LimeGreen));
            this.AddProperty(new ColorProperty(Color.Linen));
            this.AddProperty(new ColorProperty(Color.Magenta));
            this.AddProperty(new ColorProperty(Color.Maroon));
            this.AddProperty(new ColorProperty(Color.MediumAquamarine));
            this.AddProperty(new ColorProperty(Color.MediumBlue));
            this.AddProperty(new ColorProperty(Color.MediumOrchid));
            this.AddProperty(new ColorProperty(Color.MediumSeaGreen));
            this.AddProperty(new ColorProperty(Color.MediumSlateBlue));
            this.AddProperty(new ColorProperty(Color.MediumSpringGreen));
            this.AddProperty(new ColorProperty(Color.MediumTurquoise));
            this.AddProperty(new ColorProperty(Color.MediumVioletRed));
            this.AddProperty(new ColorProperty(Color.MidnightBlue));
            this.AddProperty(new ColorProperty(Color.MintCream));
            this.AddProperty(new ColorProperty(Color.MistyRose));
            this.AddProperty(new ColorProperty(Color.Moccasin));
            this.AddProperty(new ColorProperty(Color.NavajoWhite));
            this.AddProperty(new ColorProperty(Color.Navy));
            this.AddProperty(new ColorProperty(Color.OldLace));
            this.AddProperty(new ColorProperty(Color.Olive));
            this.AddProperty(new ColorProperty(Color.OliveDrab));
            this.AddProperty(new ColorProperty(Color.Orange));
            this.AddProperty(new ColorProperty(Color.Orchid));
            this.AddProperty(new ColorProperty(Color.OrangeRed));
            this.AddProperty(new ColorProperty(Color.PaleGoldenrod));
            this.AddProperty(new ColorProperty(Color.PaleGreen));
            this.AddProperty(new ColorProperty(Color.PaleTurquoise));
            this.AddProperty(new ColorProperty(Color.PaleVioletRed));
            this.AddProperty(new ColorProperty(Color.PapayaWhip));
            this.AddProperty(new ColorProperty(Color.PeachPuff));
            this.AddProperty(new ColorProperty(Color.Peru));
            this.AddProperty(new ColorProperty(Color.Pink));
            this.AddProperty(new ColorProperty(Color.Plum));
            this.AddProperty(new ColorProperty(Color.PowderBlue));
            this.AddProperty(new ColorProperty(Color.Purple));
            this.AddProperty(new ColorProperty(Color.Red));
            this.AddProperty(new ColorProperty(Color.RosyBrown));
            this.AddProperty(new ColorProperty(Color.RoyalBlue));
            this.AddProperty(new ColorProperty(Color.SaddleBrown));
            this.AddProperty(new ColorProperty(Color.Salmon));
            this.AddProperty(new ColorProperty(Color.SandyBrown));
            this.AddProperty(new ColorProperty(Color.SeaGreen));
            this.AddProperty(new ColorProperty(Color.SeaShell));
            this.AddProperty(new ColorProperty(Color.Sienna));
            this.AddProperty(new ColorProperty(Color.Silver));
            this.AddProperty(new ColorProperty(Color.SkyBlue));
            this.AddProperty(new ColorProperty(Color.SlateBlue));
            this.AddProperty(new ColorProperty(Color.SlateGray));
            this.AddProperty(new ColorProperty(Color.Snow));
            this.AddProperty(new ColorProperty(Color.SpringGreen));
            this.AddProperty(new ColorProperty(Color.SteelBlue));
            this.AddProperty(new ColorProperty(Color.Tan));
            this.AddProperty(new ColorProperty(Color.Teal));
            this.AddProperty(new ColorProperty(Color.Thistle));
            this.AddProperty(new ColorProperty(Color.Tomato));
            this.AddProperty(new ColorProperty(Color.Transparent));
            this.AddProperty(new ColorProperty(Color.Turquoise));
            this.AddProperty(new ColorProperty(Color.Violet));
            this.AddProperty(new ColorProperty(Color.Wheat));
            this.AddProperty(new ColorProperty(Color.White));
            this.AddProperty(new ColorProperty(Color.WhiteSmoke));
            this.AddProperty(new ColorProperty(Color.Yellow));
            this.AddProperty(new ColorProperty(Color.YellowGreen));

            this.AddProperty(new RandomColorProperty());


        }
        private class ColorProperty : PropertyBase
        {
            public ColorProperty(Color color)
            {
                this.Name=color.ToKnownColor().ToString();
                this.Value = new Variable(new ColorObject(color));
                this.CanSet = false;
            }
        }
        private class RandomColorProperty : PropertyBase
        {
            Random r = new Random();
            public RandomColorProperty()
            {
                this.Name = "Random";
                this.CanSet = false;
                this.HandleEvents = true;
                this.Getting += RandomColorProperty_Getting;
            }

            private void RandomColorProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)));
            }
        }
       
      


    }
}
