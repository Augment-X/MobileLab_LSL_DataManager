using AugmentX_Mobile.ViewModel;

namespace AugmentX_Mobile;

public partial class ControlPage : ContentPage
{
	public ControlPage(ControlViewModel vm)
    {
		InitializeComponent();
		BindingContext = vm;
	}
}