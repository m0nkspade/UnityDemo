using System;
using System.Collections.Generic;
using UnityEngine;

public static class BoxMath
{
    public static float Overlap(Rect a, Rect b)
    {
        float x1 = Math.Max(a.xMin, b.xMin);
        float y1 = Math.Max(a.yMin, b.yMin);
        float x2 = Math.Min(a.xMax, b.xMax);
        float y2 = Math.Min(a.yMax, b.yMax);

        float intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
        float union = a.width * a.height + b.width * b.height - intersection;
        return union > 0 ? intersection / union : 0;
    }

    public static List<DetectionType> Suppress(List<DetectionType> candidates, float threshold = 0.5f)
    {
        var sorted = new List<DetectionType>(candidates);
        sorted.Sort((a, b) => b.score.CompareTo(a.score));
        
        var kept = new List<DetectionType>();
        
        foreach (var candidate in sorted)
        {
            bool overlapKept = false;
            foreach (var k in kept)
            {
                if (Overlap(candidate.rect, k.rect) > threshold)
                {
                    overlapKept = true;
                    break;
                }
            }
            if (!overlapKept)
                kept.Add(candidate);
        }
        return kept;
    }
}

