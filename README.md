# Smooth Armor Scaling

- Replaces the piecewise damage reduction function and provides several global gear configuration options.

<br/>  
  
## Damage Reduction Function

The standard armor formula is a piecewise function in which 1 armor = 1 damage reduction for up to half of the incoming damage, after which it has diminishing returns. [Refer to the wiki page for details.](https://valheim.fandom.com/wiki/Damage_mechanics#Armor)

![Vanilla Armor Scaling Formula](https://static.wikia.nocookie.net/valheim/images/3/38/Code.png/revision/latest?cb=20210309232213)


This mod replaces that piecewise function with a singular function:
  
```
float basis = 1.0f + (ac * armorEffectiveness / dmg);
return dmg / (basis * basis);
```

This function follows the behavior of the original relatively closely but has smooth scaling across all values. Armor will be more effective vs larger hits and behave more consistently vs varying hit sizes.
  
<br/>
  
## Gear Configuration

As of v0.2.0 the `armorEffectiveness` configuration setting has been removed and the following settings added:

`playerBaseArmor` Add flat armor to players.  
`gearArmorMultiplier` Multiply base armor of Head, Chest, and Leg pieces.  
`gearFlatArmor` Add flat armor to Head, Chest, and Leg pieces.  
`gearArmorPerLevel` Armor added per quality level for Head, Chest, and Leg pieces.  
`capeFlatArmor` Add flat armor to Capes.  
`capeArmorPerLevel` Armor added per quality level for Capes.  

As opposed to the hidden effect of `armorEffectiveness` these settings are properly reflected on the tooltip values for the gear and player.

<br/> 

---  
[ServerSync enabled](https://github.com/blaxxun-boop/ServerSync)  
[Valheim Modding Server](https://discord.com/invite/89bBsvK5KC)  
[Github](https://github.com/kruftt/SmoothArmorScaling)  

`Discord` Kruft#6332  