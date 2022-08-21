namespace TrainingSchedule.Services
{
    public interface IUsersDataService
    {
        void AddUserName(long botUserId, string userName);

        string GetUserName(long botUserId);

        void RemoveUserName(long botUserId);

        void AddUserLesson(long botUserId, (int disciplineId, int levelId, DateOnly date, TimeOnly time) lesson);

        (int disciplineId, int levelId, DateOnly date, TimeOnly time) GetUserLesson(long botUserId);

        void RemoveUserLesson(long botUserId);
    }
}