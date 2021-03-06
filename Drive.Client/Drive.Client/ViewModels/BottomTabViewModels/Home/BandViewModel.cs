﻿using Drive.Client.Factories.Announcements;
using Drive.Client.Helpers;
using Drive.Client.Helpers.Localize;
using Drive.Client.Models.Arguments.BottomtabSwitcher;
using Drive.Client.Models.EntityModels.Announcement;
using Drive.Client.Models.EntityModels.Announcement.Comments;
using Drive.Client.Resources.Resx;
using Drive.Client.Services.Announcement;
using Drive.Client.Services.Signal.Announcement;
using Drive.Client.ViewModels.Base;
using Drive.Client.ViewModels.BottomTabViewModels.Home.Post;
using Drive.Client.ViewModels.IdentityAccounting.Registration;
using Drive.Client.ViewModels.Popups;
using Drive.Client.ViewModels.Posts;
using Drive.Client.Views.BottomTabViews.Home;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Drive.Client.ViewModels.BottomTabViewModels.Home {
    public sealed class BandViewModel: NestedViewModel, IVisualFiguring, ISwitchTab {

        private readonly IAnnouncementService _announcementService;

        private readonly IAnnouncementsFactory _announcementsFactory;

        private readonly IAnnouncementSignalService _announcementSignalService;

        private CancellationTokenSource _getPostsCancellationTokenSource = new CancellationTokenSource();

        ObservableCollection<PostBaseViewModel> _posts;
        public ObservableCollection<PostBaseViewModel> Posts {
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

        bool _visibilityClosedView;
        public bool VisibilityClosedView {
            get { return _visibilityClosedView; }
            set { SetProperty(ref _visibilityClosedView, value); }
        }

        PostFunctionPopupViewModel _postFunctionPopupViewModel;
        public PostFunctionPopupViewModel PostFunctionPopupViewModel {
            get => _postFunctionPopupViewModel;
            private set {
                _postFunctionPopupViewModel?.Dispose();
                SetProperty(ref _postFunctionPopupViewModel, value);
            }
        }

        public ICommand SignInCommand => new Command(async () => await NavigationService.NavigateToAsync<SignInPhoneNumberStepViewModel>());

        public ICommand SignUpCommand => new Command(async () => await NavigationService.NavigateToAsync<PhoneNumberRegisterStepViewModel>());

        public ICommand RefreshPostsCommand => new Command(() => OnRefreshPostsCommand());

        public ICommand OpenFunctionCommand => new Command((param) => OpenFunction(param));

        StringResource _tabHeader;
        public StringResource TabHeader {
            get => _tabHeader;
            private set => SetProperty(ref _tabHeader, value);
        }

        public Type RelativeViewType => typeof(BandView);

        /// <summary>
        ///     ctor().
        /// </summary>
        public BandViewModel(IAnnouncementService announcementService,
                             IAnnouncementsFactory announcementsFactory,
                             IAnnouncementSignalService announcementSignalService) {
            _announcementService = announcementService;
            _announcementsFactory = announcementsFactory;
            _announcementSignalService = announcementSignalService;

            PostFunctionPopupViewModel = DependencyLocator.Resolve<PostFunctionPopupViewModel>();
            PostFunctionPopupViewModel.InitializeAsync(this);
        }
     
        protected override void OnSubscribeOnAppEvents() {
            base.OnSubscribeOnAppEvents();

            _announcementSignalService.PostLikesCountReceived += PostLikesCountReceived;
            _announcementSignalService.NewAnnounceReceived += NewAnnounceReceived;
            _announcementSignalService.DeletedPostReceived += DeletedPostReceived;
            _announcementSignalService.PostCommentsCountReceived += PostCommentsCountReceived;
        }

        protected override void OnUnsubscribeFromAppEvents() {
            base.OnUnsubscribeFromAppEvents();

            _announcementSignalService.NewAnnounceReceived -= NewAnnounceReceived;
            _announcementSignalService.DeletedPostReceived -= DeletedPostReceived;
            _announcementSignalService.PostCommentsCountReceived -= PostCommentsCountReceived;
            _announcementSignalService.PostLikesCountReceived -= PostLikesCountReceived;
        }

        protected override void ResolveStringResources() {
            base.ResolveStringResources();

            TabHeader = ResourceLoader.GetString(nameof(AppStrings.BandUpperCase));
        }

        public override Task InitializeAsync(object navigationData) {
            if (navigationData is SelectedBottomBarTabArgs) {
                GetPosts();
            }

            PostFunctionPopupViewModel?.InitializeAsync(navigationData);

            return base.InitializeAsync(navigationData);
        }

        public override void Dispose() {
            base.Dispose();

            ResetCancellationTokenSource(ref _getPostsCancellationTokenSource);
            PostFunctionPopupViewModel?.Dispose();
            Posts?.ForEach(postViewModel => postViewModel.Dispose());
            Posts?.Clear();
        }

        private async void OnRefreshPostsCommand() {
            IsBusy = true;

            ResetCancellationTokenSource(ref _getPostsCancellationTokenSource);
            CancellationTokenSource cancellationTokenSource = _getPostsCancellationTokenSource;

            await GetDataAsync(cancellationTokenSource);

            IsBusy = false;
        }

        private void PostLikesCountReceived(object sender, PostLikedBody e) {
            try {
                if (Posts == null) return;

                foreach (var post in Posts) {
                    if (post.Post.AnnounceBody.Id == e.PostId) {
                        post.LikesCount = e.LikesCount;
                        if (e.PostLikedByUser.UserNetId == BaseSingleton<GlobalSetting>.Instance.UserProfile.NetId) {
                            post.IsLiked = e.PostLikedByUser.IsLikedByUser;
                        }
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"ERROR:{ex.Message}");
                Debugger.Break();
            }
        }

        private async void GetPosts() {
            try {
                ResetCancellationTokenSource(ref _getPostsCancellationTokenSource);
                CancellationTokenSource cancellationTokenSource = _getPostsCancellationTokenSource;

                Guid busyKey = Guid.NewGuid();
                UpdateBusyVisualState(busyKey, true);

                await GetDataAsync(cancellationTokenSource);

                UpdateBusyVisualState(busyKey, false);
            }
            catch (Exception ex) {
                Debug.WriteLine($"ERROR:{ex.Message}");
            }
        }

        private async Task GetDataAsync(CancellationTokenSource cancellationTokenSource) {
            var announces = await _announcementService.GetAnnouncesAsync(cancellationTokenSource);

            if (announces != null) {
                Posts = _announcementsFactory.BuildPostViewModels(announces);
            }
        }

        private void OpenFunction(object param) {
            if (param is PostBaseViewModel postBaseViewModelParam) {
                PostFunctionPopupViewModel.ShowPopupCommand.Execute(null);
                PostFunctionPopupViewModel.InitializeAsync(param);
            }
        }

        private void PostCommentsCountReceived(object sender, CommentCountBody e) {
            try {
                if (Posts == null) return;

                foreach (var post in Posts) {
                    if (post.Post.AnnounceBody.Id == e.PostId) {
                        post.CommentsCount = e.CommentsCount;
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"ERROR:{ex.Message}");
                Debugger.Break();
            }
        }

        private void DeletedPostReceived(object sender, string e) {
            try {
                if (Posts == null) return;

                foreach (PostBaseViewModel post in Posts.ToList()) {
                    if (post.Post.AnnounceBody.Id == e) {
                        Posts.Remove(post);
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"ERROR:{ex.Message}");
                Debugger.Break();
            }
        }

        private void NewAnnounceReceived(object sender, Announce e) {
            try {
                Posts?.Insert(0, _announcementsFactory.CreatePostViewModel(e));
            }
            catch (Exception ex) {
                Debug.WriteLine($"ERROR:{ex.Message}");
                Debugger.Break();
            }
        }

        private void UpdateView() {
            VisibilityClosedView = !BaseSingleton<GlobalSetting>.Instance.UserProfile.IsAuth;
        }

        public void ClearAfterTabTap() {
            //Dispose();
        }

        public void TabClicked() {
            GetPosts();
        }

        private void ClearSource() {
            
        }
    }
}
