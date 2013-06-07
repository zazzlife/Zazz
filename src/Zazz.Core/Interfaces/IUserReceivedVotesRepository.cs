namespace Zazz.Core.Interfaces
{
    public interface IUserReceivedVotesRepository
    {
        int GetCount(int userId);

        void Increment(int userId);

        void Decrement(int userId);
    }
}