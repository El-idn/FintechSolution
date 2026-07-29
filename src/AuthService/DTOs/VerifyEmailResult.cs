namespace AuthService.DTOs
{
    public class VerifyEmailResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
