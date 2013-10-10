using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace earthreader {
	public class AutoDiscovery {

		public static async Task<List<FeedCandidateList>> GetCandidateFeeds(string strURL) {
			List<FeedCandidateList> listCd = new List<FeedCandidateList>();

			Task<string> httpTask = GetHTML(strURL);
			string strHTML = await httpTask;

			if (strHTML == "") { return listCd; }

			try {
				XmlReader reader = XmlReader.Create(new StringReader(strHTML));
				SyndicationFeed feed = SyndicationFeed.Load(reader);

				listCd.Add(new FeedCandidateList() { Title = feed.Title.Text, URL = strURL });

			} catch (Exception e) {
				MessageBox.Show(e.Message);
				return listCd;
			}
			return listCd;
		}

		public static Task<string> GetHTML(string url) {
			return Task.Run(() => {
				string rtHTML = "";
				try {
					HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new UriBuilder(url).Uri);

					
					httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
					//httpWebRequest.Method = "POST";
					httpWebRequest.Timeout = 20000;
					httpWebRequest.UserAgent =
						"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; " +
						"Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; " +
						".NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; " +
						"InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)";

					httpWebRequest.Proxy = null;

					HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
					StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
					rtHTML = streamReader.ReadToEnd();
				} catch (Exception e) {
					MessageBox.Show(e.Message);
				}
				return rtHTML;
			});
		}
	}

	public struct FeedCandidateList {
		public string Title, URL;
	}
}
