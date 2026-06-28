using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Model
{
    public  class SwapRequest
    { 
        public int RequestId { get; set; }


        public Employee? RequestEmployee { get; set; }
        public string? RequestingEmployeeId { get; set; }
        public Employee? RecipientEmployee { get; set; }
        public string? RecipientEmployeeId { get; set; }

        public Schedule? Schedule { get; set; }
        public int? ScheduleId { get; set; }
        public string? Status { get; set; } = "Pending";
    }
}
