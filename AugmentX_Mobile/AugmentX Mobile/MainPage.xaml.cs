﻿using AugmentX_Mobile.ViewModel;

namespace AugmentX_Mobile
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage(MainViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }

}
