using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDZ.Models.Entities;
namespace IDZ.Models.ViewModels
{
    public class ServiceVM
    {
        public string ServiceName { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }
}