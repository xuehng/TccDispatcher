namespace renstech.NET.SupernovaDispatcher.Model
{
    public class LoginUserRole
    {
        public enum Role
        {
            Admin,
            Normal,
        }

        public LoginUserRole()
        {
            Charater = Role.Normal;
        }

        public Role Charater { get; set; }
    }
}
