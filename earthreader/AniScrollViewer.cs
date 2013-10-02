using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace earthreader {
	class AniScrollViewer : ScrollViewer {
		public static readonly DependencyProperty CurrentVerticalOffsetProperty =
			DependencyProperty.Register("CurrentVerticalOffset", typeof(double), typeof(AniScrollViewer),
			new PropertyMetadata(new PropertyChangedCallback(OnVerticalChanged)));

		public double CurrentVerticalOffset {
			get { return (double)GetValue(CurrentVerticalOffsetProperty); }
			set { SetValue(CurrentVerticalOffsetProperty, value); }
		}

		private static void OnVerticalChanged(DependencyObject property, DependencyPropertyChangedEventArgs e) {
			AniScrollViewer viewer = property as AniScrollViewer;
			viewer.ScrollToVerticalOffset((double)e.NewValue);
		}
	}
}
