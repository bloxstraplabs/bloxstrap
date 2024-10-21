using System.Windows.Input;

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

using Bloxstrap.UI.Elements.Base;
using Bloxstrap.UI.ViewModels.Editor;

namespace Bloxstrap.UI.Elements.Editor
{
    /// <summary>
    /// Interaction logic for BootstrapperEditorWindow.xaml
    /// </summary>
    public partial class BootstrapperEditorWindow : WpfUiWindow
    {
        private static class CustomBootstrapperSchema
        {
            private class Schema
            {
                public Dictionary<string, Element> Elements { get; set; } = new Dictionary<string, Element>();
                public Dictionary<string, Type> Types { get; set; } = new Dictionary<string, Type>();
            }

            private class Element
            {
                public string? SuperClass { get; set; } = null;
                public bool IsCreatable { get; set; } = false;

                // [AttributeName] = [TypeName]
                public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
            }

            public class Type
            {
                public bool CanHaveElement { get; set; } = false;
                public List<string>? Values { get; set; } = null;
            }

            private static Schema? _schema;

            /// <summary>
            /// Elements and their attributes
            /// </summary>
            public static SortedDictionary<string, SortedDictionary<string, string>> ElementInfo { get; set; } = new();

            /// <summary>
            /// Attributes of elements that can have property elements
            /// </summary>
            public static Dictionary<string, List<string>> PropertyElements { get; set; } = new();

            /// <summary>
            /// All type info
            /// </summary>
            public static SortedDictionary<string, Type> Types { get; set; } = new();

            public static void ParseSchema()
            {
                if (_schema != null)
                    return;

                _schema = JsonSerializer.Deserialize<Schema>(Resource.GetString("CustomBootstrapperSchema.json").Result);
                if (_schema == null)
                    throw new Exception("Deserialised CustomBootstrapperSchema is null");

                foreach (var type in _schema.Types)
                    Types.Add(type.Key, type.Value);

                PopulateElementInfo();
            }

            private static (SortedDictionary<string, string>, List<string>) GetElementAttributes(string name, Element element)
            {
                if (ElementInfo.ContainsKey(name))
                    return (ElementInfo[name], PropertyElements[name]);

                List<string> properties = new List<string>();
                SortedDictionary<string, string> attributes = new();

                foreach (var attribute in element.Attributes)
                {
                    attributes.Add(attribute.Key, attribute.Value);

                    if (!Types.ContainsKey(attribute.Value))
                        throw new Exception($"Schema for type {attribute.Value} is missing. Blame Matt!");

                    Type type = Types[attribute.Value];
                    if (type.CanHaveElement)
                        properties.Add(attribute.Key);
                }

                if (element.SuperClass != null)
                {
                    (SortedDictionary<string, string> superAttributes, List<string> superProperties) = GetElementAttributes(element.SuperClass, _schema!.Elements[element.SuperClass]);
                    foreach (var attribute in superAttributes)
                        attributes.Add(attribute.Key, attribute.Value);

                    foreach (var property in superProperties)
                        properties.Add(property);
                }

                properties.Sort();

                ElementInfo[name] = attributes;
                PropertyElements[name] = properties;

                return (attributes, properties);
            }

            private static void PopulateElementInfo()
            {
                List<string> toRemove = new List<string>();

                foreach (var element in _schema!.Elements)
                {
                    GetElementAttributes(element.Key, element.Value);

                    if (!element.Value.IsCreatable)
                        toRemove.Add(element.Key);
                }

                // remove non-creatable from list now that everything is done
                foreach (var name in toRemove)
                {
                    ElementInfo.Remove(name);
                }
            }
        }

        CompletionWindow? _completionWindow = null;

        public BootstrapperEditorWindow(string name)
        {
            CustomBootstrapperSchema.ParseSchema();

            var viewModel = new BootstrapperEditorWindowViewModel();
            viewModel.Name = name;
            viewModel.Title = $"Editing \"{name}\"";
            viewModel.Code = File.ReadAllText(Path.Combine(Paths.CustomThemes, name, "Theme.xml"));

            DataContext = viewModel;
            InitializeComponent();

            UIXML.Text = viewModel.Code;
            UIXML.TextArea.TextEntered += OnTextAreaTextEntered;
        }

        private void OnCodeChanged(object sender, EventArgs e)
        {
            BootstrapperEditorWindowViewModel viewModel = (BootstrapperEditorWindowViewModel)DataContext;
            viewModel.Code = UIXML.Text;
            viewModel.OnPropertyChanged(nameof(viewModel.Code));
        }

