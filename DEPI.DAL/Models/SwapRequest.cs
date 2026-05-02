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


        // navigation properties for employees
        public Employee? RequestEmployee { get; set; }
        public string? RequestingEmployeeId { get; set; }
        public Employee? RecipientEmployee { get; set; }
        public string? RecipientEmployeeId { get; set; }

        // navigation property for requesting employee's schedule
        public Schedule? Schedule { get; set; }
        public int? ScheduleId { get; set; }
    }
}
