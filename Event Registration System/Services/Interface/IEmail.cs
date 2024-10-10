namespace Event_Registration_System.Servcies.Interface
{
    public interface IEmail
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
