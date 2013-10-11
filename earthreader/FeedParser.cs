using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace earthreader {
	public class FeedParser {
		public static ObservableCollection<EntryItem> Parser(string strXML, string strCaption, FeedItem feedItem) {
			XmlReader reader = XmlReader.Create(new StringReader(strXML));
			SyndicationFeed feed = SyndicationFeed.Load(reader);

			ObservableCollection<EntryItem> listEntry = new ObservableCollection<EntryItem>();

			foreach (SyndicationItem item in feed.Items) {
				EntryItem ctm = new EntryItem();

				ctm.Title = item.Title.Text;
				ctm.URL = item.Links[0].Uri.OriginalString;
				ctm.Content = item.Summary.Text;

				ctm.Summary = HtmlRemoval.StripTagsCharArray(ctm.Content).Replace(Environment.NewLine, " ").Trim();
				ctm.Summary = HtmlRemoval.StripTagsCharArray(ctm.Content).Replace((char)10, ' ').Trim();


				try { ctm.Time = item.PublishDate.DateTime.ToString(); } catch { ctm.Time = ""; }
				ctm.Category = strCaption;

				listEntry.Add(ctm);
			}
			return listEntry;
		}
	}
}
