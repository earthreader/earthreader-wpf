using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace earthreader {
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {

			InitializeComponent();

			buttonMinimize.Click += (o, e) => Microsoft.Windows.Shell.SystemCommands.MinimizeWindow(Window.GetWindow(this));
			buttonMaximize.Click += (o, e) => Microsoft.Windows.Shell.SystemCommands.MaximizeWindow(Window.GetWindow(this));
			buttonRestore.Click += (o, e) => Microsoft.Windows.Shell.SystemCommands.RestoreWindow(Window.GetWindow(this));
			buttonClose.Click += (o, e) => Microsoft.Windows.Shell.SystemCommands.CloseWindow(Window.GetWindow(this));
			this.StateChanged += MainWindow_StateChanged;

			this.MouseMove += MainWindow_MouseMove;
			this.PreviewMouseDown += (o, e) => isMouseDown = true;
			this.PreviewMouseUp += (o, e) => isMouseDown = false;

			buttonCategoryAccept.Click += (o, e) => {
				if (textboxCategoryInput.Text == "") {
					ShowMessage("Category name can't be empty");
					return;
				}
			};

			// Add root
			dictFeedItem.Add(0, new FeedItem() {
				ID = 0, Caption = "all feeds", Count = 1,
				IsFeed = true, URL = "", Children = new List<int>(),
				Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconFeed.png")),
			});

			/*
			TextBlock txt = new TextBlock() {
				Height = 50, Background = Brushes.White,
			};

			stackFeedlist1.Children.Add(txt);

			Binding binding = new Binding("Caption");
			binding.Source = dictFeedItem[0];
			txt.SetBinding(TextBlock.TextProperty, binding);

			dictFeedItem[0].Caption = "ASdkuasbdoiuasbd";*/

			RefreshFeedList(0);
		}

		
		bool isScrollVisible = false, isMouseDown, isAddWindowMode;
		int nFeedlistWidth = 250, nCount = 1;
		Dictionary<int, FeedItem> dictFeedItem = new Dictionary<int, FeedItem>();

		private void MainWindow_MouseMove(object sender, MouseEventArgs e) {
			if (isAddWindowMode) {
				ScrollBar s = scrollFeedlist.Template.FindName("PART_VerticalScrollBar", scrollFeedlist) as ScrollBar;
				s.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(0)));
				isScrollVisible = false;
				return;
			} 
			
			if (isScrollVisible && e.GetPosition(this).X > nFeedlistWidth + 10 && !isMouseDown) {
				isScrollVisible = false;

				ScrollBar s = scrollFeedlist.Template.FindName("PART_VerticalScrollBar", scrollFeedlist) as ScrollBar;
				s.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(150)));

			} else if (!isScrollVisible && e.GetPosition(this).X <= nFeedlistWidth + 10) {
				isScrollVisible = true;

				ScrollBar s = scrollFeedlist.Template.FindName("PART_VerticalScrollBar", scrollFeedlist) as ScrollBar;
				s.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(150)));
			}
		}

		private void MainWindow_StateChanged(object sender, EventArgs e) {
			if (this.WindowState == WindowState.Normal) {
				buttonRestore.Visibility = Visibility.Collapsed;
				buttonMaximize.Visibility = Visibility.Visible;
				gridMain.Margin = new Thickness(0);
			} else {
				buttonRestore.Visibility = Visibility.Visible;
				buttonMaximize.Visibility = Visibility.Collapsed;
				gridMain.Margin = new Thickness(5);
			}
		}

		string strReadmode = "all";
		private void buttonModeSelect_Click(object sender, RoutedEventArgs e) {
			strReadmode = (string)((Button)sender).Tag;
			int nMode = 0;

			switch (strReadmode) {
				case "all": nMode = 0; break;
				case "unread": nMode = 1; break;
				case "starred": nMode = 2; break;
			}

			imageModeAll.Visibility = nMode == 0 ? Visibility.Visible : Visibility.Collapsed;
			imageModeUnread.Visibility = nMode == 1 ? Visibility.Visible : Visibility.Collapsed;
			imageModeStarred.Visibility = nMode == 2 ? Visibility.Visible : Visibility.Collapsed;

			buttonModeAll.Visibility = nMode != 0 ? Visibility.Visible : Visibility.Collapsed;
			buttonModeUnread.Visibility = nMode != 1 ? Visibility.Visible : Visibility.Collapsed;
			buttonModeStarred.Visibility = nMode != 2 ? Visibility.Visible : Visibility.Collapsed;
		}

		private void gridAddBackCover_MouseDown(object sender, MouseButtonEventArgs e) { buttonAdd.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }
		private void buttonAdd_Click(object sender, RoutedEventArgs e) {
			if (isAddWindowMode) {
				gridAddBackCover.IsHitTestVisible = false;
			} else {
				gridAddBackCover.IsHitTestVisible = true;
				gridMessage.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(new Thickness(0), TimeSpan.FromMilliseconds(0)));

				textboxFeedInput.Text = textboxCategoryInput.Text = "";
				stackListAutoDiscovery.Children.Clear();
				textboxFeedInput.Focus();
			}
			isAddWindowMode = !isAddWindowMode;

			int nParameter = isAddWindowMode ? 1 : 0;
			Storyboard sb = new Storyboard();
			DoubleAnimation da = new DoubleAnimation(nParameter, TimeSpan.FromMilliseconds(250));
			ThicknessAnimation ta = new ThicknessAnimation(new Thickness(0, 0, 0, -90 * (1 - nParameter)), TimeSpan.FromMilliseconds(250)) {
				BeginTime = TimeSpan.FromMilliseconds(150 * nParameter),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut }
			};

			Storyboard.SetTarget(da, gridAddWindow); Storyboard.SetTargetProperty(da, new PropertyPath(Grid.OpacityProperty));
			Storyboard.SetTarget(ta, gridAddInnerWindow); Storyboard.SetTargetProperty(ta, new PropertyPath(Grid.MarginProperty));

			sb.Children.Add(da); sb.Children.Add(ta);
			sb.Begin(this);
		}

		private void ShowMessage(string message) {
			textMessage.Text = message;
			Storyboard sb = new Storyboard();
			ThicknessAnimation ta1 = new ThicknessAnimation(new Thickness(0, -30, 0, 0), TimeSpan.FromMilliseconds(200)) {
				BeginTime = TimeSpan.FromMilliseconds(100),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
			};
			ThicknessAnimation ta2 = new ThicknessAnimation(new Thickness(0), TimeSpan.FromMilliseconds(200)) {
				BeginTime = TimeSpan.FromSeconds(3),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
			};

			Storyboard.SetTarget(ta1, gridMessage); Storyboard.SetTarget(ta2, gridMessage);
			Storyboard.SetTargetProperty(ta1, new PropertyPath(Grid.MarginProperty));
			Storyboard.SetTargetProperty(ta2, new PropertyPath(Grid.MarginProperty));

			sb.Children.Add(ta1); sb.Children.Add(ta2);
			sb.Begin(this);
		}


		bool isFirstView = true;
		private void RefreshFeedList(int nID) {
			isFirstView = !isFirstView;

			StackPanel stackPrev = stackFeedlist1;
			StackPanel stackNext = stackFeedlist2;
			if (isFirstView) {
				stackPrev = stackFeedlist2;
				stackNext = stackFeedlist1;
			}

			stackNext.Children.Clear();
			Button buttonRoot = CustomControl.GetFeedItemButton(dictFeedItem[nID], "A" + nID, 0);
			buttonRoot.Click += buttonFeedItem_Click;
			stackNext.Children.Add(buttonRoot);

			if (nID > 0) {
				FeedItem fItem = new FeedItem() {
					ID = 0, Caption = "back to parent asdubasodubasodubasoubasidbasodub", Count = 0,
					IsFeed = true, URL = "", Children = new List<int>(),
					Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconBack.png")),
				};
				Button buttonBack = CustomControl.GetFeedItemButton(fItem, "B" + nID, 20);
				buttonBack.Click += buttonFeedItem_Click;
				stackNext.Children.Add(buttonBack);
			}

			foreach (int fItemTag in dictFeedItem[nID].Children) {
				Button buttonItem = CustomControl.GetFeedItemButton(dictFeedItem[fItemTag], fItemTag.ToString(), 0);
				buttonItem.Click += buttonFeedItem_Click;
				stackNext.Children.Add(buttonItem);
			}
		}

		private void buttonFeedItem_Click(object sender, RoutedEventArgs e) {
			string strTag = (string)((Button)sender).Tag;
			int nID = 0;

			switch (strTag[0]) {
				case 'A':
					// all feeds
					nID = Convert.ToInt32(strTag.Substring(1));

					break;
				case 'B':
					// back to parent
					nID = Convert.ToInt32(strTag.Substring(1));

					break;
				default:
					// default
					nID = Convert.ToInt32(strTag);

					break;
			}

			MessageBox.Show(nID.ToString());
		}
	}
}
