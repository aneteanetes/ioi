Templates.items.BaseItem = {
	itemkind = 'weapon',
	itemstats = {
		ad=5,
		ap=5
	},

	tileset="",
	tileid=0,

	type="item",

	color = {0, 112, 221},

	itemlevel=0,
	--[[
	Серый (Хлам): #9d9d9d
Белый (Обычное): #ffffff
Зеленый (Необычное): #1eff00
Синий (Редкое): #0070dd
Фиолетовый (Эпическое): #a335ee
Оранжевый (Легендарное): #ff8000 
Дополнительные категории
Светло-золотой (Артефакт): #e6cc80
	--]]

	fulfill = function (newitem,obj)
		local withstats = false;
		local rng = world.ItemRandomSystem.GetGenerator(newitem.seed);

		for k,v in pairs(newitem.itemstats) do
			local val = rng:Next(0,v+1);
			if val > 0 then
				withstats = true;
				newitem[k]=val;
			end
		end

		return withstats;
	end
}