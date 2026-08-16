namespace gameHubBack.DTOs.Rooms;

public class ChatMessageDto
{
    public required string Nickname { get; set; }
    public required string Text { get; set; }
    public required DateTime SentAt { get; set; }
}
