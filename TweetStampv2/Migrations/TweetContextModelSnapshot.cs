// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TweetStampv2.Data;

namespace TweetStampv2.Migrations
{
    [DbContext(typeof(TweetContext))]
    partial class TweetContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.16")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TweetStampv2.Models.TweetModel", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<string>("CreatedAt")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmbbededTweetHTML")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Hash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Json")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MediaUrl1")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MediaUrl2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MediaUrl3")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MediaUrl4")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TimeStampInfo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("TsByteArr")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserFullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserProfileImgUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserScreenName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ValidationDescription")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Tweets");
                });
#pragma warning restore 612, 618
        }
    }
}
