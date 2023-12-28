using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IDZ.Models.ViewModels
{
    public class MasterVM
    {
        public Guid masterID { get; set; }
        [Required]
        [DisplayName("Фамилия")]

        public string surname { get; set; }
        [Required]
        [DisplayName("Имя")]
        public string name { get; set; }
        [DisplayName("Отчество")]
        public string patronymic { get; set; }


    }
}