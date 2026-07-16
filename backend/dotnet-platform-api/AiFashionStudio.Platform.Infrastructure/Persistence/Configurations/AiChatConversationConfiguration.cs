using AiFashionStudio.Platform.Domain.AiChat.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class AiChatConversationConfiguration : IEntityTypeConfiguration<AiChatConversation>
{
    public void Configure(EntityTypeBuilder<AiChatConversation> builder)
    {
        builder.ToTable("conversations", DatabaseSchemas.AiChat);
        builder.HasKey(conversation => conversation.Id);

        builder.Property(conversation => conversation.Id).HasColumnName("id");
        builder.Property(conversation => conversation.UserId).HasColumnName("user_id");
        builder.Property(conversation => conversation.UserRole).HasColumnName("user_role").HasMaxLength(50);
        builder.Property(conversation => conversation.Channel).HasColumnName("channel").HasMaxLength(50).IsRequired();
        builder.Property(conversation => conversation.PageType).HasColumnName("page_type").HasMaxLength(50);
        builder.Property(conversation => conversation.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(conversation => conversation.RelatedProductId).HasColumnName("related_product_id");
        builder.Property(conversation => conversation.RelatedOrderId).HasColumnName("related_order_id");
        builder.Property(conversation => conversation.CreatedAt).HasColumnName("created_at");
        builder.Property(conversation => conversation.UpdatedAt).HasColumnName("updated_at");
        builder.Property(conversation => conversation.ResolvedAt).HasColumnName("resolved_at");

        builder.HasIndex(conversation => conversation.UserId);
        builder.HasIndex(conversation => conversation.Status);
        builder.HasIndex(conversation => conversation.RelatedProductId);
        builder.HasIndex(conversation => conversation.RelatedOrderId);
    }
}

