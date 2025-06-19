using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoginApp.Models
{
    public class NguoiDungModel // Change 'class' to 'public class'
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int BiKhoa { get; set; }
        public string LyDoKhoa { get; set; }
        public DateTime? NgayTao { get; set; }
        public int NhomUserId { get; set; }
    }
}

