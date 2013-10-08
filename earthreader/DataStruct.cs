using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace earthreader {
	public class FeedItem : INotifyPropertyChanged {
		public bool IsFeed;
		public string URL;
		public int ID, ParentID;
		public List<int> Children;
		public BitmapImage Favicon;

		private string _caption;
		public string Caption {
			get { return _caption; }
			set {
				_caption = value;
				OnPropertyChanged("Caption");
			}
		}

		private int _count;
		public int Count {
			get { return _count; }
			set {
				_count = value;
				if (_count == 0) {
					_opacity = 0;
				} else { Opacity = 1; }

				if (_count > 999) {
					_noticountstring = "999+";
				} else { _noticountstring = _count.ToString(); }
				OnPropertyChanged("Count");
			}
		}

		private double _opacity;
		public double Opacity {
			get { return _opacity; }
			set { _opacity = value; }
		}

		private string _noticountstring;
		public string NotiCount {
			get { return _noticountstring; }
			set {
				_noticountstring = value;
				OnPropertyChanged("NotiCount");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	public class CustomControl {
		public static Button GetFeedItemButton(FeedItem feedItem, string strTag, double margin) {
			Button buttonBase = new Button() {
				Height = 40, Tag = strTag,
				Background = Brushes.Transparent,
				HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch,
				Margin = new System.Windows.Thickness(0, 0, 0, margin),
			};

			Grid grid = new Grid();
			Image image = new Image() {
				Source = feedItem.Favicon, Width = 30, Height = 30, Margin = new System.Windows.Thickness(5),
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
			};
			TextBlock text = new TextBlock() {
				Margin = new System.Windows.Thickness(45, 0, 45, 0), HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				VerticalAlignment = System.Windows.VerticalAlignment.Center, FontSize = 15, Foreground = Brushes.White,
				TextTrimming = System.Windows.TextTrimming.CharacterEllipsis,
			};
			Grid gridCount = new Grid() { Margin = new System.Windows.Thickness(0, 2, 10, 0), Background = Brushes.Crimson, HorizontalAlignment = System.Windows.HorizontalAlignment.Right, VerticalAlignment = System.Windows.VerticalAlignment.Center };
			TextBlock textCount = new TextBlock() {
				Margin = new System.Windows.Thickness(5, 3, 5, 3),
				VerticalAlignment = System.Windows.VerticalAlignment.Center, FontSize = 10, Foreground = Brushes.White,
			};
			gridCount.Children.Add(textCount);

			Binding binding1 = new Binding("Caption");
			binding1.Source = feedItem;
			text.SetBinding(TextBlock.TextProperty, binding1);

			Binding binding2 = new Binding("NotiCount");
			binding2.Source = feedItem;
			textCount.SetBinding(TextBlock.TextProperty, binding2);

			Binding binding3 = new Binding("Opacity");
			binding3.Source = feedItem;
			gridCount.SetBinding(Grid.OpacityProperty, binding3);

			grid.Children.Add(image);
			grid.Children.Add(text);
			grid.Children.Add(gridCount);

			buttonBase.Content = grid;
			return buttonBase;
		}

		public static Button GetFeedCandidateButton(string strCaption, string strURL) {
			Button buttonBase = new Button() {
				Height = 40, Tag = new KeyValuePair<string, string>(strCaption, strURL),
				Background = Brushes.Transparent,
				HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch,
			};

			Grid grid = new Grid();
			TextBlock text = new TextBlock() {
				Text = strCaption,
				Margin = new System.Windows.Thickness(20, 0, 20, 0), HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				VerticalAlignment = System.Windows.VerticalAlignment.Center, FontSize = 15, Foreground = Brushes.White,
				TextTrimming = System.Windows.TextTrimming.CharacterEllipsis,
			};

			grid.Children.Add(text);
			buttonBase.Content = grid;

			return buttonBase;
		}
	}
}
