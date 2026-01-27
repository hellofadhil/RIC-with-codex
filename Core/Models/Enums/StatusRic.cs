namespace Core.Models.Enums
{
    public enum StatusRic
    {
        Draft,
        Submitted_To_BR,
        Review_BR,
        Return_BR_To_User,
        Review_SARM,
        Review_ECS,
        Return_SARM_To_BR,
        Return_ECS_To_BR,
        Approval_Manager_User,
        Approval_VP_User,
        Approval_Manager_BR,
        Approval_Manager_SARM,
        Approval_VP_SARM,
        Approval_Manager_ECS,
        Approval_VP_ECS,
        Done,
    }
}
