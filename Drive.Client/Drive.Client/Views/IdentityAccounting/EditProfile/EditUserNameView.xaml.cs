﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Drive.Client.Views.IdentityAccounting.EditProfile {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditUserNameView : StepBaseView {
        public EditUserNameView() {
            InitializeComponent();
        }

        protected override void OnAppearing() {
            _entyEx.Focus();
        }
    }
}