﻿using System;

using Xamarin.Forms;

namespace Drive.Client.Controls.Stacklist.Base {
    public abstract class SourceItemBase : ContentView {
        public SourceItemBase() {
            ItemSelectCommand = new Command(() => {
                SelectionAction(this);
            });
        }

        public Command ItemSelectCommand { get; private set; }

        public Action<SourceItemBase> SelectionAction { get; set; }

        public abstract void Selected();

        public abstract void Deselected();
    }
}