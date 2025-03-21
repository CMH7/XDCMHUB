namespace XDCMHUB.Server.Models;

public class Message
{
	public int Id { get; set; }
	public string Content { get; set; }
	public int UserId { get; set; }
	public User User { get; set; }
	public int ChannelId { get; set; }
	public Channel Channel { get; set; }
	public DateTime SentAt { get; set; }
}
