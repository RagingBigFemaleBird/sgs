using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SgsCore
{
    public enum Allegiance
    {
        Unknown,
        Shu,
        Wei,
        Wu,
        Qun,
        God
    }

    public class Hero
    {
        protected Allegiance mAllegiance;
    }
}
