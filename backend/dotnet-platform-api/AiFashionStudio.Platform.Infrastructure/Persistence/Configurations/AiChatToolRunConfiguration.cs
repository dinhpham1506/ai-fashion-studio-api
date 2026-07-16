using AiFashionStudio.Platform.Domain.AiChat.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class AiChatToolRunConfiguration : IEntityTypeConfiguration<AiChatToolRun>
{
    public void Configure(EntityTypeBuilder<AiChatToolRun> builder)
    {
        builder.ToTable("tool_runs", DatabaseSchemas.AiChat);
        builder.HasKey(toolRun => toolRun.Id);

        builder.Property(toolRun => toolRun.Id).HasColumnName("id");
        builder.Property(toolRun => toolRun.ConversationId).HasColumnName("conversation_id");
        builder.Property(toolRun => toolRun.ToolName).HasColumnName("tool_name").HasMaxLength(120).IsRequired();
        builder.Property(toolRun => toolRun.InputJson).HasColumnName("input_json").HasColumnType("jsonb");
        builder.Property(toolRun => toolRun.OutputSummaryJson).HasColumnName("output_summary_json").HasColumnType("jsonb");
        builder.Property(toolRun => toolRun.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(toolRun => toolRun.ErrorMessage).HasColumnName("error_message");
        builder.Property(toolRun => toolRun.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(toolRun => toolRun.ConversationId);
        builder.HasIndex(toolRun => toolRun.ToolName);

        builder.HasOne<AiChatConversation>()
            .WithMany()
            .HasForeignKey(toolRun => toolRun.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

