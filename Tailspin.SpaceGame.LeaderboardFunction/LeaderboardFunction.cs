using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TailSpin.SpaceGame.LeaderboardFunction;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using TailSpin.SpaceGame.LeaderboardFunction.Models;

namespace TailSpin.SpaceGame.LeaderboardFunction
{
    public class LeaderboardFunction
    {
        private readonly IDocumentDBRepository _dbRespository;
        public LeaderboardFunction(IDocumentDBRepository dbRepository)
        {
            _dbRespository = dbRepository;
        }

        [FunctionName("LeaderboardFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Leaderboard function processed a request.");
            log.LogInformation("Repository Type: " + _dbRespository.GetRepositoryType());

            // Grab parameters from query string.
            string mode = req.Query["mode"];
            string region = req.Query["region"];

            int page;
            int.TryParse(req.Query["page"], out page);
            if (page < 1)
            {
                page = 1;
            }

            int pageSize;
            if (int.TryParse(req.Query["pageSize"], out pageSize))
            {
                pageSize = Math.Max(Math.Min(pageSize, 50), 1);
            }
            else
            {
                pageSize = 10;
            }

            // Create the baseline response.
            var leaderboardResponse = new LeaderboardResponse()
            {
                Page = page,
                PageSize = pageSize,
                SelectedMode = mode,
                SelectedRegion = region
            };

            // Fetch the total number of results in the background.
            var countItemsTask = this._dbRespository.CountScoresAsync(mode, region);

            // Fetch the scores that match the current filter.
            IEnumerable<Score> scores = await _dbRespository.GetScoresAsync(mode, region, page, pageSize);

            // Wait for the total count.
            leaderboardResponse.TotalResults = await countItemsTask;

            // Fetch the user profile for each score.
            // This creates a list that's parallel with the scores collection.
            var profiles = new List<Task<Profile>>();
            foreach (var score in scores)
            {
                profiles.Add(_dbRespository.GetProfileAsync(score.ProfileId));
            }
            Task<Profile>.WaitAll(profiles.ToArray());

            // Combine each score with its profile.
            leaderboardResponse.Scores = scores.Zip(profiles, (score, profile) => new ScoreProfile { Score = score, Profile = profile.Result });

            return (ActionResult)new OkObjectResult(leaderboardResponse);
        }
    }
}
