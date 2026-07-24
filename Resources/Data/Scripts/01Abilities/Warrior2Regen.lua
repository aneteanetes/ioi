Templates.Abilities.Warrior2Regen = {
    name = "abil_war_regen",
    level=1,
    mode = "active",--"passive"
    ratestat = "ap",
    ratelvl=0.21,
    rate=1.93,
    cost=15,
    element = "physical",
    duration=0,
    tileset="Consolas1",
    tileid=427,
    icon="=",
    color={0, 255, 0},
    location="combat",

    canCast = function (self, objEntity, targetEntity)
        local obj = objEntity.Data;
        if obj.rage<self.cost then
            return false;
        end

        return true;
    end,

    cast = function (self,objEntity,targetEntity)
        local obj = objEntity.Data;
        local target = targetEntity.Data;

        obj.rage = obj.rage - self.cost;
        
        local ctx = DamageContext:new();
        ctx.skipatk=true;
        ctx.target = obj:getNameColored();

        table.insert(ctx.msgs,obj:getNameColored().."/cd "..loco("using").." /c["..toHexString(self.color).."]"..loco(self.name).."/cd!");
        
        local value = self:getPower(obj);
        obj:applyheal(value,obj,ctx);

        ctx:log();

        if (ctx.died == true) then
            world.CombatSystem.Kill(targetEntity);
        end
    end,

    getPower = function (self,obj)
        local val = (obj.ap == 0 and 1 or obj.ap) * self.rate;
        val = val + (self.ratelvl*obj.level);
        
        if val<1 then
            val=1;
        end

        return math.floor(val + 0.5);
    end,
}