using Gmach.Core.Entities;
using Xunit;

namespace Gmach.Tests
{
    public class UserScoreTests
    {
        [Fact]
        public void ChangeScore_Positive_AddsPoints()
        {
            var user = new User { Score = 5 };

            user.ChangeScore(10);

            Assert.Equal(15, user.Score);
        }

        [Fact]
        public void ChangeScore_PenaltyLargerThanScore_StopsAtZero()
        {
            var user = new User { Score = 5 };

            user.ChangeScore(-20);

            Assert.Equal(0, user.Score);
        }
    }
}
