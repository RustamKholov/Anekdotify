using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace api.Data;

public partial class ApplicationDBContext : IdentityDbContext<User>
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Classification> Classifications { get; set; }

    public virtual DbSet<Source> Sources { get; set; }

    public virtual DbSet<CommentRating> CommentRatings { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<JokeLike> JokeLikes { get; set; }

    public virtual DbSet<JokePart> JokeParts { get; set; }

    public virtual DbSet<JokeRating> JokeRatings { get; set; }

    public virtual DbSet<Joke> Jokes { get; set; }

    public virtual DbSet<UserSavedJoke> UserSavedJokes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        List<IdentityRole> roles = new List<IdentityRole>
        {
            new IdentityRole
            {
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
            new IdentityRole
            {
                Name = "Moderator",
                NormalizedName = "MODERATOR"
            },
            new IdentityRole
            {
                Name = "User",
                NormalizedName = "USER"
            }
        };
        List<Source> sources = new List<Source>
        {
            new Source
            {
                SourceId = -1,
                SourceName = "From User"
            },
            new Source
            {
                SourceId = -2,
                SourceName = "System"
            },
            new Source
            {
                SourceId = -3,
                SourceName = "Generated"
            }
        };
        modelBuilder.Entity<Source>(entity =>
        {
            entity.HasKey(e => e.SourceId);
            entity.Property(e => e.SourceName).HasMaxLength(50);
        });

        modelBuilder.Entity<Source>().HasData(sources);

        modelBuilder.Entity<IdentityRole>().HasData(roles);

        modelBuilder.Entity<Classification>(entity =>
        {
            entity.HasKey(e => e.ClassificationId);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<CommentRating>(entity =>
        {
            entity.HasKey(e => e.CommentRateId);

            entity.HasIndex(e => new { e.CommentId, e.UserId }, "IX_CommentRatings").IsUnique();

            entity.Property(e => e.RatingDate).HasColumnType("datetime");

            entity.HasOne(d => d.Comment).WithMany(p => p.CommentRatings)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.Cascade)
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
                .OnDelete(DeleteBehavior.Cascade);

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
                .OnDelete(DeleteBehavior.Cascade)
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
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_JokeParts_Jokes");
        });

        modelBuilder.Entity<JokeRating>(entity =>
        {
            entity.HasKey(e => e.RatingId);

            entity.Property(e => e.RatingDate).HasColumnType("datetime");

            entity.HasOne(d => d.Joke).WithMany(p => p.JokeRatings)
                .HasForeignKey(d => d.JokeId)
                .OnDelete(DeleteBehavior.Cascade)
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
            entity.Property(e => e.SubbmissionDate).HasColumnType("datetime");

            entity.HasOne(d => d.Classification).WithMany(p => p.Jokes)
                .HasForeignKey(d => d.ClassificationId)
                .HasConstraintName("FK_Jokes_Classification");
                
            entity.HasOne(d => d.Source).WithMany(p => p.Jokes)
                .HasForeignKey(d => d.SourceId)
                .HasConstraintName("FK_Jokes_Source");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.LastJokeRetrievalDate).HasColumnType("datetime");
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.RegistrationDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<UserSavedJoke>(entity =>
        {
            entity.HasKey(e => e.UserSavedJokeId);

            entity.HasIndex(e => new { e.JokeId, e.UserId }, "IX_UserSavedJokes").IsUnique();

            entity.Property(e => e.SavedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Joke).WithMany(p => p.UserSavedJokes)
                .HasForeignKey(d => d.JokeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserSavedJokes_Jokes");

            entity.HasOne(d => d.User).WithMany(p => p.UserSavedJokes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserSavedJokes_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
