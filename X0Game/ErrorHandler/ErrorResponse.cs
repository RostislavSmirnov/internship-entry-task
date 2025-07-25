namespace X0Game.ErrorHandler
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = null!;
        public string? Details { get; set; }
    }
}
