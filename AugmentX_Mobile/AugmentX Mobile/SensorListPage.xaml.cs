using AugmentX_Mobile.ViewModel;

namespace AugmentX_Mobile;

public partial class SensorListPage : ContentPage
{
	public SensorListPage(SensorListViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}