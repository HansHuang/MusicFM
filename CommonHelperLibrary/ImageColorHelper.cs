using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonHelperLibrary
{
    public static class ImageColorHelper
    {
        #region Const Variables
        /// <summary>
        /// Threshold for judge two diff color (regard as same color if less this value)
        /// </summary>
        public static readonly double ColorDiffOffset = 0.05;

        /// <summary>
        /// Brightness adjustment offset
        /// </summary>
        public static readonly double BrighterOffset = .1;

        /// <summary>
        ///Precent of get color from ri
        /// </summary>
        public static readonly double RightSideWidth = .1;

        /// <summary>
        ///Threshold for dark color
        /// </summary>
        public static readonly double DarkValue = .15;

        /// <summary>
        /// Threshold for dark color
        /// </summary>
        public static readonly double BrightValue = .4;

        /// <summary>
        /// HUE offset
        /// </summary>
        public static readonly double HueOffset = 10;

        /// <summary>
        /// Threshold for low staturation
        /// </summary>
        public static readonly double LowSaturation = .4;

        /// <summary>
        ///Threshold for judge staturation is close to zero
        /// </summary>
        public static readonly double AlmostZeroSaturation = .001;

        /// <summary>
        /// The color of human face
        /// </summary>
        public static readonly Color FaceColor = Color.FromRgb(0xCD, 0xA3, 0x8F);

        /// <summary>
        /// Min Weight value
        /// </summary>
        public static readonly double MinWeight = .2; 
        #endregion

        public static void GetTopicColorForImageAsync(BitmapSource image, Action<Color> callback)
        {
            Task.Run(() =>
            {
                //Make sure bitmap can use in new thread
                var bitmap = new FormatConvertedBitmap(image, PixelFormats.Rgb24, BitmapPalettes.WebPalette, 0);
                if (bitmap.CanFreeze) bitmap.Freeze();
                callback(GetTopicColorForImage(bitmap));
            });
        }

        /// <summary>
        /// get top color from image
        /// </summary>
        /// <param name="bitmap">BitmapSource</param>
        public static Color GetTopicColorForImage(BitmapSource bitmap)
        {
            if (bitmap == null) return Colors.White;

            const int bytesPerPixel = 3;

            if (bitmap.CanFreeze) bitmap.Freeze();
            var pixels = new byte[bitmap.PixelHeight * bitmap.PixelWidth * bytesPerPixel];
            bitmap.CopyPixels(pixels, bitmap.PixelWidth * bytesPerPixel, 0);
            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            var color = GetColorOfRegion(pixels, width, height, 0, width, 0, height);

            var hsl = new HslColor(color);
            if (IsNotSaturateEnough(hsl) && !IsAlmostZeroSaturation(hsl))
                hsl.Saturation += 0.2;

            return Revise(hsl).ToRgb();
        }


        /// <summary>
        /// Get topic color form specified region
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="top">The top.</param>
        /// <param name="bottom">The bottom.</param>
        /// <param name="forceDisableColorWeight">if set to <c>true</c> [force disable color weight].</param>
        /// <param name="removeFaceColor">remove human face color</param>
        /// <returns></returns>
        private static Color GetColorOfRegion(byte[] pixels, int width, int height, int left, int right, int top,
            int bottom, bool forceDisableColorWeight = false, bool removeFaceColor = true)
        {
            const int bytesPerPixel = 3;
            double sr = 0, sg = 0, sb = 0;
            double totalweight = 0;
            for (var i = top; i < bottom; i++)
            {
                for (var j = left; j < right; j++)
                {
                    var r = pixels[(i * width + j) * bytesPerPixel + 0];
                    var g = pixels[(i * width + j) * bytesPerPixel + 1];
                    var b = pixels[(i * width + j) * bytesPerPixel + 2];
                    double weight;
                    if (!forceDisableColorWeight)
                    {
                        var color = Color.FromRgb(r, g, b);
                        var hslColor = new HslColor(color);
                        weight = (1 - Math.Abs(1 - 2 * hslColor.Lightness)) * hslColor.Saturation;
                        if (weight < MinWeight)
                            weight = 0;
                        if (removeFaceColor)
                        {
                            var difference = Math.Abs(new HslColor(FaceColor).Hue - hslColor.Hue) / 360;
                            if (difference <= ColorDiffOffset)
                                weight = 0;
                            else
                                weight = weight * difference;
                        }
                    }
                    else
                        weight = 1;
                    totalweight += weight;
                    sr += r * weight;
                    sg += g * weight;
                    sb += b * weight;
                }
            }

            if (totalweight <= 0)
            {
                if (removeFaceColor)
                {
                    //当去除人脸色彩后总权重为0时，禁用去除人脸色彩
                    return GetColorOfRegion(pixels, width, height, left, right, top, bottom, false, false);
                }
                //纯灰度图片不能使用权重
                var newColor = GetColorOfRegion(pixels, width, height, left, right, top, bottom, true);
                var newHslColor = new HslColor(newColor) { Saturation = 0 };
                return newHslColor.ToRgb();
            }
            sr = sr / totalweight;
            sg = sg / totalweight;
            sb = sb / totalweight;
            return Color.FromRgb((byte)sr, (byte)sg, (byte)sb);
        }
        /// <summary>
        /// two color is different or not
        /// </summary>
        /// <param name="c1">color1</param>
        /// <param name="c2">color2</param>
        /// <returns>Boolean值</returns>
        private static bool IsDiffColor(HslColor c1, HslColor c2)
        {
            return Difference(c1, c2) > ColorDiffOffset;
            //return Math.Abs((c1.R - c2.R) * (c1.R - c2.R) + (c1.G - c2.G) * (c1.G - c2.G) + (c1.B - c2.B) * (c1.B - c2.B)) > CoverColorDiff;
        }
        /// <summary>
        /// 计算两种颜色的差异。0为无差异，1为差异最大值
        /// </summary>
        /// <param name="c1">颜色1</param>
        /// <param name="c2">颜色2</param>
        /// <returns>差异值</returns>
        private static double Difference(HslColor c1, HslColor c2)
        {
            return Difference(c1.ToRgb(), c2.ToRgb());
        }
        /// <summary>
        /// 计算两种颜色的差异。0为无差异，1为差异最大值
        /// </summary>
        /// <param name="c1">颜色1</param>
        /// <param name="c2">颜色2</param>
        /// <param name="compareAlpha">是否比较Alpha通道</param>
        /// <returns>
        /// 差异值
        /// </returns>
        private static double Difference(Color c1, Color c2, bool compareAlpha = true)
        {
            if (compareAlpha)
            {
                return
                    Math.Sqrt((Math.Pow(c1.A - c2.A, 2) + Math.Pow(c1.R - c2.R, 2) + Math.Pow(c1.G - c2.G, 2) +
                               Math.Pow(c1.B - c2.B, 2)) / 4 / 255 / 255);
            }
            return Math.Sqrt((Math.Pow(c1.R - c2.R, 2) + Math.Pow(c1.G - c2.G, 2) + Math.Pow(c1.B - c2.B, 2)) / 3 / 255 / 255);
        }
        /// <summary>
        /// 颜色饱和度是否太低
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>Boolean值</returns>
        private static bool IsNotSaturateEnough(HslColor color)
        {
            return color.Saturation < LowSaturation;
        }
        /// <summary>
        /// 颜色饱和度是否接近0
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>Boolean值</returns>
        private static bool IsAlmostZeroSaturation(HslColor color)
        {
            return color.Saturation < AlmostZeroSaturation;
        }
        /// <summary>
        /// 颜色是否太暗
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>Boolean值</returns>
        private static bool IsTooDark(HslColor color)
        {
            return color.Lightness < DarkValue;
        }
        /// <summary>
        /// 颜色是否太亮
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>Boolean值</returns>
        private static bool IsTooBright(HslColor color)
        {
            return color.Lightness > BrightValue;
        }
        /// <summary>
        /// 反色
        /// </summary>
        /// <param name="color">原色</param>
        /// <returns>反色</returns>
        private static HslColor Reverse(HslColor color)
        {
            var rgb = color.ToRgb();
            return new HslColor(Color.FromArgb(rgb.A, (byte)(255 - rgb.R), (byte)(255 - rgb.G), (byte)(255 - rgb.B)));
            //return new HSLColor(hsvColor.Alpha, hsvColor.Hue + 180, 1 - hsvColor.Saturation, 1 - hsvColor.Lightness);
        }
        /// <summary>
        /// 颜色修正
        /// </summary>
        /// <param name="color1">待修正色</param>
        /// <param name="color2">参照色</param>
        /// <returns>修正色</returns>
        private static HslColor Revise(HslColor color1, HslColor color2)
        {
            var newcolor = new HslColor(color1.ToRgb());
            while (IsTooBright(newcolor) || !IsDiffColor(newcolor, color2) && !IsTooDark(newcolor) && newcolor.Lightness > 0)
                newcolor = ReviseDarker(newcolor);
            if (!IsTooDark(newcolor)) return newcolor;
            newcolor = ReviseBrighter(color1);
            while (IsTooDark(newcolor) || !IsDiffColor(newcolor, color2) && !IsTooBright(newcolor) && newcolor.Lightness < 1)
                newcolor = ReviseBrighter(newcolor);
            if (!IsTooBright(newcolor)) return newcolor;
            if (IsTooBright(color1))
                return ReviseVeryBright(color1);
            if (IsTooDark(color1))
                return ReviseVeryDark(color1);
            return color1;

        }
        /// <summary>
        /// 无参照色时的颜色修正
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <returns>修正色</returns>
        private static HslColor Revise(HslColor color)
        {
            if (IsTooDark(color))
                return ReviseBrighter(color);
            if (IsTooBright(color))
                return ReviseVeryBright(color);
            //color = ReviseDarker(color);
            //if (IsTooDark(color))
            //return ReviseVeryDark(color);
            return color;
        }
        /// <summary>
        /// 将颜色调整到能够接受的最高亮度
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <returns>修正色</returns>
        private static HslColor ReviseVeryBright(HslColor color)
        {
            return ReviseBrighter(color, BrightValue - color.Lightness);
        }
        /// <summary>
        /// 将颜色调整到能够接受的最低亮度
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <returns>修正色</returns>
        private static HslColor ReviseVeryDark(HslColor color)
        {
            return ReviseDarker(color, color.Lightness - DarkValue);
        }
        /// <summary>
        /// 将颜色调亮特定亮度
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <param name="brigher">调整的亮度</param>
        /// <returns>修正色</returns>
        private static HslColor ReviseBrighter(HslColor color, double brigher)
        {
            return new HslColor(color.Alpha, color.Hue, color.Saturation, color.Lightness + brigher);
            //return Color.FromRgb(ReviseByteBigger(hsvColor.R), ReviseByteBigger(hsvColor.G), ReviseByteBigger(hsvColor.B));
        }
        /// <summary>
        /// 将颜色调亮一些
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <returns>修正色</returns>
        private static HslColor ReviseBrighter(HslColor color)
        {
            return ReviseBrighter(color, BrighterOffset);
        }
        /// <summary>
        /// 将颜色调暗特定亮度
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <param name="darker">调整的亮度</param>
        /// <returns>修正色</returns>
        private static HslColor ReviseDarker(HslColor color, double darker)
        {
            return new HslColor(color.Alpha, color.Hue, color.Saturation, color.Lightness - darker);
        }
        /// <summary>
        /// 将颜色调暗一些
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <returns>修正色</returns>
        private static HslColor ReviseDarker(HslColor color)
        {
            return ReviseDarker(color, BrighterOffset);
        }
    }

    public class HslColor
    {
        public double A, H, S, L;

        /// <summary>
        /// 不透明度。范围：0~1，1为不透明，0为透明
        /// </summary>
        public double Alpha
        {
            get
            {
                return A;
            }
            set
            {
                if (value > 1)
                    A = 1;
                else if (value < 0)
                    A = 0;
                else A = value;
            }
        }

        /// <summary>
        /// 色相。范围：0~359.9999999。特殊颜色：红：0    黄：60    绿：120   青：180   蓝：240   洋红：300
        /// </summary>
        public double Hue
        {
            get
            {
                return H;
            }
            set
            {
                if (value >= 360)
                    H = value % 360;
                else if (value < 0)
                    H = value - Math.Floor(value / 360) * 360;
                else H = value;
            }
        }
        /// <summary>
        /// 饱和度。范围：0~1。亮度0.5时，0为灰色，1为彩色
        /// </summary>
        public double Saturation
        {
            get
            {
                return S;
            }
            set
            {
                if (value > 1)
                    S = 1;
                else if (value < 0)
                    S = 0;
                else S = value;
            }
        }

        /// <summary>
        /// 亮度。范围：0~1。0为黑色，0.5为彩色，1为白色
        /// </summary>
        public double Lightness
        {
            get
            {
                return L;
            }
            set
            {
                if (value > 1)
                    L = 1;
                else if (value < 0)
                    L = 0;
                else L = value;
            }
        }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public HslColor()
            : this(1, 0, 0, 0)
        {
        }
        /// <summary>
        /// HSL构造函数
        /// </summary>
        /// <param name="hue">色相</param>
        /// <param name="saturation">饱和度</param>
        /// <param name="lightness">亮度</param>
        public HslColor(double hue, double saturation, double lightness)
            : this(1, hue, saturation, lightness)
        {
        }
        /// <summary>
        /// AHSL构造函数
        /// </summary>
        /// <param name="alpha">Alpha通道</param>
        /// <param name="hue">色相</param>
        /// <param name="saturation">饱和度</param>
        /// <param name="lightness">亮度</param>
        public HslColor(double alpha, double hue, double saturation, double lightness)
        {
            Alpha = alpha;
            Hue = hue;
            Saturation = saturation;
            Lightness = lightness;
        }
        /// <summary>
        /// 由RGB颜色类Color构造一个HSLColor的实例
        /// </summary>
        /// <param name="color">RGB颜色</param>
        public HslColor(Color color)
        {
            FromRgb(color);
        }
        /// <summary>
        /// RGB色彩空间转换
        /// </summary>
        /// <param name="color">RGB颜色</param>
        public void FromRgb(Color color)
        {
            A = (double)color.A / 255;
            var r = (double)color.R / 255;
            var g = (double)color.G / 255;
            var b = (double)color.B / 255;

            var min = Math.Min(r, Math.Min(g, b));
            var max = Math.Max(r, Math.Max(g, b));
            var distance = max - min;

            L = (max + min) / 2;
            S = 0;
            if (!(distance > 0)) return;
            S = L < 0.5 ? distance / (max + min) : distance / ((2 - max) - min);
            var tempR = (((max - r) / 6) + (distance / 2)) / distance;
            var tempG = (((max - g) / 6) + (distance / 2)) / distance;
            var tempB = (((max - b) / 6) + (distance / 2)) / distance;
            double hT;
            if (r == max)
                hT = tempB - tempG;
            else if (g == max)
                hT = (1.0 / 3 + tempR) - tempB;
            else
                hT = (2.0 / 3 + tempG) - tempR;
            if (hT < 0)
                hT += 1;
            if (hT > 1)
                hT -= 1;
            H = hT * 360;
        }
        /// <summary>
        /// 转换到RGB色彩空间
        /// </summary>
        /// <param name="hsl">HSL颜色</param>
        /// <returns>转换后的RGB颜色</returns>
        public static Color ToRgb(HslColor hsl)
        {
            byte a = (byte)Math.Round(hsl.Alpha * 255), r, g, b;
            if (hsl.Saturation == 0)
            {
                r = (byte)Math.Round(hsl.Lightness * 255);
                g = r;
                b = r;
            }
            else
            {
                var vH = hsl.Hue / 360;
                var v2 = hsl.Lightness < 0.5 ? hsl.Lightness * (1 + hsl.Saturation) : (hsl.Lightness + hsl.Saturation) - (hsl.Lightness * hsl.Saturation);
                var v1 = 2 * hsl.Lightness - v2;
                r = (byte)Math.Round(255 * HueToRgb(v1, v2, vH + 1.0 / 3));
                g = (byte)Math.Round(255 * HueToRgb(v1, v2, vH));
                b = (byte)Math.Round(255 * HueToRgb(v1, v2, vH - 1.0 / 3));
            }
            return Color.FromArgb(a, r, g, b);
        }
        /// <summary>
        ///Hue to rgb
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="vH"></param>
        /// <returns></returns>
        public static double HueToRgb(double v1, double v2, double vH)
        {
            if (vH < 0) vH += 1;
            if (vH > 1) vH -= 1;
            if (6 * vH < 1) return v1 + ((v2 - v1) * 6 * vH);
            if (2 * vH < 1) return v2;
            if (3 * vH < 2) return v1 + (v2 - v1) * (2.0 / 3 - vH) * 6;
            return v1;
        }

        public Color ToRgb()
        {
            return ToRgb(this);
        }

        public override string ToString()
        {
            return ToRgb().ToString();
        }
    }
}