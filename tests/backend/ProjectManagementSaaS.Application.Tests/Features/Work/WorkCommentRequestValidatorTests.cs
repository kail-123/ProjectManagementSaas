using ProjectManagementSaaS.Application.Features.Work.Contracts;
using ProjectManagementSaaS.Application.Features.Work.Validation;
using Xunit;

namespace ProjectManagementSaaS.Application.Tests.Features.Work;

public sealed class WorkCommentRequestValidatorTests
{
    [Fact]
    public void CommentUpsertAcceptsMessage()
    {
        var validator = new WorkCommentUpsertRequestValidator();
        var request = new WorkCommentUpsertRequest
        {
            Message = "Please review the deployment checklist.",
            MentionUserProfileIds = []
        };

        var result = validator.Validate(request);

        Assert.Empty(result.Errors);
    }

    [Fact]
    public void CommentUpsertAcceptsLegacyBodyMarkdown()
    {
        var validator = new WorkCommentUpsertRequestValidator();
        var request = new WorkCommentUpsertRequest
        {
            BodyMarkdown = "**Legacy markdown body**",
            MentionUserProfileIds = []
        };

        var result = validator.Validate(request);

        Assert.Empty(result.Errors);
    }

    [Fact]
    public void CommentUpsertRejectsEmptyMessage()
    {
        var validator = new WorkCommentUpsertRequestValidator();
        var request = new WorkCommentUpsertRequest
        {
            Message = " ",
            BodyMarkdown = null,
            MentionUserProfileIds = []
        };

        var result = validator.Validate(request);

        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(WorkCommentUpsertRequest.RequiredMessage));
    }

    [Fact]
    public void CommentUpsertRejectsDuplicateMentions()
    {
        var mentionedUserId = Guid.NewGuid();
        var validator = new WorkCommentUpsertRequestValidator();
        var request = new WorkCommentUpsertRequest
        {
            Message = "Looping in the same user twice should fail.",
            MentionUserProfileIds = [mentionedUserId, mentionedUserId]
        };

        var result = validator.Validate(request);

        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(WorkCommentUpsertRequest.MentionUserProfileIds));
    }

    [Fact]
    public void CommentUpsertRejectsDuplicateMentionedUsers()
    {
        var mentionedUserId = Guid.NewGuid();
        var validator = new WorkCommentUpsertRequestValidator();
        var request = new WorkCommentUpsertRequest
        {
            Message = "Looping in the same project member twice should fail.",
            MentionUserProfileIds = [],
            MentionedUserIds = [mentionedUserId, mentionedUserId]
        };

        var result = validator.Validate(request);

        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(WorkCommentUpsertRequest.MentionedUserIds));
    }

    [Fact]
    public void CommentMentionsRequestRequiresUniqueMentionedUsers()
    {
        var mentionedUserId = Guid.NewGuid();
        var validator = new WorkCommentMentionsRequestValidator();
        var request = new WorkCommentMentionsRequest([mentionedUserId, mentionedUserId]);

        var result = validator.Validate(request);

        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(WorkCommentMentionsRequest.MentionedUserIds));
    }

    [Fact]
    public void CommentReactionRequestRequiresEmoji()
    {
        var validator = new WorkCommentReactionRequestValidator();
        var request = new WorkCommentReactionRequest(string.Empty);

        var result = validator.Validate(request);

        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(WorkCommentReactionRequest.Emoji));
    }

    [Fact]
    public void CommentAttachmentRequiresFileMetadata()
    {
        var validator = new WorkCommentAttachmentRequestValidator();
        var request = new WorkCommentAttachmentRequest(
            FileName: string.Empty,
            ContentType: string.Empty,
            FileSizeBytes: -1,
            StoragePath: string.Empty);

        var result = validator.Validate(request);

        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(WorkCommentAttachmentRequest.FileName));
        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(WorkCommentAttachmentRequest.ContentType));
        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(WorkCommentAttachmentRequest.FileSizeBytes));
        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(WorkCommentAttachmentRequest.StoragePath));
    }
}
