using System;
using System.Windows.Controls;
using System.Windows.Input;
using Tabalt;
using static System.Windows.Visibility;

namespace Awem.Launcher
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			this.WindowManager = new WindowManager(this.ToggleVisibility, App.Current.Shutdown);

			this.NotifcationAreaIcon = new NotifcationAreaIcon(this.WindowManager.WindowManagerActions);
		}

		public NotifcationAreaIcon NotifcationAreaIcon { get;  }
		public WindowManager WindowManager { get; }


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

		protected override void OnClosed(EventArgs args) => this.WindowManager.Dispose();
	}
}
