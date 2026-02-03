namespace OnePro.Front.Helpers
{
    public static class StatusBadgeHelper
    {
        public static string GetStatusBadgeClass(string? status)
        {
            var s = status ?? "";

            return s switch
            {
                "Draft" => "bg-gray-100 text-gray-700 border border-gray-200",

                "Submitted" or "Submitted_To_BR" or "Submitted_To_BR_Manager" =>
                    "bg-blue-100 text-blue-700 border border-blue-200",

                "Review_BR" => "bg-indigo-100 text-indigo-700 border border-indigo-200",
                "Review_SARM" => "bg-emerald-100 text-emerald-700 border border-emerald-200",
                "Review_ECS" => "bg-teal-100 text-teal-700 border border-teal-200",

                "Return_BR_To_User" or "Return_SARM_To_BR" or "Return_ECS_To_BR" =>
                    "bg-amber-100 text-amber-700 border border-amber-200",

                "Approved" or "Done" or "Approved_By_BR_Manager" =>
                    "bg-green-100 text-green-700 border border-green-200",

                "Rejected" or "Rejected_By_BR" =>
                    "bg-red-100 text-red-700 border border-red-200",

                var x when x.StartsWith("Approval_", System.StringComparison.Ordinal) =>
                    "bg-purple-100 text-purple-700 border border-purple-200",

                _ => "bg-gray-100 text-gray-600 border border-gray-200"
            };
        }
    }
}
