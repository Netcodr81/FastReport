using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FastReport.Functions;

internal class NumToWordsEnGb : NumToWordsEn
{
    private static WordInfo milliards = new WordInfo("milliard");
    private static WordInfo trillions = new WordInfo("billion");

    protected override WordInfo GetMilliards()
    {
        return milliards;
    }

    protected override WordInfo GetTrillions()
    {
        return trillions;
    }
}