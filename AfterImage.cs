using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using Modding;


namespace AfterImage
{
    public class AfterImage : Mod, ITogglableMod
    {
        public override string GetVersion() => "1.0";
        public override void Initialize() {
            Log("AfterImage enabled.");
        }

        public void Unload() {
            Log("AfterImage disabled.");
        }
    }
}
