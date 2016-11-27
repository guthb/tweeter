using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tweeter.DAL;
using Tweeter.Models;
using static Tweeter.Models.TweeterViewModels;

namespace Tweeter.Controllers
{
    public class TweetController : ApiController
    {
        //TweeterRepository Repo = new TweeterRepository();
        private TweeterRepository apiTweeterController = new TweeterRepository();

        // GET api/<controller>
        public IEnumerable<Tweet> Get()
        {
            return apiTweeterController.GetTweets();
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public Dictionary<string, bool> Post([FromBody]TweetViewModel tweet)
        {
            Dictionary<string, bool> answer = new Dictionary<string, bool>();


            if (ModelState.IsValid && User.Identity.IsAuthenticated)
            {
                //string user_id = User.Identity.GetUserId();
                //ApplicationUser found_app_user = Repo.Context.Users.FirstOrDefault(u => u.Id == user_id);
                //Twit found_user = Repo.Context.TweeterUsers.FirstOrDefault(twit => twit.BaseUser.UserName == found_app_user.UserName);
                Tweet new_tweet = new Tweet
                {
                    Message = tweet.Message,
                    ImageURL = tweet.ImageURL,
                    Author = apiTweeterController.GetTwitUser(User.Identity.GetUserId()),
                    CreatedAt = DateTime.Now
                };

                //Repo.AddTweet(new_tweet);
                apiTweeterController.AddTweet(new_tweet);
                answer.Add("Succesful", true);
            }
            else
            {
                answer.Add("Successful", false);     
            }

            return answer;
        }           
        

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
            //Repo.RemoveTweet(id);
            apiTweeterController.RemoveTweet(id);

        }
    }
}