using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDZ.Models.Entities;
using IDZ.Models.ViewModels;
namespace IDZ.Models.ViewModels
{
    public class MasterDetailsVM
    {
        public Master Master { get; set; }

        public List<ServiceVM> Services { get; set; }


    }
}