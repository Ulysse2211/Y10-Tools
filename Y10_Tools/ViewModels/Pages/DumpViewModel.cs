namespace Y10_Tools.ViewModels.Pages
{
    public partial class DumpViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _counter = 0;

        [RelayCommand]
        private void OnCounterIncrement()
        {
            return;
        }
    }
}
