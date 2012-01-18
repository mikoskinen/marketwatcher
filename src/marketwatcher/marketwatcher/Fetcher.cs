using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace marketplace.review.fetcher
{
    public class Fetcher
    {
        private const string BaseUrl = "http://catalog.zune.net/v3.2/{0}/apps/{1}/reviews/?store=Zest&chunkSize=50";
        private readonly List<string> countryCodes = new List<string>();

        public Fetcher()
        {
            countryCodes = new List<string>()
                               {
                                   "en-us",
                                    "en-gb",
                                    "en-au",
                                    "de-at",
                                    "fr-be",
                                    "nl-be",
                                    "pt-br",
                                    "en-ca",
                                    "fr-ca",
                                    "es-cl",
                                    "es-co",
                                    "cs-cz",
                                    "da-dk",
                                    "fi-fi",
                                    "fr-fr",
                                    "de-de",
                                    "el-gr",
                                    "en-hk",
                                    "zh-hk",
                                    "hu-hu",
                                    "en-in",
                                    "en-ie",
                                    "it-it",
                                    "ja-jp",
                                    "es-mx",
                                    "nl-nl",
                                    "en-nz",
                                    "nb-no",
                                    "pl-pl",
                                    "pt-pt",
                                    "ru-ru",
                                    "en-sg",
                                    "en-za",
                                    "ko-kr",
                                    "es-es",
                                    "sv-se",
                                    "de-ch",
                                    "fr-ch",
                                    "zh-tw",
                                    "zh-cn",

                               };

        }

        public List<Review> FetchReviewsForApp(string appId)
        {
            var result = new List<Review>();

            var tasks = new List<Task<List<Review>>>();
            foreach (var countryCode in countryCodes)
            {
                var code = countryCode;
                var task = Task.Factory.StartNew(() => FetchReviewsForAppFromOneMarketplace(appId, code));
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            foreach (var task in tasks)
            {
                result.AddRange(task.Result);
            }

            return result;
        }

        public List<Review> FetchReviewsForAppFromOneMarketplace(string appId, string marketplaceCountryCode)
        {
            var result = new List<Review>();

            var webClient = new WebClient();
            webClient.DownloadStringCompleted += (s, e) =>
                                                     {
                                                         if (e.Error != null)
                                                             throw e.Error;

                                                         var xmlReader = XmlReader.Create(new StringReader(e.Result));
                                                         var feed = SyndicationFeed.Load(xmlReader);

                                                         if (feed == null)
                                                             return;

                                                         result.AddRange(from syndicationItem in feed.Items
                                                                         let rating = syndicationItem.ElementExtensions[0].GetObject<XElement>().Value
                                                                         where !string.IsNullOrEmpty(rating)
                                                                         let content = syndicationItem.Content as TextSyndicationContent
                                                                         where content != null
                                                                         select new Review(syndicationItem.Id, syndicationItem.Authors[0].Name, syndicationItem.LastUpdatedTime.DateTime, int.Parse(rating), content.Text, marketplaceCountryCode));
                                                     };

            var reviewAddress = string.Format(BaseUrl, marketplaceCountryCode, appId);
            webClient.DownloadStringAsync(new Uri(reviewAddress, UriKind.Absolute));

            return result;
        }

    }
}
