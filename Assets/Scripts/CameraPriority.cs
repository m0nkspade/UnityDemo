using System;
using System.Collections.Generic;

public enum CameraClass
{
    Dedicated = 0, // Nikon
    External = 1, // any USB cam
    Phone = 2, // iPhone
    BuiltIn = 3, // macbook cam
}

public static class CameraPriority
{
    static readonly string[] DedicatedKeywords = {"nikon", "webcam utility", "z 9", "z 8", "z9", "z8", "capture", "elgato", "cam link" };
    static readonly string[] PhoneKeywords = { "iphone", "ipad", "continuity" };
    static readonly string[] BuiltInKeywords = { "macbook", "facetime", "built-in", "integrated" };
    static readonly string[] VirtualKeywords = { "obs", "virtual", "camo", "zoom" };

    public static CameraClass Classify(string deviceName)
    {
        // lower-case and trim the name, then test the keyword lists in this order:
          // dedicated, virtual, phone, built-in. Anything left is External.
        deviceName = deviceName.ToLower().Trim();
        foreach (var keyword in DedicatedKeywords)
        {
            if (deviceName.Contains(keyword))
                return CameraClass.Dedicated;
        }
        foreach (var keyword in VirtualKeywords)
        {
            if (deviceName.Contains(keyword))
                return CameraClass.External; // Assuming virtual cameras are treated as external
        }
        foreach (var keyword in PhoneKeywords)
        {
            if (deviceName.Contains(keyword))
                return CameraClass.Phone;
        }
        foreach (var keyword in BuiltInKeywords)
        {
            if (deviceName.Contains(keyword))
                return CameraClass.BuiltIn;
        }
        return CameraClass.External;    
    }
    
    public static int SelectBest(IReadOnlyList<String> deviceNames)
    {
        int bestIndex = -1;
        CameraClass bestClass = CameraClass.External;
        for (int i = 0; i < deviceNames.Count; i++)
        {
            var currentClass = Classify(deviceNames[i]);
            if (bestIndex == -1 || currentClass < bestClass)
            {
                bestIndex = i;
                bestClass = currentClass;
            }
        }
        return bestIndex;
    }

    public static bool MatchesAny(string lowerName, string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            if (lowerName.Contains(keyword))
                return true;
        }
        return false;
    }
}
