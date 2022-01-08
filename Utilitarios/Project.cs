using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("project", Schema = "road_project")]
    public class Project
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("customer_info_id"), Display(Name = "Info Cliente")]
        public int CustomerInfoId { get; set; }
        [Display(Name = "Info Cliente")]
        public CustomerInfo CustomerInfo { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        [Column("title"), Display(Name = "Nombre")]
        public string Title { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(500, ErrorMessage = "Longitud máxima de {1} caracteres")]
        [Column("description"), Display(Name = "Descripción")]
        public string Description { get; set; }

        [Column("creation_dt"), Display(Name = "Fecha Creación"), DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}")]
        public DateTime CreatedDate { get; set; }

        [Column("user_alias"), Display(Name = "Usuario")]
        public string UserAlias { get; set; }

        [Column("sync_source_id"), Display(Name = "Origen")]
        public int SyncSourceId { get; set; }
        [Display(Name = "Origen")]
        public SyncSource SyncSource { get; set; }

        // Navigation
        public ICollection<Subproject> Subprojects { get; set; }

    }
}
