using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AugmentX_Mobile.ViewModel
{
    [QueryProperty("Text", "id")]
    public partial class DetailViewModel : ObservableObject
    {
        [ObservableProperty]
        string text = string.Empty;

        [RelayCommand]
        async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}