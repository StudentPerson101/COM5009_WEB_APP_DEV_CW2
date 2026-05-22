using System.ComponentModel.DataAnnotations;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Tests;

public class ViewModelValidationTests
{
    [Fact]
    public void RegisterRequiresValidEmailAndMatchingPasswordConfirmation()
    {
        var model = new RegisterViewModel
        {
            UserName = "movie.user",
            Email = "not-an-email",
            Password = "ValidPass!1",
            ConfirmPassword = "DifferentPass!1"
        };

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(RegisterViewModel.Email)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(RegisterViewModel.ConfirmPassword)));
    }

    [Fact]
    public void LoginRequiresEmailAndPassword()
    {
        var model = new LoginViewModel();

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(LoginViewModel.Email)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(LoginViewModel.Password)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void RatingScoreMustBeOneThroughFive(int score)
    {
        var model = new RatingFormViewModel
        {
            MovieId = 1,
            Score = score
        };

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(RatingFormViewModel.Score)));
    }

    [Fact]
    public void ReviewTextHasMaximumLength()
    {
        var model = new ReviewFormViewModel
        {
            MovieId = 1,
            Text = new string('x', 4001)
        };

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ReviewFormViewModel.Text)));
    }

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }
}
