﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Craft.Net.Data.Blocks
{
    public class CoalOreBlock : Block
    {
        public override ushort Id
        {
            get { return 16; }
        }

        public override double Hardness
        {
            get { return 3; }
        }
    }
}
