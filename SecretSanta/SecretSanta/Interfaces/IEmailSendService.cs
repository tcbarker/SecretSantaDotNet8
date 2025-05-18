

namespace SecretSanta.Interfaces;

public interface IEmailSendService {

    Task SendEmail(string address, string subject, string body);
    Task<string> GetStatus();

}

