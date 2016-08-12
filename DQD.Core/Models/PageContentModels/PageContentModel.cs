﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQD.Core.Models.PageContentModels {
    public class PageContentModel {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Date { get; set; }
        public List<ContentStrings> ContentString { get; set; }
        public List<ContentImages> ContentImage { get; set; }
    }
}