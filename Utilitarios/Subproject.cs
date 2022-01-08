using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("subproject", Schema = "road_project")]
    public class Subproject
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("project_id"), Display(Name = "Proyecto Padre")]
        public int ProjectId { get; set; }
        [Display(Name = "Proyecto Padre")]
        public Project Project { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        [Column("title"), Display(Name = "Nombre actividad")]
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
    }
}
