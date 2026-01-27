namespace OnePro.Front.Models
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public Guid? IdGroup { get; set; }

        public string Email { get; set; }
        public string Name { get; set; }

        public int Role { get; set; }

        public string RoleName =>
            Role switch
            {
                0 => "User_Member",
                1 => "User_Pic",
                2 => "User_Manager",
                3 => "User_VP",
                4 => "BR_Pic",
                5 => "BR_Member",
                6 => "BR_Manager",
                7 => "BR_VP",
                8 => "SARM_Pic",
                9 => "SARM_Member",
                10 => "SARM_Manager",
                11 => "SARM_VP",
                12 => "ECS_Pic",
                13 => "ECS_Member",
                14 => "ECS_Manager",
                15 => "ECS_VP",
                _ => "Unknown",
            };
    }

    // public class UserGroupResponse
    // {
    //     public string Email { get; set; }
    //     public string Name { get; set; }

    //     public int Role { get; set; }

    //     public string RoleName =>
    //         Role switch
    //         {
    //             0 => "User_Member",
    //             1 => "User_Pic",
    //             2 => "User_Manager",
    //             3 => "User_VP",
    //             4 => "BR_Pic",
    //             5 => "BR_Manager",
    //             6 => "BR_VP",
    //             7 => "SARM_Pic",
    //             8 => "SARM_Manager",
    //             9 => "SARM_VP",
    //             10 => "ECS_Pic",
    //             11 => "ECS_Manager",
    //             12 => "ECS_VP",
    //             _ => "Unknown",
    //         };
    // }
}
