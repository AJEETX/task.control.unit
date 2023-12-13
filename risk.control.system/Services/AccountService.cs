using risk.control.system.Data;

namespace risk.control.system.Services
{
    public interface IAccountService
    {
        void ForgotPassword(string useremail, long mobile);
    }

    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext context;

        public AccountService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public void ForgotPassword(string useremail, long mobile)
        {
            //CHECK AND VALIDATE EMAIL PASSWORD

            //SEND SMS

            throw new NotImplementedException();
        }
    }
}