        private void OnTextAreaTextEntered(object sender, TextCompositionEventArgs e)
        {
            switch (e.Text)
            {
                case "<":
                    OpenElementAutoComplete();
                    break;
                case " ":
                    OpenAttributeAutoComplete();
                    break;
                case ".":
                    OpenPropertyElementAutoComplete();
                    break;
                case "/":
                    AddEndTag();
                    break;
                case ">":
                    CloseCompletionWindow();
                    break;
                case "!":
                    CloseCompletionWindow();
                    break;
            }
        }

        private (string, int) GetLineAndPosAtCaretPosition()
        {
            // this assumes the file was saved as CSLF (\r\n newlines)
            int offset = UIXML.CaretOffset - 1;
            int lineStartIdx = UIXML.Text.LastIndexOf('\n', offset);
            int lineEndIdx = UIXML.Text.IndexOf('\n', offset);

            string line;
            int pos;
            if (lineStartIdx == -1 && lineEndIdx == -1)
            {
                line = UIXML.Text;
                pos = offset;
            }
            else if (lineStartIdx == -1)
            {
                line = UIXML.Text[..(lineEndIdx - 1)];
                pos = offset;
            }
            else if (lineEndIdx == -1)
            {
                line = UIXML.Text[(lineStartIdx + 1)..];
                pos = offset - lineStartIdx - 2;
            }
            else
            {
                line = UIXML.Text[(lineStartIdx + 1)..(lineEndIdx - 1)];
                pos = offset - lineStartIdx - 2;
            }

            return (line, pos);
        }

