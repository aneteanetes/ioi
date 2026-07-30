Templates.Classes.Warrior = {

    class='Warrior',

    rage=0,

    basemhp=100,
    mhp=100,
    hp=100,
    basemindmg=2,
    basemaxdmg=4,
    ragegain=5,

    abilities = {
        "Templates.Abilities.Warrior1Supress",
        "Templates.Abilities.Warrior2Regen",
        "Templates.Abilities.Warrior3Finisher",
        "Templates.Abilities.Warrior4Ferocity",
    },

    res='rage',
    rescolor={255,0,0,255},
    resstring = function (obj)
        return tostring(obj.rage);
    end,

    init = function(obj,props)
        local components = obj["_components"];
    
        obj.rage=0;

        if(obj.perks==nil) then
            obj.perks={};
        end

        table.insert(obj.perks,Templates.Perks.Experienced);
        table.insert(obj.perks,Templates.Perks.Human);


    end,

    levelup = function (obj)
        obj:add("basemindmg",1);
        obj:add("basemaxdmg",3);
        obj:add("basemhp",3);
        obj:add("sp",1);
        obj:add("pp",1);

        obj:applyheal(obj.basemhp);

        world.LogSystem.Log(obj:coloredName().." /cd"..loco("leveledup")..": +1-3 DMG, +3HP, +1SP, +1PP!");
    end,

    afterdmg = function (self,dmg,attacker,ctx)
        if self["rage"] < 100 then
            local ragegained = 5 + self:getAbility(4)["ratelvl"] * self.level-1;
            self["rage"] = math.clamp(self["rage"]+ragegained,0,100);
            table.insert(ctx.msgs,self:getNameColored().."/cd "..loco("getting").." /c[#ff0000]"..ragegained.." "..loco("rages")..'/cd!');
        end
        return dmg;
    end,
}