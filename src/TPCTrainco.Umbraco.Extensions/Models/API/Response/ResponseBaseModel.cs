﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace TPCTrainco.Umbraco.Extensions.Models.API.Response
{
    public class ResponseBaseModel
    {
        public string Status { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
