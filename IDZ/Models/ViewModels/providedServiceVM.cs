using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IDZ.Models.Entities;
namespace IDZ.Models.ViewModels
{
    public class providedServiceVM
    {
   
        


        public Guid providedServiceID { get; set; }
        [Required]
        [DisplayName("Дата оказания услуги")]
        public DateTime providedServiceDateTime { get; set; }
        [Required]
        [DisplayName("Статус")]
        public string providedServiceStatus { get; set; }
        [Required]
        [DisplayName("Клиент")]
        public Guid clientID { get; set; }
        [Required]
        [DisplayName("Мастер")]
        public Guid masterID { get; set; }
        [Required]
        [DisplayName("Название услуги")]
        public string serviceName { get; set; }

        public IEnumerable<SelectListItem> ClientsList { get; set; }
        public IEnumerable<SelectListItem> MastersList { get; set; }
        public IEnumerable<SelectListItem> ServicesList { get; set; }



    }
}