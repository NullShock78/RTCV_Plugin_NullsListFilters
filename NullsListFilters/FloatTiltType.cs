using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullsListFilters
{
    [Serializable]
    public enum FloatTiltType
    {
        ADD = 0,
        SUB = 1,
        MUL = 2,
        DIV = 3,
        SET = 4,
        ABS,
        NEG,
        SQRT,
        RND,
        SIN, SIN2,
        COS, COS2,
        LOG, LOG2,
        TAN, TAN2,
        POW,
        LIM
    }
}
