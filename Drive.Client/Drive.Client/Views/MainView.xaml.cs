﻿using Drive.Client.Views.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Drive.Client.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainView : ContentPageBaseView {
        public MainView() {
            InitializeComponent();
        }
    }
}