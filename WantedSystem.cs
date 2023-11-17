using RAGE;
using RAGE.Elements;
using RAGE.Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

class WantedSystem
{
    private static Dictionary<string, Blip> _blips = new Dictionary<string, Blip>();

    public static void OnWantedLevelUpdate(object[] args)
    {
        int level = (int)args[0];

        Misc.SetFakeWantedLevel(level);
    }

    public static void OnBlipSetupForCop(object[] args)
    {
        var name = (string)args[0];
        float x = (float)args[1];
        float y = (float)args[2];
        float z = (float)args[3];

        _blips[name] = new Blip(801, new Vector3(x, y, z), "Target", 2.0f, 1, 255, 0, false, 0, 0, 0);
        _blips[name].SetScale(2.0f);
    }

    public static void OnBlipRemoveForCop(object[] args)
    {
        var name = (string)args[0];

        _blips[name]?.Destroy();
    }
}