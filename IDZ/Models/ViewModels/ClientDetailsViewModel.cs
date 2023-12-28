using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDZ.Models.Entities;

namespace IDZ.Models.ViewModels
{
    public class ClientDetailsViewModel
    {
        public Client Client { get; set; }
        public List<ProvidedServiceInfo> ProvidedServices { get; set; }

    }
}