using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace AugmentX_Mobile.ViewModel
{
    [QueryProperty("NameDisplay", "nameDisplay")]
    [QueryProperty("Parameters", "parameters")]
    public partial class ParameterListViewModel : ObservableObject
    {
        [ObservableProperty]
        string id;

        [ObservableProperty]
        string nameDisplay;

        [ObservableProperty]
        private ObservableCollection<ParameterItem> parameters = [];

        [ObservableProperty]
        public int currentCountDisplay = SelectionService.Instance.SelectedCount;

        public ParameterListViewModel()
        {
        }
    }
}

