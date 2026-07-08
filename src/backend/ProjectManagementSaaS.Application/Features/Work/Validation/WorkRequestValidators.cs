using FluentValidation;
using ProjectManagementSaaS.Application.Features.Work.Contracts;

namespace ProjectManagementSaaS.Application.Features.Work.Validation;

public sealed class WorkItemUpsertRequestValidator : AbstractValidator<WorkItemUpsertRequest>
{
    public WorkItemUpsertRequestValidator()
    {
        RuleFor(request => request.ProjectId).NotEmpty();
        RuleFor(request => request.WorkItemTypeId).NotEmpty();
        RuleFor(request => request.PriorityId).NotEmpty();
        RuleFor(request => request.ReporterUserProfileId).NotEmpty();
        RuleFor(request => request.Title).NotEmpty().MaximumLength(512);
        RuleFor(request => request.Description).MaximumLength(8192);
        RuleFor(request => request.AcceptanceCriteria).MaximumLength(4096);
        RuleFor(request => request.EstimatedHours).GreaterThanOrEqualTo(0).When(request => request.EstimatedHours.HasValue);
        RuleFor(request => request.LoggedHours).GreaterThanOrEqualTo(0);
        RuleFor(request => request.StoryPoints).GreaterThanOrEqualTo(0).When(request => request.StoryPoints.HasValue);
        RuleFor(request => request.Environment).MaximumLength(256);
        RuleFor(request => request.Severity).MaximumLength(64);
        RuleFor(request => request.LabelIds).NotNull().Must(BeUnique).WithMessage("Labels must be unique.");
        RuleFor(request => request.ComponentIds).NotNull().Must(BeUnique).WithMessage("Components must be unique.");
        RuleFor(request => request.WatcherUserProfileIds).NotNull().Must(BeUnique).WithMessage("Watchers must be unique.");
        RuleFor(request => request)
            .Must(request => !request.StartDate.HasValue || !request.DueDate.HasValue || request.DueDate.Value >= request.StartDate.Value)
            .WithMessage("Due date must be on or after start date.");
    }

    private static bool BeUnique(IReadOnlyCollection<Guid> ids)
    {
        return ids.Distinct().Count() == ids.Count;
    }
}

public sealed class WorkTransitionRequestValidator : AbstractValidator<WorkTransitionRequest>
{
    public WorkTransitionRequestValidator()
    {
        RuleFor(request => request.ToStatusId).NotEmpty();
        RuleFor(request => request.RowVersion).NotEmpty();
    }
}

public sealed class WorkCommentUpsertRequestValidator : AbstractValidator<WorkCommentUpsertRequest>
{
    public WorkCommentUpsertRequestValidator()
    {
        RuleFor(request => request.RequiredMessage).NotEmpty().MaximumLength(8192);
        RuleFor(request => request.MentionUserProfileIds).NotNull().Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Mentions must be unique.");
        RuleFor(request => request.MentionedUserIds).NotNull().Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Mentions must be unique.");
    }
}

public sealed class WorkCommentMentionsRequestValidator : AbstractValidator<WorkCommentMentionsRequest>
{
    public WorkCommentMentionsRequestValidator()
    {
        RuleFor(request => request.MentionedUserIds).NotEmpty().Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Mentions must be unique.");
    }
}

public sealed class WorkCommentReactionRequestValidator : AbstractValidator<WorkCommentReactionRequest>
{
    public WorkCommentReactionRequestValidator()
    {
        RuleFor(request => request.Emoji).NotEmpty().MaximumLength(16);
    }
}

public sealed class WorkCommentAttachmentRequestValidator : AbstractValidator<WorkCommentAttachmentRequest>
{
    public WorkCommentAttachmentRequestValidator()
    {
        RuleFor(request => request.FileName).NotEmpty().MaximumLength(256);
        RuleFor(request => request.ContentType).NotEmpty().MaximumLength(128);
        RuleFor(request => request.FileSizeBytes).GreaterThanOrEqualTo(0);
        RuleFor(request => request.StoragePath).NotEmpty().MaximumLength(1024);
    }
}

public sealed class WorkAttachmentRequestValidator : AbstractValidator<WorkAttachmentRequest>
{
    public WorkAttachmentRequestValidator()
    {
        RuleFor(request => request.FileName).NotEmpty().MaximumLength(256);
        RuleFor(request => request.ContentType).NotEmpty().MaximumLength(128);
        RuleFor(request => request.FileSizeBytes).GreaterThanOrEqualTo(0);
        RuleFor(request => request.StoragePath).NotEmpty().MaximumLength(1024);
        RuleFor(request => request.Version).GreaterThan(0);
        RuleFor(request => request.Description).MaximumLength(512);
    }
}

public sealed class WorkItemLinkRequestValidator : AbstractValidator<WorkItemLinkRequest>
{
    public WorkItemLinkRequestValidator()
    {
        RuleFor(request => request.TargetWorkItemId).NotEmpty();
        RuleFor(request => request.LinkType).IsInEnum();
        RuleFor(request => request.Description).MaximumLength(512);
    }
}

public sealed class WorkSavedFilterRequestValidator : AbstractValidator<WorkSavedFilterRequest>
{
    public WorkSavedFilterRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(128);
        RuleFor(request => request.QueryJson).NotEmpty().MaximumLength(4096);
    }
}

public sealed class WorkLabelUpsertRequestValidator : AbstractValidator<WorkLabelUpsertRequest>
{
    public WorkLabelUpsertRequestValidator()
    {
        RuleFor(request => request.OrganizationId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Color).NotEmpty().MaximumLength(32);
        RuleFor(request => request.Description).MaximumLength(512);
    }
}

public sealed class WorkComponentUpsertRequestValidator : AbstractValidator<WorkComponentUpsertRequest>
{
    public WorkComponentUpsertRequestValidator()
    {
        RuleFor(request => request.ProjectId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Description).MaximumLength(512);
        RuleFor(request => request.Color).NotEmpty().MaximumLength(32);
    }
}
