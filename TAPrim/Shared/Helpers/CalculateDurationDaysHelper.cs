namespace TAPrim.Shared.Helpers
{
	public class CalculateDurationDaysHelper
	{
		public static int CalculateDurationDays(int durationValue, string durationUnit)
		{
			switch (durationUnit.ToLower())
			{
				case "day":
				case "days":
					return durationValue;
				case "month":
				case "months":
					return durationValue * 30;
				case "year":
				case "years":
					return durationValue * 12 * 30;
				default:
					throw new ArgumentException("Invalid duration unit. Use 'day', 'month', or 'year'.");
			}
		}

	}
}
