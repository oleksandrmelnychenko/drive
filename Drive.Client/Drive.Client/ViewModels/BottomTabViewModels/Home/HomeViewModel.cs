﻿using Drive.Client.Helpers;
using Drive.Client.Models.Arguments.BottomtabSwitcher;
using Drive.Client.Models.EntityModels.Announcement;
using Drive.Client.ViewModels.Base;
using Drive.Client.ViewModels.BottomTabViewModels.Home.Post;
using Drive.Client.ViewModels.Posts;
using Drive.Client.Views.BottomTabViews.Home;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Drive.Client.ViewModels.BottomTabViewModels.Home {
    public sealed class HomeViewModel : TabbedViewModelBase {

        PostBaseViewModel[] _posts = new PostBaseViewModel[] { };
        public PostBaseViewModel[] Posts {
            get => _posts;
            private set => SetProperty(ref _posts, value);
        }

        PostBaseViewModel _selectedPostViewModel;
        public PostBaseViewModel SelectedPostViewModel {
            get { return _selectedPostViewModel; }
            set {
                if (SetProperty(ref _selectedPostViewModel, value)) {
                    if (value != null) {
                        NavigationService.NavigateToAsync<PostCommentsViewModel>(value);
                    }
                }
            }
        }

        protected override void TabbViewModelInit() {
            RelativeViewType = typeof(HomeView);
            TabIcon = IconPath.HOME;
            HasBackgroundItem = false;
        }


        protected override void OnSubscribeOnAppEvents() {
            base.OnSubscribeOnAppEvents();


        }

        protected override void OnUnsubscribeFromAppEvents() {
            base.OnUnsubscribeFromAppEvents();


        }

        public override Task InitializeAsync(object navigationData) {
            if (navigationData is SelectedBottomBarTabArgs) {
                try {
                    List<PostBaseViewModel> foundPosts = new List<PostBaseViewModel>();

                    for (int i = 0; i < 100; i++) {
                        Announce postBase = new Announce();
                        PostBaseViewModel postViewModel = DependencyLocator.Resolve<PostBaseViewModel>();

                        if (i % 2 == 0) {
                            postBase.Type = AnnounceType.Text;
                        }
                        else {
                            postBase.Type = AnnounceType.Image;

                            if (i % 3 == 0) {
                                postBase.MediaUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRoQ_1qSVR7vwVmYg_WLDZJVnyDc_-Qg8yC1neV90WEFLon3Zz_xw";
                            }
                        }

                        postBase.AuthorName = string.Format("{0} {1}", postBase.AuthorName, i);
                        postBase.CommentsCount = i;
                        for (int m = 0; m < i; m++) {
                            postBase.Content = string.Format("{0}. {1}", postBase.AuthorName, postBase.AuthorName);
                        }
                        postBase.PublishDate = postBase.PublishDate - TimeSpan.FromHours(i);

                        postViewModel.Post = postBase;
                        foundPosts.Add(postViewModel);
                    }

                    Posts = foundPosts.ToArray();
                }
                catch (Exception ex) {
                    Debugger.Break();
                    throw;
                }
            }

            return base.InitializeAsync(navigationData);
        }
    }
}
