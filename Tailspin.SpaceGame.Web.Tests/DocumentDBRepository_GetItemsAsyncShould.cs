using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NUnit.Framework;
using TailSpin.SpaceGame.LeaderboardFunction;

namespace Tests
{
    public class DocumentDBRepository_GetItemsAsyncShould
    {
        private IDocumentDBRepository _scoreRepository;

        [SetUp]
        public void Setup()
        {
            _scoreRepository = new LocalDocumentDBRepository(
                "SampleData/scores.json",
                "SampleData/profiles.json"
                ); 
        }

        [TestCase("1", ExpectedResult = "1")]
        public string FetchGameRegionById(string id)
        {
            // Fetch the scores.
            Task<Profile> profileTask = _scoreRepository.GetProfileAsync(
                id // item Id
            );
            Profile profile = profileTask.Result;

            // Verify that we received the specified number of items.
            return profile.Id;
        }

        [TestCase("", "Milky Way")]
        [TestCase("", "Andromeda")]
        [TestCase("", "Pinwheel")]
        [TestCase("", "NGC 1300")]
        [TestCase("", "Messier 82")]
        public void FetchOnlyRequestedGameRegion(string gameMode, string gameRegion)
        {
            const int PAGE = 0; // take the first page of results
            const int MAX_RESULTS = 10; // sample up to 10 results

            // Fetch the scores.
            Task<IEnumerable<Score>> scoresTask = _scoreRepository.GetScoresAsync(
                gameMode, 
                gameRegion, 
                PAGE,
                MAX_RESULTS
            );
            IEnumerable<Score> scores = scoresTask.Result;

            // Verify that each score's game region matches the provided game region.
            Assert.That(scores, Is.All.Matches<Score>(score => score.GameRegion == gameRegion));
        }

        [TestCase("", "NoSuchRegion")]
        public void FetchNonExistentGameRegion(string gameMode, string gameRegion) 
        {
            const int PAGE = 0; // take the first page of results
            const int MAX_RESULTS = 10; // sample up to 10 results

            // Fetch the scores.
            Task<IEnumerable<Score>> scoresTask = _scoreRepository.GetScoresAsync(
                gameMode,
                gameRegion,
                PAGE,
                MAX_RESULTS
            );
            IEnumerable<Score> scores = scoresTask.Result;

            // Verify that zero score in the non-existent game region.
            Assert.True(scores.Count() == 0);            
        }

        [TestCase("", "Milky Way", 0, ExpectedResult=0)]
        [TestCase("", "Andromeda", 1, ExpectedResult=1)]
        [TestCase("", "Pinwheel",  2, ExpectedResult=2)]
        public int ReturnRequestedCount(string gameMode, string gameRegion, int count)
        {
            const int PAGE = 0; // take the first page of results

            // Fetch the scores.
            Task<IEnumerable<Score>> scoresTask = _scoreRepository.GetScoresAsync(
                gameMode,
                gameRegion,
                PAGE,
                count // fetch this number of results
            );
            IEnumerable<Score> scores = scoresTask.Result;

            // Verify that we received the specified number of items.
            return scores.Count();
        }
    }
}
