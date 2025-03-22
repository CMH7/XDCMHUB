namespace XDCMHUB.Mobile.Components.Pages;

public partial class Login
{

	#region Custom Classes
	Credential _credential { get; set; }
	#endregion Custom Classes

	#region Primitives
	bool _isBusy { get; set; }
	#endregion Primitives

	#region Overrides
	protected override void OnInitialized()
	{
		_credential ??= new();
		base.OnInitialized();
	}
	#endregion Overrides

	#region Custom Functions
	async Task Submit()
	{

	}
	#endregion Custom Functions

	#region Classes
	class Credential
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}
	#endregion Classes
}