using AiFashionStudio.Platform.Domain.AiChat.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class AiChatMessageConfiguration : IEntityTypeConfiguration<AiChatMessage>
{
    public void Configure(EntityTypeBuilder<AiChatMessage> builder)
    {
        builder.ToTable("messages", DatabaseSchemas.AiChat);
        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id).HasColumnName("id");
        builder.Property(message => message.ConversationId).HasColumnName("conversation_id");
        builder.Property(message => message.SenderType).HasColumnName("sender_type").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(message => message.Content).HasColumnName("content").IsRequired();
        builder.Property(message => message.Intent).HasColumnName("intent").HasMaxLength(80);
        builder.Property(message => message.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        builder.Property(message => message.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(message => message.ConversationId);
        builder.HasIndex(message => message.Intent);

        builder.HasOne<AiChatConversation>()
            .WithMany()
            .HasForeignKey(message => message.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

