using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace DosDebugger
{
    /// <summary>
    /// Provides methods to draw text on a WinForms control.
    /// </summary>
    public static class GraphicsExtensions
    {
        /// <summary>
        /// Gets the amount of padding (in pixels) used on each side when a
        /// Windows control renders text in a bounding rectangle.
        /// </summary>
        /// <returns></returns>
        public static int GetTextPadding(this Graphics g, Font font)
        {
            Size szNull = new Size(int.MaxValue, int.MaxValue);
            Size szWithPadding = TextRenderer.MeasureText(
                g, "A", font, szNull, TextFormatFlags.LeftAndRightPadding);
            Size szWithoutPadding = TextRenderer.MeasureText(
                g, "A", font, szNull, TextFormatFlags.NoPadding);
            int padding = (szWithPadding.Width - szWithoutPadding.Width) / 2 - 1;
            return padding;
        }

        /// <summary>
        /// Draws a string in exactly the same way a WinForms control draws
        /// it. This method is different from DrawString() in that
        /// DrawString() does not produce a consistent look with the system.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="s"></param>
        /// <param name="font"></param>
        /// <param name="bounds"></param>
        public static void DrawText(
            this Graphics g, string s, Font font, Rectangle bounds, Color color)
        {
            TextFormatFlags flags =
                TextFormatFlags.PreserveGraphicsClipping |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.LeftAndRightPadding |
                TextFormatFlags.SingleLine |
                TextFormatFlags.GlyphOverhangPadding |
                TextFormatFlags.ExternalLeading |
                TextFormatFlags.EndEllipsis;

            TextRenderer.DrawText(g, s, font, bounds, color, flags);
        }

        public static void MeasureTexts(
            this Graphics g, TextWithInfo[] texts, Rectangle bounds)
        {
            if (g == null)
                throw new ArgumentNullException("g");
            if (texts == null)
                throw new ArgumentNullException("texts");
            if (texts.Length == 0)
                return;

            // Shrink the bounding rectangle to exclude the leading and
            // trailing paddings when rendering text.
            int padding = g.GetTextPadding(texts[0].Font);
            bounds = new Rectangle(
                bounds.Left + padding,
                bounds.Top,
                bounds.Width - padding * 2,
                bounds.Height);

            TextFormatFlags flags =
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine |
                TextFormatFlags.PreserveGraphicsClipping |
                //TextFormatFlags.GlyphOverhangPadding |
                //TextFormatFlags.ExternalLeading |
                TextFormatFlags.VerticalCenter;

            for (int i = 0; i < texts.Length; i++)
            {
                Size size = TextRenderer.MeasureText(
                    g, texts[i].Text, texts[i].Font, bounds.Size, flags);

                texts[i].Bounds = new Rectangle(
                    bounds.X,
                    bounds.Y + (bounds.Height - size.Height) / 2,
                    Math.Min(size.Width, bounds.Width),
                    size.Height);

                bounds = new Rectangle(
                    bounds.X + size.Width,
                    bounds.Y,
                    Math.Max(0, bounds.Width - size.Width),
                    bounds.Height);
            }
        }

        public static void DrawTexts(this Graphics g, TextWithInfo[] texts)
        {
            if (g == null)
                throw new ArgumentNullException("g");
            if (texts == null)
                throw new ArgumentNullException("texts");

            TextFormatFlags flags =
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine |
                TextFormatFlags.PreserveGraphicsClipping |
                //TextFormatFlags.GlyphOverhangPadding |
                //TextFormatFlags.ExternalLeading |
                TextFormatFlags.VerticalCenter;

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].Bounds.Width > 0)
                {
                    TextRenderer.DrawText(g, texts[i].Text, texts[i].Font, texts[i].Bounds, texts[i].Color, flags);
                }
            }
        }
    }

    public class TextWithInfo
    {
        public string Text { get; set; }
        public Font Font { get; set; }
        public Color Color { get; set; }
        public Rectangle Bounds { get; set; }
    }
}
