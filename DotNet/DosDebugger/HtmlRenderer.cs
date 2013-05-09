using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace DosDebugger
{
    class HtmlRenderer
    {
        string html;
        TextComponent[] components;
        Font normalFont;
        Font hoverFont;

        public HtmlRenderer(string html, Font normalFont, Font hoverFont)
        {
            this.html = html;
            this.FormatFlags =
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine |
                TextFormatFlags.PreserveGraphicsClipping |
                //TextFormatFlags.GlyphOverhangPadding |
                //TextFormatFlags.ExternalLeading |
                TextFormatFlags.VerticalCenter;
            this.normalFont = normalFont;
            this.hoverFont = hoverFont;
            this.components = GetTextComponents(html);
        }

        public TextFormatFlags FormatFlags { get; private set; }

        class TextComponent
        {
            public string Text;
            public string Url;
            public Rectangle Bounds;
        }

        struct HtmlAttribute
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        class HtmlElement
        {
            public string Name { get; set; }
            public HtmlAttribute[] Attributes { get; set; }
            public string Text { get; set; }
        }

#if false
        private static Regex rgxElement = new Regex(
            @"^<([a-z]+)[^a-z].*?>([^<]*)</\1>",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
#endif

        private static HtmlElement ParseElement(string html, int startIndex)
        {
            if (html == null)
                throw new ArgumentNullException("html");
            if (startIndex < 0 || startIndex >= html.Length)
                throw new ArgumentOutOfRangeException("startIndex");
            if (html[startIndex] != '<')
                throw new ArgumentException();

            int endIndex = html.IndexOf('>', startIndex);
            if (endIndex < 0)
                return null;

            string[] parts = html.Substring(startIndex, endIndex - startIndex - 1).Split(' ');
            if (parts.Length == 0)
                return null;

            HtmlElement element = new HtmlElement();
            element.Name = parts[0];
            element.Attributes = new HtmlAttribute[parts.Length - 1];
            for (int i = 1; i < parts.Length; i++)
            {
                HtmlAttribute attr = new HtmlAttribute();
                string s = parts[i];

                int k = s.IndexOf('=');
                if (k >= 0)
                    attr.Name = s.Substring(0, k);
                else
                    attr.Name = "";

                if (k + 2 < s.Length && s[k + 1] == '"' && s[s.Length - 1] == '"')
                    attr.Value = s.Substring(k + 2, s.Length - k - 2);
                else
                    attr.Value = s.Substring(k + 1);

                element.Attributes[i] = attr;
            }
            return element;
        }

        // Supported syntax:
        // <a href="...">label</a>
        private static TextComponent[] GetTextComponents(string html)
        {
            // Short-cut for plain text.
            if (html.IndexOf('<') == -1)
            {
                return new TextComponent[] { new TextComponent { Text = html } };
            }

            // Parse HTML.
#if false
            for (int index = 0; index < html.Length; )
            {
                int k = 
            }

            int k1 = html.IndexOf('<');
            int k2 = html.IndexOf('>');
            if (k1 >= 0 && k2 > k1)
            {

            }
            else
            {
            }
#else
            List<TextComponent> components = new List<TextComponent>(10);
            TextComponent current = null;
            html = "<html>" + html + "</html>";

            using (StringReader input = new StringReader(html))
            using (XmlReader reader = XmlReader.Create(input))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "html")
                                break;
                            if (reader.Name != "a")
                                throw new NotSupportedException("Only <a> is supported.");
                            if (current != null && current.Url != null)
                                throw new NotSupportedException("Nested element is not supported.");

                            if (current != null)
                                components.Add(current);
                            current = new TextComponent();

                            if (reader.HasAttributes)
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "href")
                                        current.Url = reader.Value;
                                }
                            }
                            if (current.Url == null)
                                throw new NotSupportedException("Missing href attribute.");

                            break;

                        case XmlNodeType.EndElement:
                            if (reader.Name == "html")
                                break;
                            components.Add(current);
                            current = null;
                            break;

                        case XmlNodeType.Text:
                            if (current == null)
                            {
                                components.Add(new TextComponent { Text = reader.Value });
                            }
                            else
                            {
                                current.Text = reader.Value;
                            }
                            break;
                    }
                }
            }
#endif
            return components.ToArray();
        }

        public void Measure(Graphics g, Rectangle bounds)
        {
            if (g == null)
                throw new ArgumentNullException("g");
            if (components.Length == 0)
                return;

            // Shrink the bounding rectangle to exclude the leading and
            // trailing paddings when rendering text.
            int padding = GetDefaultTextPadding(g, normalFont);
            bounds = new Rectangle(
                bounds.Left + padding,
                bounds.Top,
                bounds.Width - padding * 2,
                bounds.Height);

            foreach (var component in components)
            {
                Size size = TextRenderer.MeasureText(
                    g, component.Text, normalFont, bounds.Size, FormatFlags);

                component.Bounds = new Rectangle(
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

        public bool HitTest(Point pt)
        {
            foreach (var component in components)
            {
                if (component.Url != null && component.Bounds.Contains(pt))
                    return true;
            }
            return false;
        }

        public bool Draw(Graphics g, Point cursorPosition)
        {
            if (g == null)
                throw new ArgumentNullException("g");

            bool hovering = false;
            foreach (var component in components)
            {
                if (component.Bounds.Width > 0)
                {
                    if (component.Url == null) // normal text
                    {
                        TextRenderer.DrawText(
                        g,
                        component.Text,
                        normalFont,
                        component.Bounds,
                        Color.Black,
                        FormatFlags);
                    }
                    else // hyperlink
                    {
                        bool b = component.Bounds.Contains(cursorPosition);
                        TextRenderer.DrawText(
                            g,
                            component.Text,
                            b? hoverFont : normalFont,
                            component.Bounds,
                            Color.Blue,
                            FormatFlags);
                        hovering |= b;
                    }
                }
            }
            return hovering;
        }

        /// <summary>
        /// Gets the amount of padding (in pixels) used on each side when a
        /// Windows control renders text in a bounding rectangle.
        /// </summary>
        /// <returns></returns>
        public static int GetDefaultTextPadding(Graphics g, Font font)
        {
            Size szNull = new Size(int.MaxValue, int.MaxValue);
            Size szWithPadding = TextRenderer.MeasureText(
                g, "A", font, szNull, TextFormatFlags.LeftAndRightPadding);
            Size szWithoutPadding = TextRenderer.MeasureText(
                g, "A", font, szNull, TextFormatFlags.NoPadding);
            int padding = (szWithPadding.Width - szWithoutPadding.Width) / 2 - 1;
            return padding;
        }
    }
}
