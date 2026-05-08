using PMS_CS.src.Models;

namespace PMS_CS.Views
{
    // Holds the currently logged-in user details across the application
    public static class Session
    {
        public static User? CurrentUser { get; set; }
        public static Patient? CurrentPatient { get; set; }
        public static Employee? CurrentEmployee { get; set; }

        public static void Clear()
        {
            CurrentUser = null;
            CurrentPatient = null;
            CurrentEmployee = null;
        }
    }

    // A simple input dialog to replace JavaFX's TextInputDialog
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 400, Height = 200, FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption, StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Width = 340, Text = text };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340 };
            Button confirmation = new Button() { Text = "OK", Left = 260, Width = 100, Top = 100, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
        }
    }
}