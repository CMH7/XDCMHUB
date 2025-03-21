namespace XDCMHUB.Server.Models;

public class User
{
	public int Id { get; set; }
	public string Username { get; set; }
	public string PasswordHash { get; set; }
	public string Salt { get; set; }
	public bool IsAdmin { get; set; }
	public DateTime CreatedAt { get; set; }
}
