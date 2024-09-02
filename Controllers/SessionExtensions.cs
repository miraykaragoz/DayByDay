namespace DayByDay.Extensions
{
    public static class SessionExtensions
    {
        public static string GetUsername(this ISession session)
        {
            return session.GetString("Username");
        }
    }
}