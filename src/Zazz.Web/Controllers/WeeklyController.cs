﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class WeeklyController : Controller
    {
        public void New(WeeklyViewModel vm)
        {
            
        }

        public void Edit(WeeklyViewModel vm)
        {

        }
    }
}