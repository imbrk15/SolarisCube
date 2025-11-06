namespace SolarisCube.Models
{
    public class ContactModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }   // ✅ Added
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}
