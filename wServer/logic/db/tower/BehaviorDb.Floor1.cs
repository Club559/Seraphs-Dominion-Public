using wServer.logic.behaviors;
using wServer.logic.loot;
using wServer.logic.transitions;

namespace wServer.logic
{
    partial class BehaviorDb
    {
        private _ Floor1 = () => Behav()
            .Init("Lesser Tower Warrior",
                new State(
                    new Follow(0.4, range: 2),
                    new Wander(0.3),
                    new Shoot(8, coolDown: 750)
                    ),
                new TierLoot(1, ItemType.Weapon, 0.1),
                new TierLoot(1, ItemType.Armor, 0.1),
                new ItemLoot("Health Potion", 0.03),
                new ItemLoot("Banana", 0.005)
            )
            .Init("Greater Tower Warrior",
                new State(
                    new Reproduce("Lesser Tower Warrior", densityMax: 6, coolDown: 1000, spawnRadius: 2),
                    new Wander(0.3),
                    new Shoot(12, coolDown: 750)
                    ),
                new TierLoot(2, ItemType.Weapon, 0.3),
                new TierLoot(2, ItemType.Armor, 0.3),
                new TierLoot(1, ItemType.Ring, 0.22),
                new TierLoot(1, ItemType.Ability, 0.39),
                new ItemLoot("Health Potion", 0.03),
                new ItemLoot("Magic Potion", 0.03)
            )
            .Init("Tower Golem Boss",
                new State(
                    new TowerDeathPortal(),
                    new State("default",
                        new PlayerWithinTransition(8, "init")
                        ),
                    new State("init",
                        new PlayerScaleHealth(1000),
                        new PlayerScaleDefense(1),
                        new Taunt(true, "I will defend this tower!"),
                        new TimedTransition(500, "attack")
                        ),
                    new State("attack",
                        new Reproduce("Greater Tower Warrior", densityMax: 1, coolDown: 3000, spawnRadius: 1),
                        new Shoot(10, count: 6, shootAngle: 15, predictive: 2, coolDown: 3000),
                        new Follow(0.5, range: 2)
                        )
                    )
            )
            ;
    }
}