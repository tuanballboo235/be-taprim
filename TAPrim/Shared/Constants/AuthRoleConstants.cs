namespace TAPrim.Shared.Constants
{
	public static class AuthRoleConstants
	{
		public const string SuperAdmin = "0";
		public const string Admin = "Admin";
		public const string AdminLowerCase = "admin";

		public const string AdminRoles =
			SuperAdmin + "," + Admin + "," + AdminLowerCase;
	}
}
