using Microsoft.AspNetCore.Mvc;

namespace Well_Readings.Contracts
{
    public static class ApiRoutes
    {
        public const string Base = "https://localhost:7090/api/daily-entries";

        public const string Wells = Base + "/wells";
        public const string Today = Base + "/today";

        public static string ById(Guid id) => $"{Base}/{id}";
    }
}
