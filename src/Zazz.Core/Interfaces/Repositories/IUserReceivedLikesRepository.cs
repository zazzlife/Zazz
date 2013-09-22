namespace Zazz.Core.Interfaces.Repositories
{
    public interface IUserReceivedLikesRepository
    {
        int GetCount(int userId);

        void Increment(int userId);

        void Decrement(int userId);
    }
}