        /// <summary>
        /// Source: https://xsemmel.codeplex.com
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string? GetElementAtCursor(string xml, int offset, bool onlyAllowInside = false)
        {
            if (offset == xml.Length)
            {
                offset--;
            }
            int startIdx = xml.LastIndexOf('<', offset);
            if (startIdx < 0) return null;

            if (startIdx < xml.Length && xml[startIdx + 1] == '/')
            {
                startIdx = startIdx + 1;
            }

            int endIdx1 = xml.IndexOf(' ', startIdx);
            if (endIdx1 == -1 /*|| endIdx1 > offset*/) endIdx1 = int.MaxValue;

            int endIdx2 = xml.IndexOf('>', startIdx);
            if (endIdx2 == -1 /*|| endIdx2 > offset*/)
            {
                endIdx2 = int.MaxValue;
            }
            else
            {
                if (onlyAllowInside && endIdx2 < offset)
                    return null; // we dont want attribute auto complete to show outside of elements

                if (endIdx2 < xml.Length && xml[endIdx2 - 1] == '/')
                {
                    endIdx2 = endIdx2 - 1;
                }
            }

            int endIdx = Math.Min(endIdx1, endIdx2);
            if (endIdx2 > 0 && endIdx2 < int.MaxValue && endIdx > startIdx)
            {
                string element = xml.Substring(startIdx + 1, endIdx - startIdx - 1);
                return element == "!--" ? null : element; // dont treat comments as elements
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// A space between the cursor and the element will completely cancel this function
        /// </summary>
        private string? GetElementAtCursorNoSpaces(string xml, int offset)
        {
            (string line, int pos) = GetLineAndPosAtCaretPosition();

            string curr = "";
            while (pos != -1)
            {
                char c = line[pos];
                if (c == ' ' || c == '\t')
                    return null;
                if (c == '<')
                    return curr;
                curr = c + curr;
                pos--;
            }

            return null;
        }

        /// <summary>
        /// Returns null if not eligible to auto complete there.
        /// Returns the name of the element to show the attributes for
        /// </summary>
        /// <returns></returns>
        private string? ShowAttributesForElementName()
        {
            (string line, int pos) = GetLineAndPosAtCaretPosition();

            // check if theres an even number of speech marks on the line
            int numSpeech = line.Count(x => x == '"');
            if (numSpeech % 2 == 0)
            {
                // we have an equal number, let's check if pos is in between the speech marks
                int count = -1;
                int idx = pos;
                int size = line.Length - 1;
                while (idx != -1)
                {
                    count++;

                    if (size > idx + 1)
                        idx = line.IndexOf('"', idx + 1);
                    else
                        idx = -1;
                }

                if (count % 2 != 0)
                {
                    // odd number of speech marks means we're inside a string right now
                    // we dont want to display attribute auto complete while we're inside a string
                    return null;
                }
            }

            return GetElementAtCursor(UIXML.Text, UIXML.CaretOffset, true);
        }

        private void AddEndTag()
        {
            CloseCompletionWindow();

            if (UIXML.Text.Length > 2 && UIXML.Text[UIXML.CaretOffset - 2] == '<')
            {
                var elementName = GetElementAtCursor(UIXML.Text, UIXML.CaretOffset - 3);
                if (elementName == null)
                    return;

                UIXML.TextArea.Document.Insert(UIXML.CaretOffset, $"{elementName}>");
            }
            else
            {
                if (UIXML.Text.Length > UIXML.CaretOffset && UIXML.Text[UIXML.CaretOffset] == '>')
                    return;

                var elementName = ShowAttributesForElementName(); // re-using functions :)
                if (elementName != null)
                    UIXML.TextArea.Document.Insert(UIXML.CaretOffset, ">");
            }
        }

        private void OpenElementAutoComplete()
        {
            var data = new List<ICompletionData>();

            foreach (var element in CustomBootstrapperSchema.ElementInfo.Keys)
                data.Add(new ElementCompletionData(element));

            ShowCompletionWindow(data);
        }

        private void OpenAttributeAutoComplete()
        {
            string? element = ShowAttributesForElementName();
            if (element == null)
            {
                CloseCompletionWindow();
                return;
            }

            if (!CustomBootstrapperSchema.ElementInfo.ContainsKey(element))
            {
                CloseCompletionWindow();
                return;
            }

            var attributes = CustomBootstrapperSchema.ElementInfo[element];

            var data = new List<ICompletionData>();

            foreach (var attribute in attributes)
                data.Add(new AttributeCompletionData(attribute.Key, () => OpenTypeValueAutoComplete(attribute.Value)));

            ShowCompletionWindow(data);
        }

        private void OpenTypeValueAutoComplete(string typeName)
        {
            var typeValues = CustomBootstrapperSchema.Types[typeName].Values;
            if (typeValues == null)
                return;

            var data = new List<ICompletionData>();

            foreach (var value in typeValues)
                data.Add(new TypeValueCompletionData(value));

            ShowCompletionWindow(data);
        }

        private void OpenPropertyElementAutoComplete()
        {
            string? element = GetElementAtCursorNoSpaces(UIXML.Text, UIXML.CaretOffset);
            if (element == null)
            {
                CloseCompletionWindow();
                return;
            }

            if (!CustomBootstrapperSchema.PropertyElements.ContainsKey(element))
            {
                CloseCompletionWindow();
                return;
            }

            var properties = CustomBootstrapperSchema.PropertyElements[element];

            var data = new List<ICompletionData>();

            foreach (var property in properties)
                data.Add(new TypeValueCompletionData(property));

            ShowCompletionWindow(data);
        }

        private void CloseCompletionWindow()
        {
            if (_completionWindow != null)
            {
                _completionWindow.Close();
                _completionWindow = null;
            }
        }

        private void ShowCompletionWindow(List<ICompletionData> completionData)
        {
            CloseCompletionWindow();

            if (!completionData.Any())
                return;

            _completionWindow = new CompletionWindow(UIXML.TextArea);

            IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
            foreach (var c in completionData)
                data.Add(c);

            _completionWindow.Show();
            _completionWindow.Closed += (_, _) => _completionWindow = null;
        }
    }

    public class ElementCompletionData : ICompletionData
    {
        public ElementCompletionData(string text)
        {
            this.Text = text;
        }

        public System.Windows.Media.ImageSource? Image => null;

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content => Text;

        public object? Description => null;

        public double Priority { get; }

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }

    public class AttributeCompletionData : ICompletionData
    {
        private Action _openValueAutoCompleteAction;

        public AttributeCompletionData(string text, Action openValueAutoCompleteAction)
        {
            _openValueAutoCompleteAction = openValueAutoCompleteAction;
            this.Text = text;
        }

        public System.Windows.Media.ImageSource? Image => null;

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content => Text;

        public object? Description => null;

        public double Priority { get; }

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text + "=\"\"");
            textArea.Caret.Offset = textArea.Caret.Offset - 1;
            _openValueAutoCompleteAction();
        }
    }

    public class TypeValueCompletionData : ICompletionData
    {
        public TypeValueCompletionData(string text)
        {
            this.Text = text;
        }

        public System.Windows.Media.ImageSource? Image => null;

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content => Text;

        public object? Description => null;

        public double Priority { get; }

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}
