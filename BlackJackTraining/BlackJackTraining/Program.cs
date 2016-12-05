namespace BlackJackTraining
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using BlackJackTraining.DataAccess;

    public class Program
    {
        public decimal AmountHigh { get; private set; }

        public decimal AmountLow { get; private set; }

        private const int RecordPageSize = 10000;

        private int gameCount;
        private bool readingMode;
        private LiteGameRecordStore recordStore;

        public static int Main(string[] args)
        {
            Program program = new Program();
            string errorMessage;

            if (!program.ParseArguments(args, out errorMessage))
            {
                Console.WriteLine(errorMessage);
                return 1;
            }

            program.Run();

            return 0;
        }

        public Program()
        {
            this.gameCount = 20;
            this.recordStore = null;
            this.readingMode = false;

            this.AmountHigh = this.AmountLow = 0;
        }

        public Program(string[] args) :
            this()
        {
            string errorMsg;

            if (!this.ParseArguments(args, out errorMsg))
            {
                throw new ArgumentException(errorMsg, "args");
            }
        }

        public void Run()
        {
            decimal totalWin;

            if (this.recordStore != null)
            {
                if (this.readingMode)
                {
                    long start = 0;
                    GameRecord lastOne = null;
                    int recordCount = 0;
                    totalWin = 0;
                    IEnumerable<GameRecord> recordPage = this.recordStore.GetGameRecordPage(start, RecordPageSize);

                    while (recordPage.Any())
                    {
                        foreach (var oneRecord in recordPage)
                        {
                            this.PrintEvents(oneRecord);
                            lastOne = oneRecord;
                            start = oneRecord.Id;
                            recordCount++;
                        }

                        recordPage = this.recordStore.GetGameRecordPage(++start, RecordPageSize);
                    }

                    if(lastOne != null)
                    {
                        totalWin = lastOne.ResultingAmount;
                    }

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(" *** Read Game Result ***");
                    Console.WriteLine();
                    Console.WriteLine("  - Game Count:     {0}", recordCount);
                    Console.WriteLine("  - Highest Amount: {0}", this.AmountHigh);
                    Console.WriteLine("  - Lowest Amount:  {0}", this.AmountLow);
                    Console.WriteLine("  - Final Amount:   {0}", totalWin);
                    Console.WriteLine("  - Source File:    {0}", this.recordStore.DatabaseName);
                }
                else
                {
                    totalWin = TrainingHelper.SimulateGames(
                                this.gameCount,
                                new GameContext(),
                                x => 20m,
                                TrainingHelper.GetActionByWizardStrategy,
                                this.StoreGameRecord);

                    Console.WriteLine();
                    Console.WriteLine(" *** Game Completed ***");
                    Console.WriteLine();
                    Console.WriteLine("  - Game Count:     {0}", this.gameCount);
                    Console.WriteLine("  - Final Amount:   {0}", totalWin);
                    Console.WriteLine("  - Result File:    {0}", this.recordStore.DatabaseName);
                }
            }
            else
            {
                // Default Run behavior: simulate games and print events to console
                totalWin = TrainingHelper.SimulateGames(
                            this.gameCount,
                            new GameContext(),
                            x => 20m,
                            TrainingHelper.GetActionByWizardStrategy,
                            this.PrintEvents);

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(" *** Game Completed ***");
                Console.WriteLine();
                Console.WriteLine("  - Game Count:     {0}", this.gameCount);
                Console.WriteLine("  - Highest Amount: {0}", this.AmountHigh);
                Console.WriteLine("  - Lowest Amount:  {0}", this.AmountLow);
                Console.WriteLine("  - Final Amount:   {0}", totalWin);
            }
        }

        /// <summary>
        /// Parse the arguments from command line:
        ///     BlackJackTraining [-g (GameCount)] [-s] [-r (FilePath)]
        ///     
        ///     -g : Set the game count to simulate.
        ///     -s : Save game result to file instead of console print-out.
        ///     -r : Read and print out saved game result from given file path.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool ParseArguments(string[] args, out string errorMsg)
        {
            string context = null;
            errorMsg = string.Empty;

            foreach (var argument in args)
            {
                if (!string.IsNullOrEmpty(context))
                {
                    if (context.StartsWith("-g"))
                    {
                        if (!int.TryParse(argument, out this.gameCount))
                        {
                            errorMsg = string.Format("Expect integer value for {0}. But input is {1}.", context, argument);
                            return false;
                        }
                    }
                    else if (context.StartsWith("-r"))
                    {
                        this.recordStore = new LiteGameRecordStore(argument);
                        this.readingMode = true;
                    }
                }
                else
                {
                    string cmdArg = argument.ToLower();

                    if (cmdArg.StartsWith("-s"))
                    {
                        string storeFile =
                            string.Format(
                                @"{0}\{1}_{2:yy}{2:MM}{2:dd}_{3}",
                                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                                "ResultStore",
                                DateTime.Now,
                                Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace('/', '-'));

                        this.recordStore = new LiteGameRecordStore(storeFile);
                    }
                    else
                    {
                        context = cmdArg;
                    }
                }
            }

            return true;
        }

        private void PrintEvents(GameRecord gameRecord)
        {
            gameRecord.GameEvents.ForEach(
                oneEvent => Console.WriteLine(
                    "{0},{1},{2},{3},{4}",
                    gameRecord.Id,
                    oneEvent.Id,
                    oneEvent.EventType,
                    oneEvent.EventMessage,
                    oneEvent.EventData));

            if (gameRecord.ResultingAmount > this.AmountHigh)
            {
                this.AmountHigh = gameRecord.ResultingAmount;
            }

            if (gameRecord.ResultingAmount < this.AmountLow)
            {
                this.AmountLow = gameRecord.ResultingAmount;
            }
        }

        private void StoreGameRecord(GameRecord gameRecord)
        {
            if(this.recordStore != null)
            {
                this.recordStore.InsertGameRecord(gameRecord);
            }
        }
    }
}
