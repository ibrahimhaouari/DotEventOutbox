﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace DotEventOutbox.Infrastructure.EntityFramework.Migrations
{
    [DbContext(typeof(OutboxDbContext))]
    partial class OutboxDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Outbox")
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("DotEventOutbox.Entities.DeadLetterMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Error")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LastErrorOccurredOnUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("OccurredOnUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Retries")
                        .HasColumnType("integer");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("Id");

                    b.ToTable("DeadLetterMessages", "Outbox");
                });

            modelBuilder.Entity("DotEventOutbox.Entities.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("OccurredOnUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ProcessedOnUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("Id");

                    b.ToTable("OutboxMessages", "Outbox");
                });

            modelBuilder.Entity("DotEventOutbox.Entities.OutboxMessagesConsumer", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("Id", "Name");

                    b.ToTable("OutboxMessageConsumers", "Outbox");
                });
#pragma warning restore 612, 618
        }
    }
}
