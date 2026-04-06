using DoAnCk.Models.Entities;
using System.Collections.Generic;

namespace DoAnCk.Models.ViewModels
{
    public class UserListViewModel
    {
        public User User { get; set; }
        public List<string> Roles { get; set; }
    }
}