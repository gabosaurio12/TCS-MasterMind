using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MasterMind_Client.Modals
{
    /// <summary>
    /// Interaction logic for PrivateRoomCodeModal.xaml.
    /// </summary>
    public partial class PrivateRoomCodeModal : Window
    {
        private bool isDistributingDigits;

        public PrivateRoomCodeModal()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(this, CodeBox_Pasting);
        }

        protected override void OnClosed(EventArgs e)
        {
            DataObject.RemovePastingHandler(this, CodeBox_Pasting);
            (DataContext as IDisposable)?.Dispose();
            base.OnClosed(e);
        }

        private void CodeTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var currentTxt = (TextBox)sender;
            var sanitizedText = SanitizeDigits(currentTxt.Text);

            if (!string.Equals(sanitizedText, currentTxt.Text, StringComparison.Ordinal))
            {
                currentTxt.Text = sanitizedText;
                currentTxt.CaretIndex = sanitizedText.Length;
                return;
            }

            if (!isDistributingDigits && currentTxt.Text.Length == 1)
            {
                MoveFocusToNext(currentTxt);
            }
        }

        private void CodeTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var currentBox = (TextBox)sender;

            if (e.Key == Key.Back && string.IsNullOrEmpty(currentBox.Text))
            {
                var request = new TraversalRequest(FocusNavigationDirection.Previous);
                currentBox.MoveFocus(request);
            }
        }

        private void CodeBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            var currentTxt = (TextBox)sender;
            var pastedText = e.DataObject.GetData(DataFormats.UnicodeText) as string;
            var digits = SanitizeDigits(pastedText);

            if (digits.Length == 0)
            {
                e.CancelCommand();
                return;
            }

            e.CancelCommand();
            DistributeDigits(currentTxt, digits);
        }

        private void DistributeDigits(TextBox currentTxt, string digits)
        {
            var codeBoxes = GetCodeBoxes(currentTxt);
            var startIndex = codeBoxes.IndexOf(currentTxt);

            if (startIndex < 0)
            {
                return;
            }

            isDistributingDigits = true;
            try
            {
                FillCodeBoxes(codeBoxes, startIndex, digits);
            }
            finally
            {
                isDistributingDigits = false;
            }

            var nextIndex = Math.Min(startIndex + digits.Length, codeBoxes.Count - 1);
            codeBoxes[nextIndex].Focus();
        }

        private static void FillCodeBoxes(IList<TextBox> codeBoxes, int startIndex, string digits)
        {
            for (var offset = 0; offset < digits.Length && startIndex + offset < codeBoxes.Count; offset++)
            {
                codeBoxes[startIndex + offset].Text = digits[offset].ToString();
            }
        }

        private static IList<TextBox> GetCodeBoxes(TextBox currentTxt)
        {
            var panel = currentTxt.Parent as Panel;
            return panel == null
                ? new List<TextBox>()
                : panel.Children.OfType<TextBox>().ToList();
        }

        private static string SanitizeDigits(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return new string(text.Where(char.IsDigit).ToArray());
        }

        private static void MoveFocusToNext(TextBox currentTxt)
        {
            var request = new TraversalRequest(FocusNavigationDirection.Next);
            currentTxt.MoveFocus(request);
        }
    }
}
