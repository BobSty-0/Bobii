using System;
using System.ComponentModel.DataAnnotations;

namespace Bobii.src.EntityFramework.Entities
{
    class tempcommands
    {
        [Key]
        public long id { get; set; }
        public string commandname { get; set; }
        public bool enabled { get; set; }
        public ulong guildguid { get; set; }
    }
}
