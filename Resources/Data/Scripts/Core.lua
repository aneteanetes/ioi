math.randomseed(os.time());

math.round = function(num)
    return math.floor(num + 0.5)
end

Core = {
    groupby = function(data, key_selector)
        local grouped = {}
        for k, item in ipairs(data) do
            local key = key_selector(item)
            grouped[key] = grouped[key] or {}
            table.insert(grouped[key], item)
        end
        return grouped
    end,
    combatDelayMS=800
}
Templates={
    Base={},
    Perks={},
    Races={},
    Classes={},
    Abilities={},
    enemy={},
    spawner={},
    loot={},
    items={},
}