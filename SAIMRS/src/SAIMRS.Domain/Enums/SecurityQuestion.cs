namespace SAIMRS.Domain.Enums;

/// <summary>Questions the admin can choose from for password recovery.</summary>
public enum SecurityQuestion
{
    FirstPetName = 1,
    MotherMaidenName = 2,
    BirthCity = 3,
    ElementarySchool = 4,
    ChildhoodNickname = 5,
    FavoriteFood = 6
}

public static class SecurityQuestionExtensions
{
    public static string ToQuestionText(this SecurityQuestion question) => question switch
    {
        SecurityQuestion.FirstPetName => "What is the name of your first pet?",
        SecurityQuestion.MotherMaidenName => "What is your mother's maiden name?",
        SecurityQuestion.BirthCity => "In what city were you born?",
        SecurityQuestion.ElementarySchool => "What was the name of your elementary school?",
        SecurityQuestion.ChildhoodNickname => "What was your childhood nickname?",
        SecurityQuestion.FavoriteFood => "What is your favorite food?",
        _ => question.ToString()
    };
}
