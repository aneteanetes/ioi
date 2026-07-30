DamageContext = {}
DamageContext.__index = DamageContext

function DamageContext:new()
    local obj = {
        attacker="",
        target="",
        attacked=0,
        dmg = 0,
        defed=0,
        died=false,
        action="strike",
        healed=0,
        killed=nil,
        skipatk=false,
        msgs={},
        elem='physical'
    }
    setmetatable(obj, self)
    return obj
end

function DamageContext:elemColor(elem)
    if elem=='physical' then
        return '/c[#5c5c5c]';
    elseif elem=='pure' then
        return '/c[#f6ff00]';
    end

    return '/cd';
end

function DamageContext:log()
    local white = " /c[#F5DEB3]";
    local orange = " /c[#f57207]";
    local defcolor = " /c[#026300]";

    local log={}
    
    if self.skipatk==false then
        local attackheader = self.attacker.." /cd"..loco("attacking").." /cd"..self.target.."/cd!";
        world.CombatSystem.LogCombat(attackheader);
    end

    if self.msgs ~= nil then
        for _,v in pairs(self.msgs) do
            world.CombatSystem.LogCombat(v);
        end
    end

    if self.healed == 0 then

        local elemColor = self:elemColor(self.elem);

        if self.dmg > 0 then
            table.insert(log,self.target.." /cd"..loco("getting")..orange..tostring(self.dmg).." "..elemColor..loco(self.elem)..' /cd'..loco("dmgplural"));
        else
            table.insert(log,self.target.." /cd"..loco("notgetting").." "..loco("dmgplural"));
        end

        if self.elem~='pure' or self.dmg > 0 then
            table.insert(log,"/cd"..loco("deals")..white..tostring(self.attacked));
        end

        if self.defed > 0 then
            table.insert(log,"/cd"..loco("defeddmg")..defcolor..tostring(self.defed));
        end

        if self.dmg ~= self.attacked then
            table.insert(log,"/cd"..loco("glancingblow"));
        end
    else

        table.insert(log,self.target.." /cd"..loco("restores").."/c[#00c90a] "..self.healed.." /cd"..loco("hpes"));
    end

    local msg = table.concat(log,"/cd, ");
    world.CombatSystem.LogCombat(msg.."/cd!");

    if(self.died==true) then
        world.CombatSystem.LogCombat(self.target.." /cd"..loco("diedcombat").."!");
    end

    if(self.killed~=nil) then
        world.CombatSystem.LogCombat(self.killed);
    end

    world.CombatSystem.LogCombatDelimiter();
end