﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorFabric
{
    public enum Alignment
    {
        Unset,
        Auto,
        Stretch,
        Baseline,
        SpaceBetween,
        SpaceAround,
        SpaceEvenly,
        Start,
        Center,
        End
    }

    public static class CssUtils
    {
        public static Dictionary<Alignment, string> AlignMap = new Dictionary<Alignment, string>
        {
            [Alignment.Auto] = "auto",
            [Alignment.Stretch] = "stretch",
            [Alignment.Baseline] = "baseline",
            [Alignment.Start] = "flex-start",
            [Alignment.End] = "flex-end",
            [Alignment.Center] = "center",
            [Alignment.SpaceBetween] = "space-between",
            [Alignment.SpaceAround] = "space-around",
            [Alignment.SpaceEvenly] = "space-evenly",
        };
    }
}
