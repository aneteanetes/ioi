Templates.Base.Enemy = {

	assemblySquadRadius=2,

	collide = function(self,selfentity,objmap,collision)
		if collision.Entity["type"]=='player' then
			collision:StopMove();
			objmap:StopMove();
			world.LogSystem.Log("/c["..toHexString(self.color).."]"..selfentity["getName"](selfentity.Data).." /cd"..loco("attackyou").." !");
			world.CombatSystem:StartCombat(selfentity);
		end
	end,

    combatturn = function(self, target)
		self.strike(self,target);
        self.tick(self);
    end,
}