using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Awem.Windowing;
using ReactiveUI;
using Tabalt;
using static System.Windows.Visibility;
using ReactiveUI.Wpf;

namespace Awem.Launcher
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : IViewFor<LauncherViewModel>
	{
		public static readonly DependencyProperty ViewModelProperty = DependencyProperty
			.Register(nameof(ViewModel), typeof(LauncherViewModel), typeof(MainWindow));

		public MainWindow()
		{
			InitializeComponent();

			var windowManager = new WindowManager(this.ToggleVisibility, App.Current.Shutdown);
			this.ViewModel = new LauncherViewModel(windowManager);

			this.NotifcationAreaIcon = new NotifcationAreaIcon(this.ViewModel.WindowManager.WindowManagerActions);

			this.WhenActivated(d =>
			{
				var textChanges = this.txtFilter.Events().TextChanged;

				//this.Bind(this.ViewModel, vm => vm.WindowManager.AllApplications, v => v.lvApplications.ItemsSource);

			});

			//this.Bind(this.ViewModel, vm => vm.WindowManager.AllApplications, v => v.lvApplications.ItemsSource);

			this.lvApplications.ItemsSource = this.ViewModel.WindowManager.AllApplications;
		}

		public NotifcationAreaIcon NotifcationAreaIcon { get;  }

		public LauncherViewModel ViewModel
		{
			get => (LauncherViewModel)GetValue(ViewModelProperty);
			set => SetValue(ViewModelProperty, value);
		}

		object IViewFor.ViewModel
		{
			get => ViewModel;
			set => ViewModel = (LauncherViewModel)value;
		}

		private void ToggleVisibility() => this.Visibility = this.Visibility == Hidden ? Visible : Hidden;

		private void lvApplications_KeyDown(object sender, KeyEventArgs e)
		{
//			if (Keyboard.Modifiers == ModifierKeys.None)
//			{
//				this.IsSpecialKeyHandled(e, false);
//				return;
//			}
//			this.FocusInput();
		}


		private void lvApplications_KeyUp(object sender, KeyEventArgs e)
		{
//			this.IsSpecialKeyHandled(e, true);
//			this.UpdateBigIconFromSelection();
		}
		private void lvApplications_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
//			this.FocusInput();
		}
		private void txtFilter_KeyUp(object sender, KeyEventArgs e) {}

		protected override void OnClosed(EventArgs args)
		{
			this.NotifcationAreaIcon.Dispose();
			this.ViewModel.Dispose();
		}
	}


	public class LauncherViewModel : ReactiveObject, IDisposable
	{
		public WindowManager WindowManager { get; }

		public LauncherViewModel(WindowManager windowManager)
		{
			this.WindowManager = windowManager;
		}

		public void Dispose() => this.WindowManager?.Dispose();
	}
}
