Templates.Perks.Experienced = {
    -- ids
    id="ExperiencedPerk",
    
    desc="ExperiencedPerkDesc",
    icon="{",
    color={0, 0, 255, 255},
    mods = {
        {
            id="Experienced_ad",
            type=Templates.Base.Mod.Type.Flat,
            value=3,
            stat="ad"
        },
        {
            id="Experienced_def",
            type=Templates.Base.Mod.Type.Flat,
            value=1,
            stat="def"
        }
    }
}