using System;
using System.Drawing;

namespace FastReport.Utils
{
    /// <summary>
    /// Provides a data for paint event.
    /// </summary>
    public class FRPaintEventArgs
    {
        private readonly IGraphics graphics;
        private readonly float scaleX;
        private readonly float scaleY;
        private readonly GraphicCache cache;

        public static Func<Graphics, IGraphics> GraphicsAdapterFactory { get; set; } = _ => throw new PlatformNotSupportedException("System.Drawing-based Graphics adapter has been removed. Use image-based Skia adapters.");
        public static Func<Image, IGraphics> ImageAdapterFactory { get; set; } = image => SkiaBackedGraphics.FromImage(image);
        public static Func<IntPtr, IGraphics> WindowGraphicsAdapterFactory { get; set; } = _ => throw new PlatformNotSupportedException("Window handle graphics adapter is not supported in Skia-only mode.");

        public static IGraphics CreateGraphics(Graphics graphics)
        {
            return GraphicsAdapterFactory(graphics);
        }

        public static IGraphics CreateGraphics(Image image)
        {
            return ImageAdapterFactory(image);
        }

        public static IGraphics CreateGraphicsFromWindowHandle(IntPtr hwnd)
        {
            return WindowGraphicsAdapterFactory(hwnd);
        }

        public static bool TryGetNativeGraphics(IGraphics graphics, out Graphics nativeGraphics)
        {
            if (graphics != null)
                return graphics.TryGetNativeGraphics(out nativeGraphics);

            nativeGraphics = null;
            return false;
        }

        /// <summary>
        /// Gets a <b>Graphics</b> object to draw on.
        /// </summary>
        public IGraphics Graphics
        {
            get { return graphics; }
        }

        /// <summary>
        /// Gets the X scale factor.
        /// </summary>
        public float ScaleX
        {
            get { return scaleX; }
        }

        /// <summary>
        /// Gets the Y scale factor.
        /// </summary>
        public float ScaleY
        {
            get { return scaleY; }
        }

        /// <summary>
        /// Gets the cache that contains graphics objects.
        /// </summary>
        public GraphicCache Cache
        {
            get { return cache; }
        }

        /// <summary>
        /// Initializes a new instance of the <b>FRPaintEventArgs</b> class with specified settings.
        /// </summary>
        /// <param name="g"><b>IGraphicsRenderer</b> object to draw on.</param>
        /// <param name="scaleX">X scale factor.</param>
        /// <param name="scaleY">Y scale factor.</param>
        /// <param name="cache">Cache that contains graphics objects.</param>
        public FRPaintEventArgs(IGraphics g, float scaleX, float scaleY, GraphicCache cache)
        {
            graphics = g;
            this.scaleX = scaleX;
            this.scaleY = scaleY;
            this.cache = cache;
        }

        /// <summary>
        /// Initializes a new instance of the <b>FRPaintEventArgs</b> class with specified settings.
        /// </summary>
        /// <param name="g"><b>Graphics</b> object to draw on.</param>
        /// <param name="scaleX">X scale factor.</param>
        /// <param name="scaleY">Y scale factor.</param>
        /// <param name="cache">Cache that contains graphics objects.</param>
        public FRPaintEventArgs(Graphics g, float scaleX, float scaleY, GraphicCache cache) :
            this(CreateGraphics(g), scaleX, scaleY, cache)
        {
        }
    }

}
