namespace CoursesAPI.Services.Utilities
{
	public class DateTimeUtils
	{
		public static bool IsLeapYear(int year)
		{
			// TODO: add your logic here!!!1!!!one!!!eleven
			if((year % 4 == 0 && year % 100 != 0) || (year % 400 == 0))
			{
				return true;
			} else {
				return false;
			}
		}
	}
}
