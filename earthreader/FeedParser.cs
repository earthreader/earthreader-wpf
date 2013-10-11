using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace earthreader {
	public class FeedParser {
		public static List<EntryItem> Parser(string strXML, string strCaption) {
			XmlReader reader = XmlReader.Create(new StringReader(strXML));
			SyndicationFeed feed = SyndicationFeed.Load(reader);

			List<EntryItem> listEntry = new List<EntryItem>();

			foreach (SyndicationItem item in feed.Items) {
				EntryItem ctm = new EntryItem();

				ctm.Title = item.Title.Text;
				ctm.URL = item.Links[0].Uri.OriginalString;
				ctm.Content = item.Summary.Text;
				ctm.Summary = HtmlRemoval.StripTagsCharArray(ctm.Content);

				try { ctm.Time = item.PublishDate.DateTime.ToString(); } catch { ctm.Time = ""; }
				ctm.Category = strCaption;

				listEntry.Add(ctm);
			}
			return listEntry;
		}
	}
}
