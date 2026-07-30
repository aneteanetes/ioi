Templates.loot.table = {
--[[entry1 = {
		type = Templates.loot.chance.guarant,
		generator = 'Templates.items.Sword'
	},
	entry2 = {
		type = Templates.loot.chance.percent,
		generator = 'Templates.items.Helm',
		percent=50
	},
	entry3 = {
		type = Templates.loot.chance.condition,
		generator = 'Templates.items.Offhand',
		condition = function (obj)
			return true;
		end
	},--]]

	generate = function (self,name,obj)
	
		local itemEntities = {};

		for key,entry in pairs(Templates.loot.table[name]) do
			if (entry.type ~= nil) then
				if (entry.type == Templates.loot.chance.guarant) then
					self:generateGuarant(itemEntities,entry,obj);
				elseif (entry.type == Templates.loot.chance.percent) then
					self:generatePercent(itemEntities,entry,obj);
				elseif (entry.type == Templates.loot.chance.condition) then
					self:generateCondition(itemEntities,entry,obj);
				end
			end
		end

		return itemEntities; -- talbe of GameEntity

	end,

	generateGuarant = function (self,items,entry,obj)
		if (entry.generator ~= nil) then
			local item = self.generateItem(entry.generator,obj);
			if (item~=nil) then
				table.insert(items,item);
			end
		end
	end,

	generatePercent = function (self,items,entry,obj)
		if (entry.generator ~= nil and entry.percent ~= nil) then
			if math.random(1, 101) <= entry.percent then
				local item = self.generateItem(entry.generator,obj);
				if (item~=nil) then
					table.insert(items,item);
				end
			end
		end
	end,

	generateCondition = function (self,items,entry,obj)
		if (entry.generator ~= nil and entry.condition ~= nil) then
			if entry.condition(obj) then
				local item = self.generateItem(entry.generator,obj);
				if (item~=nil) then
					table.insert(items,item);
				end
			end
		end
	end,

	generateItem = function (generatorId,obj)
		local itemEntity = world.SpawnSystem.SpawnEntity('Templates.Base.Object','Templates.items.BaseItem',generatorId)
		local entity = itemEntity.Data;
		local isFilled = entity.fulfill(entity,obj);

		if isFilled==false then
			return nil;
		else
			return itemEntity;
		end
	end,
}