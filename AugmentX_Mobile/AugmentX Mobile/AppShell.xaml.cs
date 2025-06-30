namespace AugmentX_Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(SensorListPage), typeof(SensorListPage));
            Routing.RegisterRoute(nameof(ParameterListPage), typeof(ParameterListPage));

            Routing.RegisterRoute(nameof(ControlPage), typeof(ControlPage));
            Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
        }
    }
}
