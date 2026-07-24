Templates.Abilities.Warrior1Supress = {
    name = "abil_war_supress",
    level=1,
    mode = "active",--"passive"
    ratestat = "ad",
    ratelvl=0.39,
    rate=2.34,
    cost=10,
    element = "physical",
    duration=0,
    tileset="Consolas1",
    tileid=111,
    color={84, 93, 222},
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

        local elem = self.element;
        local dmg=0;

        local isEffective = loco('effective');

        if (target.hp==target.mhp) then
           elem="pure";
           dmg = self:getPower(obj);
        else
            dmg = math.round(self:getPower(obj)/4);
            if dmg < 0 then
                dmg=0;
            end
            isEffective = loco('not')..' '..isEffective;
        end

        local ctx = DamageContext:new();
        ctx.attacked = math.floor(dmg);
        ctx.skipatk=true;
        ctx.elem=elem;

        dmg = target:applydmg(dmg,obj,ctx,elem);
        table.insert(ctx.msgs,obj:getNameColored().."/cd "..loco("using").." /c["..toHexString(self.color).."]"..loco(self.name).."/cd - "..isEffective..'!');

        ctx:log();

        if (ctx.died == true) then
            world.CombatSystem.Kill(targetEntity);
        end
    end,

    getPower = function (self,obj)
        local val = obj.ad * self.rate;
        val = val + (self.ratelvl*obj.level);
        return math.floor(val + 0.5);
    end,
}