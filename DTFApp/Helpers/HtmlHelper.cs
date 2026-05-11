using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Documents;

namespace DTFApp.Helpers
{
    public struct HtmlTag
    {
        public string Name { get; set; }
        public bool IsClosing { get; set; }
        public bool IsSelfClosing { get; set; }
        public string Href { get; set; }
    }

    public static class HtmlHelper
    {
        public static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            return Regex.Replace(html, "<[^>]*>", "");
        }

        public static HtmlTag? ExtractTag(string html, ref int i)
        {
            int start = html.IndexOf('<', i);
            if (start < 0) return null;

            int end = html.IndexOf('>', start);
            if (end < 0) return null;

            i = end + 1;
            var content = html.Substring(start + 1, end - start - 1).Trim();

            bool isClosing = content.StartsWith("/");
            if (isClosing) content = content.Substring(1).Trim();

            bool isSelfClosing = content.EndsWith("/");
            if (isSelfClosing) content = content.TrimEnd('/').TrimEnd();

            var parts = content.Split(new[] { ' ' }, 2);
            var name = parts[0].ToLowerInvariant();

            string href = null;
            if (name == "a" && parts.Length > 1)
            {
                var match = Regex.Match(parts[1], @"href=""([^""]+)""");
                if (match.Success) href = match.Groups[1].Value;
            }

            return new HtmlTag { Name = name, IsClosing = isClosing, IsSelfClosing = isSelfClosing, Href = href };
        }

        public static void ParseInlines(string html, ref int i, IList<Inline> output, string closeTag)
        {
            while (i < html.Length)
            {
                if (html[i] == '<')
                {
                    var tag = ExtractTag(html, ref i);
                    if (tag == null) { i++; continue; }

                    if (closeTag != null && tag.Value.IsClosing && tag.Value.Name == closeTag)
                        return;

                    switch (tag.Value.Name)
                    {
                        case "br":
                            output.Add(new LineBreak());
                            break;
                        case "b":
                        case "strong":
                            {
                                var bold = new Bold();
                                var sub = new List<Inline>();
                                ParseInlines(html, ref i, sub, tag.Value.Name);
                                foreach (var inline in sub)
                                    bold.Inlines.Add(inline);
                                output.Add(bold);
                                break;
                            }
                        case "i":
                        case "em":
                            {
                                var italic = new Italic();
                                var sub = new List<Inline>();
                                ParseInlines(html, ref i, sub, tag.Value.Name);
                                foreach (var inline in sub)
                                    italic.Inlines.Add(inline);
                                output.Add(italic);
                                break;
                            }
                        case "a":
                            {
                                var link = new Hyperlink();
                                if (tag.Value.Href != null)
                                    link.NavigateUri = new Uri(tag.Value.Href);
                                var sub = new List<Inline>();
                                ParseInlines(html, ref i, sub, tag.Value.Name);
                                foreach (var inline in sub)
                                    link.Inlines.Add(inline);
                                output.Add(link);
                                break;
                            }
                    }
                }
                else
                {
                    int next = html.IndexOf('<', i);
                    if (next < 0) next = html.Length;
                    if (closeTag != null)
                    {
                        var closePos = html.IndexOf("</" + closeTag + ">", i);
                        if (closePos >= 0 && closePos < next) next = closePos;
                    }
                    var text = html.Substring(i, next - i);
                    i = next;
                    if (!string.IsNullOrEmpty(text))
                    {
                        output.Add(new Run { Text = WebUtility.HtmlDecode(text) });
                    }
                }
            }
        }
    }
}
