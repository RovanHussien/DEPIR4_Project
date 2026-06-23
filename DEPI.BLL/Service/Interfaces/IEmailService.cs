using System.Threading.Tasks;

namespace DEPI.BLL.Service.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
