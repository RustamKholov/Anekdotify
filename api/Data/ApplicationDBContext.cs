using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Data;

public partial class ApplicationDBContext : DbContext
{
    public ApplicationDBContext()
    {
    }

    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Classification> Classifications { get; set; }

    public virtual DbSet<CommentRating> CommentRatings { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<JokeLike> JokeLikes { get; set; }

    public virtual DbSet<JokePart> JokeParts { get; set; }

    public virtual DbSet<JokeRating> JokeRatings { get; set; }

    public virtual DbSet<Joke> Jokes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSavedJoke> UserSavedJokes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Classification>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<CommentRating>(entity =>
        {
            entity.HasKey(e => e.CommentRateId);

            entity.HasIndex(e => new { e.CommentId, e.UserId }, "IX_CommentRatings").IsUnique();

            entity.Property(e => e.RatingDate).HasColumnType("datetime");

            entity.HasOne(d => d.Comment).WithMany(p => p.CommentRatings)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommentRatings_Comments");

            entity.HasOne(d => d.User).WithMany(p => p.CommentRatings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommentRatings_User");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId);

            entity.HasIndex(e => e.JokeId, "IX_Comments_JokeId");

            entity.Property(e => e.CommentDate).HasColumnType("datetime");

            entity.HasOne(d => d.Joke).WithMany(p => p.Comments)
                .HasForeignKey(d => d.JokeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comments_User");
        });

        modelBuilder.Entity<JokeLike>(entity =>
        {
            entity.HasKey(e => e.LikeId);

            entity.HasIndex(e => new { e.JokeId, e.UserId }, "UQ_UserJokeLike").IsUnique();

            entity.Property(e => e.LikeDate).HasColumnType("datetime");

            entity.HasOne(d => d.Joke).WithMany(p => p.JokeLikes)
                .HasForeignKey(d => d.JokeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JokeLikes_Jokes");

            entity.HasOne(d => d.User).WithMany(p => p.JokeLikes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JokeLikes_User");
        });

        modelBuilder.Entity<JokePart>(entity =>
        {
            entity.HasKey(e => e.JokePartId);

            entity.Property(e => e.PartType).HasMaxLength(50);
            entity.Property(e => e.SubmissionDate).HasColumnType("datetime");

            entity.HasOne(d => d.AssociatedJoke).WithMany(p => p.JokeParts)
                .HasForeignKey(d => d.AssociatedJokeId)
                .HasConstraintName("FK_JokeParts_Jokes");
        });

        modelBuilder.Entity<JokeRating>(entity =>
        {
            entity.HasKey(e => e.RatingId);

            entity.Property(e => e.RatingDate).HasColumnType("datetime");

            entity.HasOne(d => d.Joke).WithMany(p => p.JokeRatings)
                .HasForeignKey(d => d.JokeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JokeRatings_Jokes");

            entity.HasOne(d => d.User).WithMany(p => p.JokeRatings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JokeRatings_User");
        });

        modelBuilder.Entity<Joke>(entity =>
        {
            entity.HasKey(e => e.JokeId);

            entity.Property(e => e.ApprovalDate).HasColumnType("datetime");
            entity.Property(e => e.Source).HasMaxLength(50);
            entity.Property(e => e.SubbmissionDate).HasColumnType("datetime");

            entity.HasOne(d => d.Classification).WithMany(p => p.Jokes)
                .HasForeignKey(d => d.ClassificationId)
                .HasConstraintName("FK_Jokes_Classification");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.LastJokeRetrievalDate).HasColumnType("datetime");
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.RegistrationDate).HasColumnType("datetime");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsFixedLength();
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<UserSavedJoke>(entity =>
        {
            entity.HasKey(e => e.UserSavedJokeId);

            entity.HasIndex(e => new { e.JokeId, e.UserId }, "IX_UserSavedJokes").IsUnique();

            entity.Property(e => e.SavedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Joke).WithMany(p => p.UserSavedJokes)
                .HasForeignKey(d => d.JokeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSavedJokes_Jokes");

            entity.HasOne(d => d.User).WithMany(p => p.UserSavedJokes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSavedJokes_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
