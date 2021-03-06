﻿using Rocket.API;

namespace UD_DeathBag
{
    public class UD_DeathBagConfiguration : IRocketPluginConfiguration
    {
        public bool Enabled;
        public ushort DeathBagId;
        public uint Delay;
        public bool StoreClothes;

        public void LoadDefaults()
        {
            Enabled = true;
            DeathBagId = 26007;
            Delay = 300;
            StoreClothes = true;
        }
    }
}
