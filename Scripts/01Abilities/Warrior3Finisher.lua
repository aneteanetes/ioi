Templates.Abilities.Warrior3Finisher = {
    name = "abil_war_finisher",
    level=1,
    mode = "active",--"passive"
    ratestat = "ad",
    ratelvl=0.8,
    rate=3.36, --1.36
    cost=5,
    element = "physical",
    duration=0,
    tileset="Consolas1",
    tileid=4,
    color={184, 42, 49},
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

        if (target.hp<=target.mhp*0.25) then
           dmg = self:getPower(obj);
        else
            dmg = obj:defaultDamage();
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
        local val = (obj.ad == 0 and 1 or obj.ad) * self.rate;
        val = val + (self.ratelvl*obj.level);
        
        if val<1 then
            val=1;
        end

        return math.floor(val + 0.5);
    end,
}