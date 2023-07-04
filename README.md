# Medieval Madness
 
This is the source code for [Medieval Madness](https://wardergrip.itch.io/medieval-madness) which was a group project for the subject game projects in [Digital Arts and Entertainment](https://digitalartsandentertainment.be/).

Made by:
- Jens Brunson
- Renzo Depoortere
- Reï Messely
- Gaétan Schepens
- Alex Sterneckert

## My contribution

I contributed in these big areas:
- Combat (except for ragdoll)
    - Health system (Similar to Super Smash Brothers)
    - Weapons
    - Weapon spawning
    - Picking up weapons
- Alternative rounds
    - Weighted random
    - Easy to make new ones
- HUD
    - Playerpanel
    - Death timer
    - Helmet shake
    - Fire
    - Scoreboard

## Combat

Combat is an essential part of the game and being able to divide the tasks was quite important. We discussed what differences each weapon would have so that the other programmer could start on everything regarding the ragdoll side of things and I could make the actual weapons.

### Health
For the health system, it was interesting to work on a reversed health system. Instead of having a max number and reducing it to 0, you start at 0 and it increases more and more until you hit a kill condition. For those that are unfamiliar with what I am talking about, we use the Super Smash Brothers health system. Instead of dying when health hits 0, you die when you leave the play area and in our case hit the spikes too fast. The higher your health, the harder you get knocked back and the easier it is to hit the velocity treshold of the spikes.

### Weapons
All weapons have a different weight, hitbox, durability, damage and knockback. Although the health value is a big contributor to the amount of knockback that is applied, the knockback value makes it so that some weapons are far better than others to finish off other players. But, other weapons that are light and have small knockback are better at dealing damage.

Weapon spawning like the game rounds is weighted. 
```cs
public GameObject NextSpawnableObject()
{
    float totalWeight = spawnableObjects.Sum(obj => obj.Weight);

    float randomWeight = UnityEngine.Random.Range(0f, totalWeight);

    foreach (SpawnableObject obj in spawnableObjects)
    {
        randomWeight -= obj.Weight;
        if (randomWeight <= 0)
        {
            return obj.Prefab;
        }
    }

    throw new UnityException("Something went terribly wrong.");
}
```
This weighting makes it easy to make some weapons a little more uncommon than others which makes it more fun to get that one rare weapon and makes it easier to balance. If one weapon is objectively better but it is in abundance, everyone will use it and it will feel unfun.

## Alternative rounds

Each game round has a list of settings. These are the settings that will be loaded when the game round is chosen. These settings are nothing more than some multipliers and values. The base round has a virtual initialisation function that can be overriden to add extra functionality.
For example: for the more dragons round, the overriden function will spawn in more dragons.
This simple approach makes it easy to make many rounds and even give players the oppertunity to make their own (if we had the time to make a custom round screen) without sacrificing too much flexibility and diversity.

## HUD

The HUD is rather basic. The main things that I enjoyed about making of the HUD is the polish. Things like making the helmet shake on damage and making sure the background changes with the player color.

## Conclusion

Here I want to write the things I learned. A main cliche that I often thought wouldn't be a mistake I would make is making a whole lot of things global. There are so much singletons that there is a singleton to group the singletons. In some cases, the only thing the singleton does is handle events and delegate functions. In some cases, I think a static part of a class would be a really clean way to reduce the singletons and keep a thing to its class. 
Another thing I want to think more about is the overall game loop. Finishing the game loop at the end of production is stressful and very error prone to say the least. I think that in my next project I will try to make the game loop first and see how that works out.
Last, but definitely not least, I learned the beauty of observers and wish to utilise them more without gettint too obnoxious.