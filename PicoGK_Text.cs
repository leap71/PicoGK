using System.Reflection;
using SkiaSharp;

namespace PicoGK
{
	public class Text
	{
        static public SKTypeface oDefaultTypeface
        {
            get
            {
                lock (g_oTFLock) // Be thread safe
                {
                    if (g_oTypeFace is null)
                    {
                        try
                        {
                            // Try to load from embedded resource (when in Nuget mode)
                            using Stream? oStream = typeof(Library).Assembly.GetManifestResourceStream("PicoGK.Resources.font.ttf");

                            if (oStream is null)
                            {
                                // Load font from disk
                                g_oTypeFace = SKTypeface.FromFile(Path.Combine(Utils.strPicoGKSourceCodeFolder(), "assets", "Jost.ttf"));
                            }
                            else
                            {
                                // Load font from embedded resource
                                g_oTypeFace = SKTypeface.FromStream(oStream);
                            }
                        }
                            
                        catch
                        {
                            // No font available, default to SkiaSharp's font
                            g_oTypeFace = SKTypeface.CreateDefault();
                        }
                    }

                    return g_oTypeFace;
                }
            }
        }

        static public ImageRgba32 imgRenderText(    string strText,
                                                    int nFontHeight,
                                                    int nPadding                = 10,
                                                    ColorFloat? _clrBackground = null,
                                                    ColorFloat? _clrText       = null,
                                                    SKTypeface? _oTypeface     = null)
        {
            ColorFloat  clrBackground = _clrBackground ?? new("FF");
            ColorFloat  clrText       = _clrText       ?? new("00");
            SKTypeface  oTypeface     = _oTypeface     ?? oDefaultTypeface;

            using SKFont oFont = new(oTypeface, nFontHeight);

            using SKPaint oPaint = new SKPaint
            {
                IsAntialias = true,
                Color       = clrText.oAsSkColor()
            };

            // Measure text bounds using SKFont
            oFont.MeasureText(strText, out SKRect oBounds);

            int nWidth  = (int)(oBounds.Width + 0.5f) + 2 * nPadding;
            int nHeight = (int)(oBounds.Height + 0.5f) + 2 * nPadding;

            using SKBitmap oSkBitmap = new(nWidth, nHeight);
            using SKCanvas oSkCanvas = new(oSkBitmap);

            oSkCanvas.Clear(clrBackground.oAsSkColor());

            float fX = nPadding - oBounds.Left;
            float fY = nPadding - oBounds.Top;

            oSkCanvas.DrawText(strText, fX, fY, SKTextAlign.Left, oFont, oPaint);

            return Image.imgFromSKBitmap(oSkBitmap);
        }

        static readonly object       g_oTFLock = new();
        static SKTypeface?  g_oTypeFace = null;
    }  
}

