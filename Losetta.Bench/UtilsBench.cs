using AliceScript;
using AliceScript.Functions;
using BenchmarkDotNet.Attributes;
using System.Security.Cryptography;

namespace Losetta.Bench
{
    public class UtilsBench
    {
        const string TEXT = "Lorem ipsum dolor sit amet Est duo in duis eos eros justo duis aliquyam aliquyam euismod takimata possim eos. Ipsum sanctus nostrud eleifend voluptua consetetur. Consectetuer dolor illum vero ipsum. Sanctus ut est veniam at sit sed clita amet diam wisi. Et ipsum nonumy duo duo stet no sit voluptua hendrerit.\r\n\r\nDolore consetetur clita volutpat eu diam magna lorem at euismod. Rebum at ea labore mazim consetetur veniam invidunt assum diam sea accumsan amet dolores et dolores justo. Dolor velit dolores tempor no clita ex. Erat lorem rebum et ut ut. Sea vero veniam lorem. Iusto sanctus consectetuer ea at velit nonumy justo dolores sit lorem et stet sanctus nonumy nonumy vero sed et. Et iusto sit vero eos et sanctus sanctus. Stet iriure at duis eirmod et dolor ipsum. In ut et nonummy duo sed esse consetetur gubergren facilisis sit sanctus qui vel id duo. Lorem facer nulla consetetur sit illum laoreet at nulla vero vero voluptua vero ut te ut eos. Kasd esse eirmod lorem diam lorem nulla voluptua sea feugiat. Consectetuer dolore eum autem elitr. Sanctus rebum nonumy magna sit sed liber nonumy consectetuer clita amet invidunt nibh vel odio nihil erat rebum commodo. Sit dolores eirmod ea. Dolor consetetur stet. Dolore sit aliquyam magna stet et est commodo eos rebum gubergren consequat ea possim sadipscing erat clita zzril feugait. Nonumy volutpat ut dolor labore voluptua in voluptua et diam feugiat esse ea gubergren justo ea accusam ipsum.\r\n\r\nEa amet sanctus. Eirmod tempor aliquyam eum. Invidunt dolor voluptua justo. Diam et clita et magna invidunt lorem veniam kasd et invidunt in ea sanctus vel consetetur. Zzril ipsum in sit consetetur veniam tempor vero eros eum te magna nonumy amet soluta. Ea justo lorem tempor eirmod elitr feugiat vel clita soluta diam stet iriure kasd sea kasd sit erat. Sed eros et diam et sea invidunt est justo esse molestie et rebum dolor nonummy sit molestie amet. Ipsum ut dolores sadipscing ea ullamcorper stet placerat dolor veniam takimata elit stet gubergren.\r\n\r\nTation sit sanctus adipiscing erat eu lorem elitr sit. Illum diam euismod invidunt nibh. Nibh feugiat vel lorem exerci consequat lorem eos dignissim. Facilisis at et lorem accusam nibh erat at accusam accusam amet duo sanctus dignissim dignissim elitr. Aliquyam sed kasd stet facilisis ut invidunt est ut ea facilisis clita vero sed labore kasd sed. Clita facilisis elitr ea sit eros et dolore exerci est diam dolores velit erat ut diam at. No et dolor nonumy magna. Invidunt facilisis est ea. Rebum et sanctus. Option tempor dolor lorem ipsum gubergren cum sit ipsum duis eleifenswnsfwnsfwd sed.";
        [Benchmark]
        public void NomalContains()
        {
            TEXT.Contains("nsfw");
        }

        [Benchmark]
        public void ComparisionContains()
        {
            TEXT.Contains("nsfw",StringComparison.Ordinal);
        }


    }

    internal static class Test
    {
        public static int Pow(int x)
        {
            return x * x;
        }
    }
    internal class PowFunc : FunctionBase
    {
        public PowFunc()
        {
            this.Name = "Pow";
            this.MinimumArgCounts = 1;
            this.MaximumArgCounts = 1;
            this.Run += PowFunc_Run;
        }

        private void PowFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            int x = Utils.GetSafeInt(e.Args, 0);
            e.Return = new Variable(x * x);
        }
    }
}
