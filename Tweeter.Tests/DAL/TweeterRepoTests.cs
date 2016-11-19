using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tweeter.DAL;
using Moq;
using System.Data.Entity;
using Tweeter.Models;
using System.Collections.Generic;
using System.Linq;

namespace Tweeter.Tests.DAL
{
    [TestClass]
    public class TweeterRepoTests
    {

        private Mock<DbSet<Twit>> mock_users { get; set; }
        private Mock<DbSet<Tweet>> mock_tweets { get; set; }
        private Mock<TweeterContext> mock_context { get; set; }
        private TweeterRepository Repo { get; set; }
        private List<Twit> users { get; set; }
        private List<Tweet> tweets { get; set; }
        private List<ApplicationUser> app_users { get; set; }
        private Mock<DbSet<ApplicationUser>> mock_app_user { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            mock_context = new Mock<TweeterContext>();
            mock_users = new Mock<DbSet<Twit>>();
            mock_tweets = new Mock<DbSet<Tweet>>();
            mock_app_user = new Mock<DbSet<ApplicationUser>>();
            Repo = new TweeterRepository(mock_context.Object);
            users = new List<Twit>
            {
                new Twit {
                    TwitId = 1,
                    BaseUser = new ApplicationUser() { UserName = "radbrad"}
                },
                new Twit {
                    TwitId = 2,
                    BaseUser = new ApplicationUser() { UserName = "sirrichard"}
                }

            };

            app_users = new List<ApplicationUser>
            {
                new ApplicationUser { 
                    Id = "1",
                    UserName = "radbrad"
                },
                new ApplicationUser {                 
                    Id = "2",
                    UserName = "sirrichard"
                }
            };

            tweets = new List<Tweet>();

            /* 
             1. Install Identity into Tweeter.Tests (using statement needed)
             2. Create a mock context that uses 'UserManager' instead of 'TweeterContext'
             */
        }

        public void ConnectToDatastore()
        {
            var query_users = users.AsQueryable();
            var query_tweets = tweets.AsQueryable();
            var query_app_user = app_users.AsQueryable();

            mock_users.As<IQueryable<Twit>>().Setup(m => m.Provider).Returns(query_users.Provider);
            mock_users.As<IQueryable<Twit>>().Setup(m => m.Expression).Returns(query_users.Expression);
            mock_users.As<IQueryable<Twit>>().Setup(m => m.ElementType).Returns(query_users.ElementType);
            mock_users.As<IQueryable<Twit>>().Setup(m => m.GetEnumerator()).Returns(() => query_users.GetEnumerator());

            mock_context.Setup(c => c.TweeterUsers).Returns(mock_users.Object);
            mock_users.Setup(u => u.Add(It.IsAny<Twit>())).Callback((Twit t) => users.Add(t));
            /*
             * Below mocks the 'Users' getter that returns a list of ApplicationUsers
             * mock_user_manager_context.Setup(c => c.Users).Returns(mock_users.Object);
             * 
             */

            /* IF we just add a Username field to the Twit model
             * mock_context.Setup(c => c.TweeterUsers).Returns(mock_users.Object); Assuming mock_users is List<Twit>
             */

            mock_tweets.As<IQueryable<Tweet>>().Setup(m => m.Provider).Returns(query_tweets.Provider);
            mock_tweets.As<IQueryable<Tweet>>().Setup(m => m.Expression).Returns(query_tweets.Expression);
            mock_tweets.As<IQueryable<Tweet>>().Setup(m => m.ElementType).Returns(query_tweets.ElementType);
            mock_tweets.As<IQueryable<Tweet>>().Setup(m => m.GetEnumerator()).Returns(() => query_tweets.GetEnumerator());

            mock_context.Setup(c => c.Tweets).Returns(mock_tweets.Object);
            mock_tweets.Setup(u => u.Add(It.IsAny<Tweet>())).Callback((Tweet t) => tweets.Add(t));
            mock_tweets.Setup(u => u.Remove(It.IsAny<Tweet>())).Callback((Tweet t) => tweets.Remove(t));


            //mock 
            mock_app_user.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(query_app_user.Provider);
            mock_app_user.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(query_app_user.Expression);
            mock_app_user.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(query_app_user.ElementType);
            mock_app_user.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(() => query_app_user.GetEnumerator());

            mock_context.Setup(c => c.Users).Returns(mock_app_user.Object);
            mock_app_user.Setup(u => u.Add(It.IsAny<ApplicationUser>())).Callback((ApplicationUser t) => app_users.Add(t));

        }

