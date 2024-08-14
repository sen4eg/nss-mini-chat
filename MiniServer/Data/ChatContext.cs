using Microsoft.EntityFrameworkCore;
using MiniServer.Data.Model;


namespace MiniServer.Data; 

public class ChatContext : DbContext{
    public ChatContext(DbContextOptions<ChatContext> options) : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<MiniServer.Data.Model.Message> Messages { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupSetting> GroupSettings { get; set; }
    public DbSet<GroupRole> GroupRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<ContactType> ContactTypes { get; set; }
    public DbSet<AuthenicatedToken> ValidationTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GroupRole>()
            .HasKey(gr => new { gr.UserId, gr.GroupId });

        modelBuilder.Entity<GroupRole>()
            .HasOne(gr => gr.User)
            .WithMany(u => u.GroupRoles)
            .HasForeignKey(gr => gr.UserId);

        modelBuilder.Entity<GroupRole>()
            .HasOne(gr => gr.Group)
            .WithMany(g => g.GroupRoles)
            .HasForeignKey(gr => gr.GroupId);

        modelBuilder.Entity<MiniServer.Data.Model.Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.UserId);

        modelBuilder.Entity<MiniServer.Data.Model.Message>()
            .HasMany(m => m.Attachments)
            .WithOne(a => a.Message)
            .HasForeignKey(a => a.MessageId);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        
        modelBuilder.Entity<User>()  
            .HasMany(u => u.GroupRoles)
            .WithOne(gr => gr.User)
            .HasForeignKey(gr => gr.UserId);

        modelBuilder.Entity<GroupSetting>()
            .HasOne(gs => gs.Group)
            .WithMany(g => g.GroupSettings)
            .HasForeignKey(gs => gs.GroupId);
        
        modelBuilder.Entity<Group>()
            .HasMany(g => g.GroupSettings)
            .WithOne(gs => gs.Group)
            .HasForeignKey(gs => gs.GroupId);

        modelBuilder.Entity<Group>()
            .HasMany(g => g.GroupRoles)
            .WithOne(gr => gr.Group)
            .HasForeignKey(gr => gr.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<GroupRole>()
            .HasMany(gr => gr.Permissions)
            .WithOne(p => p.GroupRole)
            .HasPrincipalKey(p => p.GroupRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Contact>()
            .HasOne(c => c.User)
            .WithMany(u => u.Contacts)
            .HasForeignKey(c => c.UserId);

        modelBuilder.Entity<Contact>()
            .HasOne(c => c.ContactType)
            .WithMany(ct => ct.Contacts)
            .HasForeignKey(c => c.ContactTypeId);
        
        modelBuilder.Entity<AuthenicatedToken>()
            .HasOne(u => u.User)
            .WithMany(ut => ut.AuthenicatedTokens)
            .HasForeignKey(t => t.Username)
            .HasPrincipalKey(ut => ut.Username);
    }
}
