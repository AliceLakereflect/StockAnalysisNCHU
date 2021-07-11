﻿using System.Collections.Generic;

public class ChartData
{
    public string Name { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public List<string> Day { get; set; }
    public List<long> Timestamp { get; set; }
    public List<double?> Price { get; set; }
    public List<double?> PriceAvg5Days { get; set; }
    public List<double?> PriceAvg10Days { get; set; }
    public List<double?> PriceAvg20Days { get; set; }
    public List<double?> PriceAvg60Days { get; set; }

    public ChartData()
    {
    }

    public List<double?> GetMaValue(int avgDay)
    {
        switch (avgDay)
        {
            case 5:
                return PriceAvg5Days;
            case 10:
                return PriceAvg10Days;
            case 20:
                return PriceAvg20Days;
            case 60:
                return PriceAvg60Days;
            default:
                return new List<double?>();
        }
    }
}