        [TestMethod]
        public void RepoEnsureCanCreateInstance()
        {
            TweeterRepository repo = new TweeterRepository();
            Assert.IsNotNull(repo);
        }

        [TestMethod]
        public void RepoEnsureICanGetUsernames()
        {
            // Arrange
            ConnectToDatastore();

            // Act
            List<string> usernames = Repo.GetUsernames();

            // Assert
            Assert.AreEqual(2, usernames.Count);
        }

        [TestMethod]
        public void RepoEnsureUsernameExists()
        {
            // Arrange
            ConnectToDatastore();

            // Act
            bool exists = Repo.UsernameExists("radbrad");

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void RepoEnsureUsernameExistsOfTwit()
        {
            // Arrange
            ConnectToDatastore();

            // Act
            Twit found_twit = Repo.UsernameExistsOfTwit("radbrad");

            // Assert
            Assert.IsNotNull(found_twit);
        }

        [TestMethod]
        public void RepoEnsureICanCreateTweet()
        {
            // Arrange
            ConnectToDatastore();

            // Act
            Tweet a_tweet = new Tweet {
                TweetId = 1,
                Message = "my message",
                Author = new Twit { TwitId = 1, BaseUser = new ApplicationUser { UserName = "radbrad" } },
                CreatedAt = DateTime.Now
            };
            Repo.AddTweet(a_tweet);

            int expected_tweets = 1;
            int actual_tweets = Repo.Context.Tweets.Count();

            // Assert
            Assert.AreEqual(expected_tweets, actual_tweets);
        }

        [TestMethod]
        public void RepoEnsureICanCreateTweetWithMessage()
        {
            // Arrange
            ConnectToDatastore();

            // Act
            Repo.AddTweet("radbrad", "my tweet!!!!");

            int expected_tweets = 1;
            int actual_tweets = Repo.Context.Tweets.Count();

            // Assert
            Assert.AreEqual(expected_tweets, actual_tweets);
        }

        [TestMethod]
        public void RepoEnsureICanRemoveTweet()
        {
            // Arrange
            ConnectToDatastore();
            Tweet a_tweet = new Tweet
            {
                TweetId = 3,
                Message = "my message",
                CreatedAt = DateTime.Now,
                Author = new Twit { TwitId = 3, BaseUser = new ApplicationUser { UserName = "radbrad" } }
            };
            Tweet another_tweet = new Tweet
            {
                TweetId = 4,
                Message = "my message",
                CreatedAt = DateTime.Now,
                Author = new Twit { TwitId = 4, BaseUser = new ApplicationUser { UserName = "sirrichard" } }
            };
            Repo.AddTweet(a_tweet);
            Repo.AddTweet(another_tweet);

            // Act

            Tweet removed_tweet = Repo.RemoveTweet(3);

            int expected_tweets = 1;
            int actual_tweets = Repo.Context.Tweets.Count();

            // Assert
            Assert.AreEqual(expected_tweets, actual_tweets);
            Assert.IsNotNull(removed_tweet);
        }

        [TestMethod]
        public void RepoEnsureICanGetTweets()
        {
            // Arrange
            ConnectToDatastore();
            Repo.AddTweet("radbrad", "my tweet!!!!");
            
            // Act

            int expected_tweets = 1;
            int actual_tweets = Repo.GetTweets().Count();

            // Assert
            Assert.AreEqual(expected_tweets, actual_tweets);
        }

        [TestMethod]
        public void RepoEnsureIcanGetTwitUser()
        {

            // Arrange
            ConnectToDatastore();
            

            // Act
            Twit locate_twit = Repo.GetTwitUser("1");

            // Assert
            Assert.AreEqual("radbrad", locate_twit.BaseUser.UserName);

        }

    }
}
