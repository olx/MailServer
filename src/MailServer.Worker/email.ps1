$Username = "MyUserName";
$Password = "MyPassword";

function Send-ToEmail([string]$email){

    $message = new-object Net.Mail.MailMessage;
    $message.From = "YourName@gmail.com";
    $message.To.Add($email);
    $message.Subject = "subject text here...";
    $message.Body = "body text here...";


    $smtp = new-object Net.Mail.SmtpClient("127.0.0.1", "25");
    $smtp.Credentials = New-Object System.Net.NetworkCredential($Username, $Password);
    $smtp.send($message);
    write-host "Mail Sent" ; 
 }

Send-ToEmail  -email "reciever@gmail.com"