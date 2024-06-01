using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveGuardian.ConsoleApp.Data.Entities;

public class Files
{
    [Key]
    public int ID { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public string OwnHash { get; set; }
    public string TotalHash { get; set; }
}
