using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalManager
{
    public class EventViewModel
    {
        public string? Id { get; set; }
        public string? Summary { get; set; }
        public string? Location { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }
}
