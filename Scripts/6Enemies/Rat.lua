Templates.enemy.rat = {
    -- ids
    id="moveable",
            
    name="rat_enemy",
    type="enemy",
	speed = 0.15,
    idleSpeed=10,
    idleAmplitude=.5,
    stepSleepMS=0,
    pathSleepMS=0,

    movearea = {
        x=-9,
        y=-9,
        w=19,
        h=9
    },

    exp=2,

    ad=5,
    def=3,
    mdef=2,

    -- hp
    basemhp=10,
    mhp=10,
    hp=10,

    -- damage
    basemindmg=1,
    basemaxdmg=2,

    loottablename=nil,
    loottable=nil,

    icon='r',
    color={173,113,56},
    
    init = function (obj,props)
        local components = obj["_components"];

        table.insert(components,2,"Templates.Base.Enemy");
        table.insert(components,1,"Templates.Races.animal");

        if props.class ~= nil then
            table.insert(components,1,"Templates.Classes."..props.class);
        else
            table.insert(components,1,"Templates.Classes.bruiser");
        end

        table.insert(components,1,"Templates.Base.Moveable");

        if obj.loottablename ~= nil then
            obj.loottable = world.SpawnLootTable(obj.loottablename);
        end

    end

}