using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FastReport.Data;

internal static class CubeHelper
{
    public static CubeSourceBase GetCubeSource(Dictionary dictionary, string complexName)
    {
        if (String.IsNullOrEmpty(complexName))
            return null;
        string[] names = complexName.Split('.');
        return dictionary.FindByAlias(names[0]) as CubeSourceBase;
    }
}