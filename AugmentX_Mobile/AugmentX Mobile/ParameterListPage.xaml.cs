using AugmentX_Mobile.ViewModel;

namespace AugmentX_Mobile;

public partial class ParameterListPage : ContentPage
{
	public ParameterListPage(ParameterListViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}