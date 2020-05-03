using System.ComponentModel.DataAnnotations;
using System;

namespace Guestbook.ViewModels
{

    public class AddMessageViewModel
    {
        public String Token { get; set; } 
        public string SenderName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string MessageText { get; set; }
    }

}
