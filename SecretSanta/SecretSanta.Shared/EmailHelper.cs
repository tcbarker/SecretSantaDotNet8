

namespace SecretSanta.Shared;

public class EmailHelper {
    
    public static bool IsValid(string email){
        try {
            var address = new System.Net.Mail.MailAddress(email);
        }
        catch {
            return false;
        }
        return true;
    }

    public static string[] ParseForEmails(string emails){
        List<string> foundmails = new List<string>();
        string[] atsplit = emails.Split('@');
        for(int i = 0;i<atsplit.Length-1;i++){
            if(atsplit[i+1]==string.Empty){
                i++;
                continue;
            }
            if(atsplit[i]==string.Empty){
                continue;
            }

            char[] splitter = new Char[]{',','\r','\n',' ',';'};

            string[] usersplits = atsplit[i].Split(splitter);
            string[] domainsplits = atsplit[i+1].Split(splitter);
            string newmail = usersplits[usersplits.Length-1]+"@"+domainsplits[0];

            if(IsValid(newmail)){
                foundmails.Add(newmail);
            }
        }

        return foundmails.ToArray();
    }

}

/*

make a test?? todo.

List of Valid Email Addresses

email@example.com
firstname.lastname@example.com
email@subdomain.example.com
firstname+lastname@example.com
email@123.123.123.123
email@[123.123.123.123]
"email"@example.com
1234567890@example.com
email@example-one.com
_______@example.com
email@example.name
email@example.museum
email@example.co.jp
firstname-lastname@example.com



List of Strange Valid Email Addresses

much.”more\ unusual”@example.com
very.unusual.”@”.unusual.com@example.com
very.”(),:;<>[]”.VERY.”very@\\ "very”.unusual@strange.example.com



List of Invalid Email Addresses

plainaddress
#@%^%#$@#$@#.com
@example.com
Joe Smith <email@example.com>
email.example.com
email@example@example.com
.email@example.com
email.@example.com
email..email@example.com
あいうえお@example.com
email@example.com (Joe Smith)
email@example
email@-example.com
email@example.web
email@111.222.333.44444
email@example..com
Abc..123@example.com



List of Strange Invalid Email Addresses

”(),:;<>[\]@example.com
just”not”right@example.com
this\ is"really"not\allowed@example.com
*/

