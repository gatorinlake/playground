namespace BlackJackTraining.DataAccess
{
    using System.Collections.Generic;
    using System.Linq;
    using LiteDB;

    public class LiteGameRecordStore : IGameRecordStore
    {
        private const string GameRecordStoreName = "GameRecords";

        public string DatabaseName { get; private set; }

        public LiteGameRecordStore(string dbName)
        {
            this.DatabaseName = dbName;
        }

        public void InsertGameRecord(GameRecord gameRecord)
        {
            using(var db = new LiteDatabase(this.DatabaseName))
            {
                var gameRecords = db.GetCollection<GameRecord>(GameRecordStoreName);

                gameRecords.Insert(gameRecord);
            }
        }

        public void UpdateGameRecord(GameRecord gameRecord)
        {
            using (var db = new LiteDatabase(this.DatabaseName))
            {
                var gameRecords = db.GetCollection<GameRecord>(GameRecordStoreName);

                gameRecords.Update(gameRecord);
            }
        }

        public GameRecord GetGameRecordById(long gameId)
        {
            using (var db = new LiteDatabase(this.DatabaseName))
            {
                var gameRecords = db.GetCollection<GameRecord>(GameRecordStoreName);

                return gameRecords.FindById(gameId);
            }
        }

        public IEnumerable<GameRecord> GetGameRecordPage(long start, int count)
        {
            using (var db = new LiteDatabase(this.DatabaseName))
            {
                var gameRecords = db.GetCollection<GameRecord>(GameRecordStoreName);

                return gameRecords.Find(x => x.Id >= start, 0, count).OrderBy(x => x.Id);
            }
        }
    }
}
