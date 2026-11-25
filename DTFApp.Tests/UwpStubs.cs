namespace Windows.UI.Xaml
{
    public enum Visibility { Visible = 0, Collapsed = 1 }

    public enum TextWrapping { NoWrap = 0, Wrap = 1 }

    public struct Thickness
    {
        public Thickness(double left, double top, double right, double bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }
        public double Left; public double Top; public double Right; public double Bottom;
    }

    public class UIElement
    {
        public Thickness Margin { get; set; }
    }
}

namespace Windows.UI.Xaml.Controls
{
    public class TextBlock : Windows.UI.Xaml.UIElement
    {
        public string Text { get; set; }
        public Windows.UI.Xaml.TextWrapping TextWrapping { get; set; }
    }
}

namespace Windows.UI.Xaml.Documents
{
    public class Inline { }

    public class InlineCollection : System.Collections.Generic.List<Inline> { }

    public class Span : Inline
    {
        public InlineCollection Inlines { get; } = new InlineCollection();
    }

    public class Bold : Span { }

    public class Italic : Span { }

    public class Hyperlink : Span
    {
        public System.Uri NavigateUri { get; set; }
    }

    public class LineBreak : Inline { }

    public class Run : Inline
    {
        public string Text { get; set; }
    }

    public class Paragraph
    {
        public InlineCollection Inlines { get; } = new InlineCollection();
    }
}

namespace Windows.Storage
{
    public enum CreationCollisionOption { ReplaceExisting }

    public interface IStorageFile { }

    public class StorageFile : IStorageFile { }

    public class StorageFolder
    {
        public System.Threading.Tasks.Task<StorageFile> GetFileAsync(string name) => System.Threading.Tasks.Task.FromResult<StorageFile>(null);
        public System.Threading.Tasks.Task<StorageFile> CreateFileAsync(string name, CreationCollisionOption option) => System.Threading.Tasks.Task.FromResult(new StorageFile());
    }

    public class ApplicationDataContainer
    {
        public System.Collections.Generic.IDictionary<string, object> Values { get; } = new System.Collections.Generic.Dictionary<string, object>();
    }

    public class ApplicationData
    {
        public static ApplicationData Current { get; } = new ApplicationData();
        public StorageFolder LocalFolder { get; } = new StorageFolder();
        public ApplicationDataContainer LocalSettings { get; } = new ApplicationDataContainer();
    }

    public static class FileIO
    {
        public static System.Threading.Tasks.Task<string> ReadTextAsync(IStorageFile file) => System.Threading.Tasks.Task.FromResult("");
        public static System.Threading.Tasks.Task WriteTextAsync(IStorageFile file, string contents) => System.Threading.Tasks.Task.CompletedTask;
    }
}
