using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace marketwatcher
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

        public IObservable<List<Review>> FetchReviewsForApp(string appId)
        {
            var tasks = from c in countryCodes
                        select FetchReviewsForAppFromOneMarketplace(appId, c);

            return tasks.Merge();
        }

        public IObservable<List<Review>> FetchReviewsForAppFromOneMarketplace(string appId, string marketplaceCountryCode)
        {
            var webClient = new WebClient();

            var o = Observable.FromEventPattern<DownloadStringCompletedEventArgs>(webClient, "DownloadStringCompleted")
                              .ObserveOn(Scheduler.ThreadPool)
                              .Select(e =>
                                          {
                                              if (e.EventArgs.Error != null)
                                                  throw e.EventArgs.Error;

                                              var webSiteString = e.EventArgs.Result;

                                              var xmlReader = XmlReader.Create(new StringReader(webSiteString));
                                              var feed = SyndicationFeed.Load(xmlReader);

                                              var reviews = new List<Review>();
                                              if (feed == null)
                                                  return reviews;

                                              reviews.AddRange(from syndicationItem in feed.Items
                                                              let rating = syndicationItem.ElementExtensions[0].GetObject<XElement>().Value
                                                              where !string.IsNullOrEmpty(rating)
                                                              let content = syndicationItem.Content as TextSyndicationContent
                                                              where content != null
                                                              select new Review(syndicationItem.Id, syndicationItem.Authors[0].Name, syndicationItem.LastUpdatedTime.DateTime, int.Parse(rating), content.Text, marketplaceCountryCode));

                                              return reviews;
                                          }).Take(1);


            var reviewAddress = string.Format(BaseUrl, marketplaceCountryCode, appId);
            webClient.DownloadStringAsync(new Uri(reviewAddress, UriKind.Absolute));

            return o;
        }

    }
}
