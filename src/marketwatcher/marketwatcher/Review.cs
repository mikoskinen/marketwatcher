using System;

namespace marketplace.review.fetcher
{
    public class Review
    {
        public string Id { get; private set; }
        public string Author { get; private set; }
        public DateTime UpdateTime { get; private set; }
        public int Score { get; private set; }
        public string Comments { get; private set; }
        public string CountryCode { get; private set; }

        public Review(string id, string author, DateTime updateTime, int score, string comments, string countryCode)
        {
            Id = id;
            Author = author;
            UpdateTime = updateTime;
            Score = score;
            Comments = comments;
            CountryCode = countryCode;
        }
    }
}