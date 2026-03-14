// // @file RecentProject.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RetroEngine.Editor.Core.Data.Entities;

[Index(nameof(Path), IsUnique = true)]
[Index(nameof(LastOpened))]
public sealed class RecentProject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public required string Path { get; set; } = "";

    public required string Name { get; set; } = "";

    public required DateTime LastOpened { get; set; }
}
