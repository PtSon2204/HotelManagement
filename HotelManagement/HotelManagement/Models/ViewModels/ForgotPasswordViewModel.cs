namespace HotelManagement.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        public string Username { get; set; } = null!;

        public string NewPassword { get; set; } = null!;

        public string ConfirmNewPassword { get; set; } = null!;
    }
}

