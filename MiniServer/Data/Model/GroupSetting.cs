﻿using System.ComponentModel.DataAnnotations;

namespace MiniServer.Data.Model;

public class GroupSetting
{
    public GroupSetting(string name, string value, Group group) {
        Name = name;
        Value = value;
        Group = group;
    }

    [Key]
    public int GroupSettingId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Value { get; set; }

    // Foreign key for the group this setting belongs to
    public int GroupId { get; set; }

    // Navigation property for the group this setting belongs to
    public Group Group { get; set; }
}