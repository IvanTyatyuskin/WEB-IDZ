using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IDZ.Models.ViewModels
{
    public class MSVM
    {



        [Required]
        [DisplayName("Время выполнения услуги")]
        public TimeSpan ExecutionTime { get; set; }
        [Required]
        [DisplayName("Мастер")]
        public Guid MasterID { get; set; }
        [Required]
        [DisplayName("Услуга")]
        public string ServiceName { get; set; }




        // Для выпадающего списка мастеров
        public List<SelectListItem> MastersList { get; set; }

        // Для выпадающего списка услуг
        public List<SelectListItem> ServicesList { get; set; }

    }
}