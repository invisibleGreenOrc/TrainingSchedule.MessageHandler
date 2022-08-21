namespace TrainingSchedule.Services
{
    public class UsersDataService : IUsersDataService
    {
        private Dictionary<long, string> _userNames = new();

        private Dictionary<long, (int disciplineId, int levelId, DateOnly date, TimeOnly time)> _userLesson = new();

        public void AddUserName(long botUserId, string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException($"{nameof(userName)} не может быть пустым");
            }

            _userNames[botUserId] = userName;
        }

        public string GetUserName(long botUserId)
        {
            return _userNames[botUserId];
        }

        public void RemoveUserName(long botUserId)
        {
            if (!_userNames.Remove(botUserId))
            {
                throw new KeyNotFoundException($"В сохраненных именах пользователей не найдено имя для пользователя {botUserId}");
            }
        }

        public void AddUserLesson(long botUserId, (int disciplineId, int levelId, DateOnly date, TimeOnly time) lesson)
        {
            _userLesson[botUserId] = lesson;
        }

        public (int disciplineId, int levelId, DateOnly date, TimeOnly time) GetUserLesson(long botUserId)
        {
            return _userLesson[botUserId];
        }

        public void RemoveUserLesson(long botUserId)
        {
            if (!_userLesson.Remove(botUserId))
            {
                throw new KeyNotFoundException($"В сохраненных данных тренировок не найдена тренировка для пользователя {botUserId}");
            }
        }
    }
}
