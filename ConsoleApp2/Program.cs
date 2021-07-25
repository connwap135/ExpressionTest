using ImageMagick;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var logoPath = "c.png";
            var readSettings = new MagickReadSettings
            {
                Font = "msyh.ttf",
                TextGravity = Gravity.Center,
                BackgroundColor = MagickColors.Transparent,
                FillColor = MagickColors.White,
                FontPointsize = 54D,
                FontStyle = FontStyleType.Bold,
                Height = 90,
                Width = 90
            };
            using (var caption = new MagickImage($"caption:熊", readSettings))
            {
                using var shadow = new MagickImage("xc:none", 90, 90);
                shadow.Settings.FillColor =MagickColors.Orange;
                shadow.Draw(new DrawableCircle(45, 45, 76, 70));
                shadow.Blur(0, 5);
                caption.Composite(shadow, Gravity.Center, CompositeOperator.DstOver);
                caption.Write(logoPath, MagickFormat.Png8);
            }

        }
    }
}
