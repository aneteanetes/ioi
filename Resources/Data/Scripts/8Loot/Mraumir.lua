Templates.loot.table.Mraumir = {
	entry1 = {
		type = Templates.loot.chance.guarant,
		generator = 'Templates.items.Sword'
	},
	entry2 = {
		type = Templates.loot.chance.percent,
		generator = 'Templates.items.Sword',
		percent=50
	},
	entry3 = {
		type = Templates.loot.chance.condition,
		generator = 'Templates.items.Sword',
		condition = function (obj)
			if obj["class"]=='Warrior' then
				return true;
			else
				return false;
			end
		end
	},
}