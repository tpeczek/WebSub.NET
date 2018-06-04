using System.ComponentModel.DataAnnotations;

namespace Demo.AspNetCore.WebSub.Model
{
    public class SubscribeViewModel
    {
        [Required]
        public string Url { get; set; }
    }
}
