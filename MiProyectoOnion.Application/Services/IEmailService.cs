namespace MiProyectoOnion.Domain.Interfaces.Services;

// We define a MOCK to test if the email is really sending de messages to the email
// Instead of sending it and avoiding blocks from Gmail due to SPAM
public interface IEmailService
{
    // We define who, The subject and the body.
    void SendEmail(string to, string subject, string body);
}