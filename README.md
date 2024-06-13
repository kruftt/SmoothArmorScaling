# Smooth Armor Scaling

- Replaces the piecewise damage reduction function and provides a damage taken and global gear configuration options.

<br/>  
  
## Damage Reduction Function

The standard armor formula is a piecewise function in which 1 armor = 1 damage reduction for up to half of the incoming damage, after which it has diminishing returns. [Refer to the wiki page for details.](https://valheim.fandom.com/wiki/Damage_mechanics#Armor)

![Vanilla Armor Scaling Formula](https://static.wikia.nocookie.net/valheim/images/3/38/Code.png/revision/latest?cb=20210309232213)


This mod replaces that piecewise function with a singular function:
  
```cs
float basis = 1.0f + (ac * armorEffectiveness / dmg);
return (dmg * damageTaken) / (basis * basis);
```

This function follows the behavior of the original relatively closely but has smooth scaling across all values. Armor will be more effective vs larger hits and behave more consistently vs varying hit sizes.
  
<br/>
  
## Gear Configuration

Settings can be used to adjust the armor amounts on players and gear as well as overall damage taken:

- Enable/disable the smooth armor function.  
- Coefficient for the effect of armor in damage reduction.  
- Multiply the final amount of damage applied to player.  
  
- Add flat armor to players.  
  
- Multiply base armor of Head, Chest, and Leg pieces.  
- Add flat armor to Head, Chest, and Leg pieces.  
- Armor added per quality level for Head, Chest, and Leg pieces.  
  
- Add flat armor to Capes.  
- Armor added per quality level for Capes.  
  
The results of applying these settings are reflected on their respective tooltips.  

<br/> 

---  
[ServerSync enabled](https://github.com/blaxxun-boop/ServerSync)  
[Valheim Modding Server](https://discord.com/invite/89bBsvK5KC)  
[Thunderstore](https://thunderstore.io/c/valheim/p/kruft/SmoothArmorScaling/)
[Github](https://github.com/kruftt/SmoothArmorScaling)  

`Discord` Kruft#6332  