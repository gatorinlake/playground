namespace BlackJackTraining.DataAccess
{
    using System.Collections.Generic;

    public interface IGameRecordStore
    {
        void InsertGameRecord(GameRecord gameRecord);

        void UpdateGameRecord(GameRecord gameRecord);

        GameRecord GetGameRecordById(long gameId);

        IEnumerable<GameRecord> GetGameRecordPage(long start, int count); 
    }
}
