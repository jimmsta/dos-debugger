using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WpfDebugger
{
    public static class HtmlConverter
    {
        public static readonly DependencyProperty InnerHtmlProperty =
            DependencyProperty.RegisterAttached("InnerHtml",
            typeof(string),
            typeof(HtmlConverter),
            new UIPropertyMetadata("", InnerHtmlChanged));

        public static string GetInnerHtml(TextBlock obj)
        {
            return (string)obj.GetValue(InnerHtmlProperty);
        }

        public static void SetInnerHtml(TextBlock obj, string value)
        {
            obj.SetValue(InnerHtmlProperty, value);
        }

        private static void InnerHtmlChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            string html = e.NewValue as string;
            TextBlock textBlock = sender as TextBlock;

            if (textBlock != null && html != null)
            {
                textBlock.Inlines.Clear();
                textBlock.Inlines.Add(ConvertHtmlToInlines(html));
            }
        }

        /// <summary>
        /// Parses an html string to an InlineCollection. The opening tags
        /// and closing tags MUST match, otherwise the result is undefined.
        /// </summary>
        /// <param name="html"></param>
        /// <remarks>
        /// The following HTML tags are supported:
        ///   a -> Hyperlink
        ///   b -> Bold
        ///   i -> Italic
        ///   u -> Underline
        ///   br -> LineBreak (close tag optional)
        ///   (plain text) -> Run
        /// 
        /// The following WPF elements that are valid child elements of Span
        /// are not supported by this method:
        ///   Figure, Floater, InlineUIContainer.
        /// </remarks>
        /// <returns></returns>
        private static Inline ConvertHtmlToInlines(string html)
        {
            if (html == null)
                throw new ArgumentNullException("html");

            // Maintain a stack of the top Span element. For example,
            // <b><a href="somewhere">click me</a> if you <i>want</i>.</b>
            // would push an element to the stack for each open tag, and
            // pop an element from the stack for each close tag.
            Stack<Span> elementStack = new Stack<Span>();
            Span top = new Span();
            elementStack.Push(top);

            for (int index = 0; index < html.Length; )
            {
                int k1 = html.IndexOf('<', index);
                if (k1 == -1) // all text
                {
                    top.Inlines.Add(new Run(html.Substring(index)));
                    break;
                }
                if (k1 > index) // at least some text
                {
                    top.Inlines.Add(new Run(html.Substring(index, k1 - index)));
                    index = k1;
                }

                // Now 'index' points to '<'. Search for '>'.
                int k2 = html.IndexOf('>', index + 1);
                if (k2 == -1) // '<' without '>'
                {
                    top.Inlines.Add(new Run(html.Substring(index)));
                    break;
                }
                
                string tagString = html.Substring(k1 + 1, k2 - k1 - 1);
                HtmlElement tag = HtmlElement.Parse(tagString);
                string tagName = (tag != null) ? tag.Name.ToLowerInvariant() : null;

                if (tagName == null) // parse failed; output as is
                {
                    top.Inlines.Add(new Run(html.Substring(k1, k2 - k1 + 1)));
                }
                else if (tagName != "" && tagName[0] == '/') // close tag
                {
                    if (tagName == "/a" && top is Hyperlink ||
                        tagName == "/b" && top is Bold ||
                        tagName == "/i" && top is Italic ||
                        tagName == "/u" && top is Underline)
                    {
                        elementStack.Pop();
                        top = elementStack.Peek();
                    }
                    else
                    {
                        // unmatched close tag; output as is.
                        top.Inlines.Add(new Run(html.Substring(k1, k2 - k1 + 1)));
                    }
                }
                else // open tag or open-close tag (e.g. <br/>)
                {
                    Inline element = null;
                    switch (tagName)
                    {
                        case "a":
                            {
                                Hyperlink hyperlink = new Hyperlink();
                                if (tag.Attributes != null)
                                {
                                    foreach (HtmlAttribute attr in tag.Attributes)
                                    {
                                        if (attr.Name == "href")
                                            hyperlink.NavigateUri = new Uri(attr.Value, UriKind.RelativeOrAbsolute);
                                    }
                                }
                                element = hyperlink;
                            }
                            break;
                        case "b":
                            element = new Bold();
                            break;
                        case "i":
                            element = new Italic();
                            break;
                        case "u":
                            element = new Underline();
                            break;
                        case "br":
                            break;
                    }

                    if (element != null) // supported element
                    {
                        top.Inlines.Add(element);
                        if (element is Span &&   // not br
                            html[k2 - 1] != '/') // not self-closed tag
                        {
                            elementStack.Push((Span)element);
                            top = (Span)element;
                        }
                    }
                    else // unsupported element, treat as text
                    {
                        top.Inlines.Add(new Run(html.Substring(k1, k2 - k1 + 1)));
                    }
                }

                index = k2 + 1;
            }

            // Return the root element. Note that some open tags may not be
            // closed, but we ignore that.
            while (elementStack.Count > 0)
            {
                top = elementStack.Pop();
            }
            return top; // .Inlines;
        }
#if false
            Hyperlink hyperlink = new Hyperlink(new Run("Haha"));
            hyperlink.NavigateUri = new Uri("http://www.microsoft.com");
            hyperlink.Inlines
            yield return hyperlink;
            yield return new Run(html);
#endif
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

#if false
        public static HtmlElement Parse(string s)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            HtmlElement element = new HtmlElement();

            // Find element name.
            int i = 0;
            while (i < s.Length && Char.IsLetterOrDigit(s[i]))
            {
                i++;
            }
            element.Name = s.Substring(0, i);

        }

        Regex rgxTagName = new Regex(
            @"[a-zA-Z0-9]+(\s+[a-zA-Z0-9]+\s*=""",
            RegexOptions.Singleline | RegexOptions.CultureInvariant);

        Regex rgxAttribute = new Regex(
            @"\s+([a-zA-Z0-9]+)=(""[.*?]""|[^""\s]+",
            RegexOptions.Singleline | RegexOptions.CultureInvariant);
#else
        public static HtmlElement Parse(string s)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (s.Length == 0)
                return null;

            HtmlElement element = new HtmlElement();

            // Find element name.
            int i = (s[0] == '/') ? 1 : 0;
            while (i < s.Length && Char.IsLetterOrDigit(s[i]))
            {
                i++;
            }
            element.Name = s.Substring(0, i);

            // Shortcut if there are no attributes.
            if (i == s.Length)
                return element;

            // Parse attributes.
            List<HtmlAttribute> attrs = new List<HtmlAttribute>(4);
            while (i < s.Length)
            {
                HtmlAttribute attr = new HtmlAttribute();

                // Skip blanks.
                while (i < s.Length && char.IsWhiteSpace(s[i]))
                    i++;
                if (i == s.Length)
                    break;

                // Find '='.
                int k1 = s.IndexOf('=', i);
                if (k1 < 0)
                    return null;

                // Strip name on the left side of '='.
                attr.Name = s.Substring(i, k1 - i);

                // The next char must be '"'.
                k1++;
                if (k1 >= s.Length || s[k1] != '"')
                    return null;

                // Find closing '"'.
                int k2 = s.IndexOf('"', k1 + 1);
                if (k2 == -1)
                    return null;
                attr.Value = s.Substring(k1 + 1, k2 - k1 - 1);

                attrs.Add(attr);
                i = k2 + 1;
            }

            element.Attributes = attrs.ToArray();
            return element;
        }
#endif
    }
}
