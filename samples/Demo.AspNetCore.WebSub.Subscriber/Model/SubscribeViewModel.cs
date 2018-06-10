using System.ComponentModel.DataAnnotations;

namespace Demo.AspNetCore.WebSub.Subscriber.Model
{
    public class SubscribeViewModel
    {
        [Required]
        public string Url { get; set; }
    }
}
