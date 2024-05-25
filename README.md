### Smooth Armor Scaling
---
Replaces the armor function and provides an armor multiplier configuration setting.

The standard armor formula is a piecewise function in which 1 armor = 1 damage reduction for up to half of the incoming damage, after which it has diminishing returns in proportion to its reciprocal. [Refer to the wiki page for damage mechanics.](https://valheim.fandom.com/wiki/Damage_mechanics#Armor)

![Vanilla Armor Scaling Formula](https://static.wikia.nocookie.net/valheim/images/3/38/Code.png/revision/latest?cb=20210309232213)


This mod replaces that piecewise function with a singular function:
  
```
float basis = 1.0f + (ac * armorEffectiveness / dmg);
return dmg / (basis * basis);
```

This function follows the behavior of the original relatively closely but has smooth scaling across all values. Armor will be more effective vs larger hits and behave more consistently vs varying hit sizes.

<br />  

[ServerSync enabled.](https://github.com/blaxxun-boop/ServerSync)
  
<br />
  
#### Additional Info
---
    
[Github Repo](https://github.com/kruftt/SmoothArmorScaling)  
[Valheim Modding Server](https://discord.com/invite/89bBsvK5KC)  
`Discord ID:` Kruft#6332  
