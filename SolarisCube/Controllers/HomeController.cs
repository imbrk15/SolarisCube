using Microsoft.AspNetCore.Mvc;
using SolarisCube.Models;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SolarisCube.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;

        public HomeController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitContact([FromBody] ContactModel enquiry)
        {
            if (enquiry == null || string.IsNullOrWhiteSpace(enquiry.Email))
                return BadRequest(new { success = false, message = "Please fill all required fields." });

            try
            {
                // 1️⃣ Send mail to Admin
                using (var smtp = new SmtpClient(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"])))
                {
                    smtp.Credentials = new NetworkCredential(_config["Smtp:User"], _config["Smtp:Password"]);
                    smtp.EnableSsl = true;

                    var adminMail = new MailMessage
                    {
                        From = new MailAddress(_config["Smtp:User"], "SolarisCube Website"),
                        Subject = $"New Enquiry from {enquiry.Name}",
                        Body =
                            $"<h3>New Enquiry Received</h3>" +
                            $"<p><b>Name:</b> {enquiry.Name}</p>" +
                            $"<p><b>Email:</b> {enquiry.Email}</p>" +
                            $"<p><b>Mobile:</b> {enquiry.Mobile}</p>" +
                            $"<p><b>Subject:</b> {enquiry.Subject}</p>" +
                            $"<p><b>Message:</b> {enquiry.Message}</p>",
                        IsBodyHtml = true
                    };
                    adminMail.To.Add("support@solarisCube.in");

                    await smtp.SendMailAsync(adminMail);
                }

                // 2️⃣ Send auto-reply to User
                using (var smtp = new SmtpClient(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"])))
                {
                    smtp.Credentials = new NetworkCredential(_config["Smtp:User"], _config["Smtp:Password"]);
                    smtp.EnableSsl = true;

                    var userMail = new MailMessage
                    {
                        From = new MailAddress(_config["Smtp:User"], "SolarisCube Team"),
                        Subject = "We received your enquiry",
                        Body =
                            $"<p>Hi {enquiry.Name},</p>" +
                            $"<p>Thank you for contacting SolarisCube. We will respond shortly.</p>" +
                            $"<h4>Your Enquiry:</h4>" +
                            $"<p><b>Subject:</b> {enquiry.Subject}</p>" +
                            $"<p><b>Message:</b> {enquiry.Message}</p>" +
                            $"<br/><a href='https://www.solariscube.com' target='_blank'>Visit SolarisCube</a>" +
                            $"<br/><p>Best regards,<br/>SolarisCube Team</p>",
                        IsBodyHtml = true
                    };
                    userMail.To.Add(enquiry.Email);

                    await smtp.SendMailAsync(userMail);
                }

                return Ok(new { success = true, message = "Your enquiry has been sent successfully!" });
            }
            catch (System.Exception ex)
            {
                // 🔴 Show real Gmail error
                return StatusCode(500, new { success = false, message = $"SMTP Error: {ex.Message}" });
            }
        }
    }
}
