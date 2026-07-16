using AiFashionStudio.Platform.Domain.AiChat.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class AiChatSupportTicketConfiguration : IEntityTypeConfiguration<AiChatSupportTicket>
{
    public void Configure(EntityTypeBuilder<AiChatSupportTicket> builder)
    {
        builder.ToTable("support_tickets", DatabaseSchemas.AiChat);
        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.Id).HasColumnName("id");
        builder.Property(ticket => ticket.ConversationId).HasColumnName("conversation_id");
        builder.Property(ticket => ticket.UserId).HasColumnName("user_id");
        builder.Property(ticket => ticket.OrderId).HasColumnName("order_id");
        builder.Property(ticket => ticket.IssueType).HasColumnName("issue_type").HasMaxLength(80).IsRequired();
        builder.Property(ticket => ticket.Severity).HasColumnName("severity").HasMaxLength(30).IsRequired();
        builder.Property(ticket => ticket.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(ticket => ticket.AssignedTo).HasColumnName("assigned_to");
        builder.Property(ticket => ticket.Summary).HasColumnName("summary").IsRequired();
        builder.Property(ticket => ticket.CreatedAt).HasColumnName("created_at");
        builder.Property(ticket => ticket.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(ticket => ticket.ConversationId);
        builder.HasIndex(ticket => ticket.UserId);
        builder.HasIndex(ticket => ticket.OrderId);
        builder.HasIndex(ticket => ticket.Status);

        builder.HasOne<AiChatConversation>()
            .WithMany()
            .HasForeignKey(ticket => ticket.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

