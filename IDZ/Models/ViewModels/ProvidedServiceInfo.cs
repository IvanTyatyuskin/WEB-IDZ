using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDZ.Models.Entities;

namespace IDZ.Models.ViewModels
{
    public class ProvidedServiceInfo
    {
        public string ServiceName { get; set; }
        public DateTime ProvidedServiceDateTime { get; set; }
        public string ProvidedServiceStatus { get; set; }
        public string MasterName { get; set; }
        public string MasterSurname { get; set; }
        public int ServicePrice { get; set; }



    }
}