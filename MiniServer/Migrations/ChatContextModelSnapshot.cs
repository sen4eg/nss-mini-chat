﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MiniServer.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MiniServer.Migrations
{
    [DbContext(typeof(ChatContext))]
    partial class ChatContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MiniServer.Data.Model.Attachment", b =>
                {
                    b.Property<long>("AttachmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("AttachmentId"));

                    b.Property<byte[]>("Content")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("Filename")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("MessageId")
                        .HasColumnType("bigint");

                    b.HasKey("AttachmentId");

                    b.HasIndex("MessageId");

                    b.ToTable("Attachments");
                });

            modelBuilder.Entity("MiniServer.Data.Model.AuthenicatedToken", b =>
                {
                    b.Property<string>("Token")
                        .HasColumnType("text");

                    b.Property<string>("Device")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ip")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Token");

                    b.HasIndex("Username");

                    b.ToTable("ValidationTokens");
                });

            modelBuilder.Entity("MiniServer.Data.Model.Contact", b =>
                {
                    b.Property<long>("ContactId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("ContactId"));

                    b.Property<int>("ContactTypeId")
                        .HasColumnType("integer");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("ContactId");

                    b.HasIndex("ContactTypeId");

                    b.HasIndex("UserId");

                    b.ToTable("Contacts");
                });

            modelBuilder.Entity("MiniServer.Data.Model.ContactType", b =>
                {
                    b.Property<int>("ContactTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ContactTypeId"));

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ContactTypeId");

                    b.ToTable("ContactTypes");
                });

            modelBuilder.Entity("MiniServer.Data.Model.Group", b =>
                {
                    b.Property<long>("GroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("GroupId"));

                    b.Property<int>("CreatorUserId")
                        .HasColumnType("integer");

                    b.Property<long>("CreatorUserUserId")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("GroupId");

                    b.HasIndex("CreatorUserUserId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("MiniServer.Data.Model.GroupRole", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<long>("GroupId")
                        .HasColumnType("bigint");

                    b.Property<int>("GroupRoleId")
                        .HasColumnType("integer");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("UserId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("GroupRoles");
                });

            modelBuilder.Entity("MiniServer.Data.Model.GroupSetting", b =>
                {
                    b.Property<int>("GroupSettingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("GroupSettingId"));

                    b.Property<long>("GroupId")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("GroupSettingId");

                    b.HasIndex("GroupId");

                    b.ToTable("GroupSettings");
                });

            modelBuilder.Entity("MiniServer.Data.Model.Message", b =>
                {
                    b.Property<long>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("MessageId"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("ReceiverId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<bool>("isDeleted")
                        .HasColumnType("boolean");

                    b.HasKey("MessageId");

                    b.HasIndex("UserId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("MiniServer.Data.Model.Permission", b =>
                {
                    b.Property<long>("PermissionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("PermissionId"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("GroupRoleId")
                        .HasColumnType("integer");

                    b.Property<string>("PermissionName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("PermissionId");

                    b.HasIndex("GroupRoleId");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("MiniServer.Data.Model.User", b =>
                {
                    b.Property<long>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("UserId"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("UserId");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("MiniServer.Data.Model.Attachment", b =>
                {
                    b.HasOne("MiniServer.Data.Model.Message", "Message")
                        .WithMany("Attachments")
                        .HasForeignKey("MessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Message");
                });

            modelBuilder.Entity("MiniServer.Data.Model.AuthenicatedToken", b =>
                {
                    b.HasOne("MiniServer.Data.Model.User", "User")
                        .WithMany("AuthenicatedTokens")
                        .HasForeignKey("Username")
                        .HasPrincipalKey("Username")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MiniServer.Data.Model.Contact", b =>
                {
                    b.HasOne("MiniServer.Data.Model.ContactType", "ContactType")
                        .WithMany("Contacts")
                        .HasForeignKey("ContactTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MiniServer.Data.Model.User", "User")
                        .WithMany("Contacts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ContactType");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MiniServer.Data.Model.Group", b =>
                {
                    b.HasOne("MiniServer.Data.Model.User", "CreatorUser")
                        .WithMany()
                        .HasForeignKey("CreatorUserUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatorUser");
                });

            modelBuilder.Entity("MiniServer.Data.Model.GroupRole", b =>
                {
                    b.HasOne("MiniServer.Data.Model.Group", "Group")
                        .WithMany("GroupRoles")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MiniServer.Data.Model.User", "User")
                        .WithMany("GroupRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MiniServer.Data.Model.GroupSetting", b =>
                {
                    b.HasOne("MiniServer.Data.Model.Group", "Group")
                        .WithMany("GroupSettings")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("MiniServer.Data.Model.Message", b =>
                {
                    b.HasOne("MiniServer.Data.Model.User", "Sender")
                        .WithMany("SentMessages")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("MiniServer.Data.Model.Permission", b =>
                {
                    b.HasOne("MiniServer.Data.Model.GroupRole", "GroupRole")
                        .WithMany("Permissions")
                        .HasForeignKey("GroupRoleId")
                        .HasPrincipalKey("GroupRoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GroupRole");
                });

            modelBuilder.Entity("MiniServer.Data.Model.ContactType", b =>
                {
                    b.Navigation("Contacts");
                });

            modelBuilder.Entity("MiniServer.Data.Model.Group", b =>
                {
                    b.Navigation("GroupRoles");

                    b.Navigation("GroupSettings");
                });

            modelBuilder.Entity("MiniServer.Data.Model.GroupRole", b =>
                {
                    b.Navigation("Permissions");
                });

            modelBuilder.Entity("MiniServer.Data.Model.Message", b =>
                {
                    b.Navigation("Attachments");
                });

            modelBuilder.Entity("MiniServer.Data.Model.User", b =>
                {
                    b.Navigation("AuthenicatedTokens");

                    b.Navigation("Contacts");

                    b.Navigation("GroupRoles");

                    b.Navigation("SentMessages");
                });
#pragma warning restore 612, 618
        }
    }
}
