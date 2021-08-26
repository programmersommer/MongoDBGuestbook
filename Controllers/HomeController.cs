using System;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Guestbook.Models;
using System.Net.Http;
using Newtonsoft.Json;
using MongoDB.Driver;
using System.Web;

namespace Guestbook
{

    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IMongoCollection<Message> _messages;

        public HomeController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;

            _httpClient = httpClientFactory.CreateClient("defaultClient");

            var client = new MongoClient(_configuration["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(_configuration["MongoDB:Database"]);
            _messages = database.GetCollection<Message>(_configuration["MongoDB:Collection"]);
        }

        public IActionResult Index()
        {
            return View("Index");
        }

        public IActionResult AddMessagePage()
        {

            return View("AddMessage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<bool> AddMessage(Guestbook.ViewModels.AddMessageViewModel model)
        {
            try
            {
                var parameters = new Dictionary<string, string> {
                { "secret", _configuration["ReCaptcha:SecretKey"] },
                { "response", model.Token } };
                var encodedContent = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", encodedContent);
                var result = JsonConvert.DeserializeObject<ReCaptchaResponse>(await response.Content.ReadAsStringAsync());

                if (result.success)
                {
                    _messages.InsertOne(new Message { SenderName = model.Name, Email = model.Email, MessageText = model.MessageText, MessageDate = System.DateTime.UtcNow });
                }

            }
            catch (Exception _)
            {
                return false;
            }

            return true;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataHandler(DataTableParameters model)
        {
            var start = Convert.ToInt32(HttpContext.Request.Form["start"].FirstOrDefault());
            var length = Convert.ToInt32(HttpContext.Request.Form["length"].FirstOrDefault());

            List<Message> messages;
            long total;

            total = await _messages.Find(Message => true).CountDocumentsAsync();
            messages = await _messages.Find(Message => true).Sort(new MongoDB.Bson.BsonDocument("MessageDate", -1)).Skip(start).Limit(length).ToListAsync();

            var newData = messages.Select(m => new[]
            {
                    "<div class=\"row align-items-center justify-content-center mt-3\">"
                                  +"<span style=\"color:Orange;\">"+HttpUtility.HtmlEncode(m.SenderName)+"</span>"
                                          +"&nbsp; &nbsp; &nbsp;"
                                     + "<span style=\"color:DarkBlue;\">"+HttpUtility.HtmlEncode(m.Email)+"</span>"
                                         +"&nbsp; &nbsp; &nbsp;"
                                     +"<span style=\"color:BurlyWood;\">"+HttpUtility.HtmlEncode(m.MessageDate)+"</span>"
                                   +"</div>"
                                   +"<div class=\"row align-items-center justify-content-center mt-3\">"
                                      +"<div>"+DisplaySmiles(HttpUtility.HtmlEncode(m.MessageText))+"</div>"
                                   +"</div>"
                }).ToList();

            var result = new
            {
                aaData = newData,
                iTotalDisplayRecords = total,
                iTotalRecords = total
            };

            return Json(result);
        }

        public string DisplaySmiles(string text)
        {
            StringBuilder sb = new StringBuilder(text);

            sb = sb.Replace(":)", "<img alt='smile' src='images/emotions/smile.gif' />")
                       .Replace(";)", "<img alt='wink' src='images/emotions/wink.gif' />")
                       .Replace(":(", "<img alt='sad' src='images/emotions/sad.gif' />")
                       .Replace(":robot:", "<img alt='robot' src='images/emotions/robot.gif' />")
                       .Replace(":oops:", "<img alt='oops' src='images/emotions/oops.gif' />")
                       .Replace(":inLove:", "<img alt='love' src='images/emotions/inLove.gif' />")
                       .Replace(":fingerUp:", "<img alt='finger up' src='images/emotions/fingerUp.gif' />")
                       .Replace(":fingerDown:", "<img alt='finger down' src='images/emotions/fingerDown.gif' />")
                       .Replace(":angel:", "<img alt='angel' src='images/emotions/angel.gif' />")
                       .Replace(":angry:", "<img alt='angry' src='images/emotions/angry.gif' />");

            return sb.ToString();
        }

    }
}