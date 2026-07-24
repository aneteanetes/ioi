Templates.Abilities.Base = {
    name = "abil_war_supress",
    level=1,
    mode = "active",--"passive"
    ratestat = "ad",
    rate=2.34,
    costpercent=10,
    element = "physical",
    duration=0,
    icon="╥",
    color={0,0,255},
    location="combat",

    getPower = function (self)
	    return math.round(self.ap * self.rate);
    end,

    cast = function (self,obj,target)
	    
    end,

}