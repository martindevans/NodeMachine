using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using NodeMachine.Connection;
using NodeMachine.Model.Project;
using System.ComponentModel;

namespace NodeMachine.View.Controls
{
    public abstract class BaseYamlEditorControl<TModel>
        : BaseEditorControl<TModel>
        where TModel : class, INotifyPropertyChanged, new()
    {
        protected abstract string ValueMarkup { get; set; }

        protected BaseYamlEditorControl(IProjectManager manager, IGameConnection connection, TModel value)
            : base(manager, connection, value)
        {
        }

        protected void Editor_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                var richTextBox = (RichTextBox)sender;
                if (richTextBox == null)
                    return;

                e.Handled = true;

                var start = richTextBox.Selection.Start;
                var end = richTextBox.Selection.End;

                //No text is selected, simply insert 2 spaces
                if (start.CompareTo(end) == 0)
                {
                    start.InsertTextInRun("  ");
                    var afterSpaces = end.GetPositionAtOffset(2);
                    richTextBox.Selection.Select(afterSpaces, afterSpaces);
                    return;
                }

                //A run of text is selected, insert spcaes at the start og all the lines
                var current = start;
                while (current != null && current.CompareTo(end) < 0)
                {
                    var lineStart = current.GetLineStartPosition(0);
                    if (lineStart != null)
                        lineStart.InsertTextInRun("  ");

                    current = current.GetLineStartPosition(1);
                }
            }
        }

        private bool _suppressSave = false;
        protected void Editor_OnLoaded(object sender, EventArgs e)
        {
            var editor = (RichTextBox)sender;

            _suppressSave = true;
            try
            {
                editor.Document.Blocks.Clear();
                editor.Document.Blocks.Add(new Paragraph(new Run(ValueMarkup ?? "")));
            }
            finally
            {
                _suppressSave = false;
            }
        }

        protected void Editor_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var editor = (RichTextBox)sender;

            if (editor.IsLoaded && !_suppressSave)
                ValueMarkup = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd).Text.TrimEnd('\r', '\n');
        }

        protected abstract void CheckMarkup(object sender, RoutedEventArgs e);

        protected abstract void SendToGame(object sender, RoutedEventArgs e);
    }
}
