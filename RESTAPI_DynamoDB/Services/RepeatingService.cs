using Amazon.DynamoDBv2.DataModel;
using RESTAPI_DynamoDB.Data;
using RESTAPI_DynamoDB.Models;

namespace RESTAPI_DynamoDB.Services
{
    public class RepeatingService : BackgroundService
    {
        private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(24));
        private readonly IDynamoDBContext _context;

        public RepeatingService(IDynamoDBContext context)
        {
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(await _timer.WaitForNextTickAsync(stoppingToken)
                && !stoppingToken.IsCancellationRequested)
            {
                await CleanSongs();
                await CleanUsers();
            }
        }

        public async Task CleanSongs()
        {
            //Get all the Songs in the Database.
            List<Song> allSongs = await _context.ScanAsync<Song>(new List<ScanCondition>()).GetRemainingAsync();

            //Delete all the Songs in the Database.
            foreach (Song song in allSongs)
            {
                await _context.DeleteAsync(song);
            }

            //Repopulate the Database with the default Song values.
            foreach (Song song in DefaultValues.songs)
            {
                await _context.SaveAsync(song);
            }
        }

        public async Task CleanUsers()
        {
            //Get all the Users in the Database.
            List<User> allUsers = await _context.ScanAsync<User>(new List<ScanCondition>()).GetRemainingAsync();

            //Delete all the Users in the Database.
            foreach (User user in allUsers)
            {
                await _context.DeleteAsync(user);
            }

            //Repopulate the Database with the default User values.
            foreach (User user in DefaultValues.users)
            {
                await _context.SaveAsync(user);
            }
        }
    }
}
