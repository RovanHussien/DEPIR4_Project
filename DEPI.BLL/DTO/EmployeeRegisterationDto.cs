using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.BLL.DTO
{
    public class EmployeeRegisterationDto
    {
        [Display(Name ="Ssn")]
        public string EmployeeId { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }
        public string Address { get; set; }
        public int PhoneNumber { get; set; }
    }
}
