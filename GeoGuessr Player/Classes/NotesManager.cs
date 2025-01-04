using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GeoGuessrPlayer.Classes
{
    public class NotesManager
    {
        private static readonly string notesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GeoGuessrPlayerNotes.rtf");

        // Saving text with formatting from the RichTextBox to a file (RTF format)
        public static void SaveNotes(RichTextBox richTextBox)
        {
            try
            {
                TextRange range = new(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                using (FileStream fs = new(notesPath, FileMode.Create))
                {
                    range.Save(fs, DataFormats.Rtf); // Save as RTF format (retains rich text formatting)
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving file: " + ex.Message);
            }
        }

        // Loading text with formatting from a file (RTF format) into the RichTextBox
        public static void LoadNotes(RichTextBox richTextBox)
        {
            if (File.Exists(notesPath))
            {
                try
                {
                    TextRange range = new(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                    using (FileStream fs = new(notesPath, FileMode.Open))
                    {
                        range.Load(fs, DataFormats.Rtf); // Load as RTF format (preserves rich text formatting)
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading file: " + ex.Message);
                }
            }
            else
            {
                // Create a new file if it doesn't exist
                using FileStream fs = File.Create(notesPath);
            }
        }

        // Method to toggle Bold for selected text or caret
        public static void ToggleBold(RichTextBox NotesTextBox)
        {
            if (NotesTextBox.Selection.IsEmpty)
            {
                // If no text is selected, toggle the bold state at the caret position.
                var currentFormat = NotesTextBox.Selection.GetPropertyValue(TextElement.FontWeightProperty);
                if ((FontWeight)currentFormat != FontWeights.Bold)
                    NotesTextBox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                else
                    NotesTextBox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
            else
            {
                // Toggle bold for the selected text
                var currentFormat = NotesTextBox.Selection.GetPropertyValue(TextElement.FontWeightProperty);
                if ((FontWeight)currentFormat != FontWeights.Bold)
                    NotesTextBox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                else
                    NotesTextBox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
        }

        // Method to toggle Italic for selected text or caret
        public static void ToggleItalic(RichTextBox NotesTextBox)
        {
            if (NotesTextBox.Selection.IsEmpty)
            {
                // If no text is selected, toggle the italic state at the caret position.
                var currentFormat = NotesTextBox.Selection.GetPropertyValue(TextElement.FontStyleProperty);
                if ((FontStyle)currentFormat != FontStyles.Italic)
                    NotesTextBox.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
                else
                    NotesTextBox.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
            }
            else
            {
                // Toggle italic for the selected text
                var currentFormat = NotesTextBox.Selection.GetPropertyValue(TextElement.FontStyleProperty);
                if ((FontStyle)currentFormat != FontStyles.Italic)
                    NotesTextBox.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
                else
                    NotesTextBox.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
            }
        }

        // Method to increase the font size of the selected text
        public static void IncreaseFontSize(RichTextBox NotesTextBox)
        {
            if (!NotesTextBox.Selection.IsEmpty)
            {
                var currentSize = NotesTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
                if (currentSize != DependencyProperty.UnsetValue)
                {
                    double newSize = (double)currentSize + 2;  // Increase by 2 (adjust as needed)
                    NotesTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, newSize);
                }
            }
            else
            {
                // Increase the font size at the caret position
                var currentSize = NotesTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
                if (currentSize != DependencyProperty.UnsetValue)
                {
                    double newSize = (double)currentSize + 2;  // Increase by 2 (adjust as needed)
                    NotesTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, newSize);
                }
            }
        }

        // Method to decrease the font size of the selected text
        public static void DecreaseFontSize(RichTextBox NotesTextBox)
        {
            if (!NotesTextBox.Selection.IsEmpty)
            {
                var currentSize = NotesTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
                if (currentSize != DependencyProperty.UnsetValue)
                {
                    double newSize = (double)currentSize - 2;  // Decrease by 2 (adjust as needed)
                    NotesTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, newSize);
                }
            }
            else
            {
                // Decrease the font size at the caret position
                var currentSize = NotesTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
                if (currentSize != DependencyProperty.UnsetValue)
                {
                    double newSize = (double)currentSize - 2;  // Decrease by 2 (adjust as needed)
                    NotesTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, newSize);
                }
            }
        }
    }
}