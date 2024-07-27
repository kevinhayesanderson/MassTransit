namespace HelloAPI.Contracts
{
    //[EntityName("message-submitted")]
    //[ExcludeFromTopology]
    //[ConfigureConsumeTopology(true)]
    public class Message
    {
        public string? Text { get; set; } = string.Empty;
    }
}