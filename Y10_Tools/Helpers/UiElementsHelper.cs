namespace Y10_Tools.Helpers
{
    class UiElementsHelper
    {
        public static async void MessBox(string title, string content)
        {
            var tmbox = new Wpf.Ui.Controls.MessageBox();
            tmbox.Content = content;
            tmbox.Title = title;
            await tmbox.ShowDialogAsync();
        }
    }
}
