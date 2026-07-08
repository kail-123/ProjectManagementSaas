using ProjectManagementSaaS.Application.Features.Work;
using ProjectManagementSaaS.Domain.Work;
using Xunit;

namespace ProjectManagementSaaS.Application.Tests.Features.Work;

public sealed class WorkActivityCategoryTests
{
    [Theory]
    [InlineData(WorkActivityType.CommentAdded, WorkActivityCategory.Comments)]
    [InlineData(WorkActivityType.ReplyAdded, WorkActivityCategory.Comments)]
    [InlineData(WorkActivityType.AssignmentChanged, WorkActivityCategory.Assignments)]
    [InlineData(WorkActivityType.StatusChanged, WorkActivityCategory.StatusChanges)]
    [InlineData(WorkActivityType.PriorityChanged, WorkActivityCategory.Priority)]
    [InlineData(WorkActivityType.AttachmentAdded, WorkActivityCategory.Attachments)]
    [InlineData(WorkActivityType.MentionAdded, WorkActivityCategory.Mentions)]
    [InlineData(WorkActivityType.ReactionAdded, WorkActivityCategory.Reactions)]
    [InlineData(WorkActivityType.ProjectMembershipChanged, WorkActivityCategory.ProjectMembership)]
    [InlineData(WorkActivityType.TitleChanged, WorkActivityCategory.System)]
    public void ActivityTypeMapsToExpectedCategory(WorkActivityType activityType, WorkActivityCategory expected)
    {
        var category = WorkActivities.GetCategory(activityType);

        Assert.Equal(expected, category);
    }
}
