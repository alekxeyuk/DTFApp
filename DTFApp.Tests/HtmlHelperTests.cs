using DTFApp.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DTFApp.Tests
{
    [TestClass]
    public class HtmlHelperTests
    {
        [TestMethod]
        public void StripHtml_RemovesTags()
        {
            Assert.AreEqual("hello world", HtmlHelper.StripHtml("<b>hello</b> <i>world</i>"));
        }

        [TestMethod]
        public void StripHtml_NullOrEmpty_ReturnsEmpty()
        {
            Assert.AreEqual("", HtmlHelper.StripHtml(null));
            Assert.AreEqual("", HtmlHelper.StripHtml(""));
        }

        [TestMethod]
        public void ExtractTag_ParsesAnchorHref()
        {
            string html = "text <a href=\"https://example.com/x\">link</a>";
            int i = 0;

            var tag = HtmlHelper.ExtractTag(html, ref i);

            Assert.IsTrue(tag.HasValue);
            Assert.AreEqual("a", tag.Value.Name);
            Assert.IsFalse(tag.Value.IsClosing);
            Assert.AreEqual("https://example.com/x", tag.Value.Href);
        }

        [TestMethod]
        public void ExtractTag_DetectsClosingTag()
        {
            string html = "</b>";
            int i = 0;

            var tag = HtmlHelper.ExtractTag(html, ref i);

            Assert.IsTrue(tag.HasValue);
            Assert.AreEqual("b", tag.Value.Name);
            Assert.IsTrue(tag.Value.IsClosing);
        }
    }
}
