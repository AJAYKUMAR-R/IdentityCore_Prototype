using System.Threading.Tasks;

namespace IdentityNetCore.Service;

//Email abstraction for implementing the Email logic for dependency injection
public interface IEmailSender
{
    Task SendEmailAsync(string fromAddress, string toAddress, string subject, string message);
}