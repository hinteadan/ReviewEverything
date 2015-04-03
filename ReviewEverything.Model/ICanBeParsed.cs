﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.Model
{
    public interface ICanBeParsed
    {
        ReviewItem Parse();
        Task<ReviewItem> ParseAsync();
    }
}
