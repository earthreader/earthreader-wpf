using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace earthreader {
	public class CustomControl {
		public static Button GetFeedItemButton(FeedItem feedItem, string strTag, double margin) {
			Button buttonBase = new Button() {
				Height = 40, Tag = strTag,
				Background = Brushes.Transparent,
				HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch,
				Margin = new System.Windows.Thickness(0, 0, 0, margin),
			};

			ContextMenu context = new ContextMenu() { HasDropShadow = false };

			if (strTag[0] == 'C') {
				MenuItem mItem1 = new MenuItem() { Header = "Rename", Tag = "R" + feedItem.ID };
				context.Items.Add(mItem1);
			}
			if (strTag[0] == 'C' || strTag[0] == 'F') {
				MenuItem mItem2 = new MenuItem() { Header = feedItem.Caption, Tag = "D" + feedItem.ID, HeaderStringFormat = "Delete {0}" };

				Binding binding0 = new Binding("Caption");
				binding0.Source = feedItem;
				mItem2.SetBinding(MenuItem.HeaderProperty, binding0);

				context.Items.Add(mItem2);
				buttonBase.ContextMenu = context;
			}

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

		public static Button GetFeedCandidateButton(FeedCandidateList fcl) {
			string strCaption = fcl.Title;
			string strURL = fcl.URL;

			Button buttonBase = new Button() {
				Height = 40, Tag = fcl,
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
