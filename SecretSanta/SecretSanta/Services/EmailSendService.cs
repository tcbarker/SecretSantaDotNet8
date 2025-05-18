using SecretSanta.Interfaces;

namespace SecretSanta.Services;

public class EmailSendService : IEmailSendService {//fake test version not for production

    static string Emailhistory = "Fake email sent log - started at "+DateTime.Now+":-\r\n";

    public Task SendEmail(string address, string subject, string body){
        string message = "Email Sent to: "+address+", Subject: "+subject+"\r\n"+body;
        Emailhistory+= "\r\nMessage sent at "+DateTime.Now+" : "+message;
        Console.WriteLine(message);//not for production
        return Task.CompletedTask;
    }

    public Task<string> GetStatus(){
        return Task.FromResult<string>(Emailhistory);//not for production
    }

}

