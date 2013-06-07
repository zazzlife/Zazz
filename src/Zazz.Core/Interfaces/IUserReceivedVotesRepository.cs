namespace Zazz.Core.Interfaces
{
    public interface IUserReceivedVotesRepository
    {
        int GetCount(int userId);

        int Increment(int userId);

        int Decrement(int userId);
    }
}