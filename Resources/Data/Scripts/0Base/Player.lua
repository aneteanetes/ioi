Templates.Base.Player = {
	--[[collide = function(self,objmap,collision)
		if collision.Entity["type"]=='enemy' then
			collision:StopMove();
			objmap:StopMove();
			world.CombatSystem:StartCombat(collision.Entity);
		end
	end--]]
}