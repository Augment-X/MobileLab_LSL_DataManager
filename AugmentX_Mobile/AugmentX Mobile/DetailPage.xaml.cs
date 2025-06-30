using AugmentX_Mobile.ViewModel;

namespace AugmentX_Mobile;

public partial class DetailPage : ContentPage
{
    public DetailPage(DetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}