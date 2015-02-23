using wServer.logic.behaviors;
using wServer.logic.loot;
using wServer.logic.transitions;

namespace wServer.logic
{
    partial class BehaviorDb
    {
        private _ Floor2 = () => Behav()
            .Init("Tower Bandit",
                new State(
                    new Follow(0.4, range: 2),
                    new Wander(0.3),
                    new Shoot(8, coolDown: 750)
                    ),
                new TierLoot(2, ItemType.Weapon, 0.1),
                new TierLoot(2, ItemType.Armor, 0.1),
                new TierLoot(1, ItemType.Ring, 0.08),
                new TierLoot(1, ItemType.Ability, 0.15),
                new ItemLoot("Health Potion", 0.2)
            )
            .Init("Tower Bandit Leader",
                new State(
                    new Wander(0.5),
                    new Shoot(8, coolDown: 750),
                    new Grenade(4, 50, 8, coolDown: 4000),
                    new Reproduce("Tower Bandit", densityMax: 6, coolDown: 1000, spawnRadius: 1)
                    ),
                new TierLoot(3, ItemType.Weapon, 0.3),
                new TierLoot(3, ItemType.Armor, 0.3),
                new TierLoot(1, ItemType.Ring, 0.22),
                new TierLoot(2, ItemType.Ability, 0.39),
                new ItemLoot("Health Potion", 0.1)
            )
            .Init("Super Tower Warrior",
                new State(
                    new TowerDeathPortal(),
                    new State("default",
                        new PlayerWithinTransition(8, "init")
                        ),
                    new State("init",
                        new PlayerScaleHealth(2000),
                        new PlayerScaleDefense(2),
                        new TimedTransition(0, "spintransition")
                        ),
                    new State("spintransition",
                        new Order(20, "Super Tower Warrior Decoy", "decay"),
                        new ConditionalEffect(ConditionEffectIndex.Invulnerable),
                        new StayCloseToSpawn(2, 0),
                        new Flash(0xFF00FF00, 250, 10),
                        new TimedTransition(3000, "spin")
                        ),
                    new State("spin",
                        new Follow(0.4, range: 1),
                        new Shoot(20, 4, fixedAngle: 0, coolDownOffset: 0, coolDown: 1000),
                        new Shoot(20, 4, fixedAngle: 18, coolDownOffset: 200, coolDown: 1000),
                        new Shoot(20, 4, fixedAngle: 36, coolDownOffset: 400, coolDown: 1000),
                        new Shoot(20, 4, fixedAngle: 54, coolDownOffset: 600, coolDown: 1000),
                        new Shoot(20, 4, fixedAngle: 72, coolDownOffset: 800, coolDown: 1000),
                        new TimedTransition(5000, "decoytransition")
                        ),
                    new State("decoytransition",
                        new ConditionalEffect(ConditionEffectIndex.Invulnerable),
                        new StayCloseToSpawn(2, 0),
                        new Flash(0xFF00FF00, 250, 10),
                        new TimedTransition(3000, "decoy")
                        ),
                    new State("decoy",
                        new Reproduce("Super Tower Warrior Decoy", densityMax: 5, coolDown: 500, spawnRadius: 0),
                        new Shoot(20, coolDownOffset: 1000, coolDown: 1000),
                        new Wander(0.6),
                        new TimedTransition(20000, "spintransition")
                        )
                    )
            )
            .Init("Super Tower Warrior Decoy",
                new State(
                    new State("init",
                        new PlayerScaleHealth(500),
                        new PlayerScaleDefense(2),
                        new TimedTransition(0, "wander")
                        ),
                    new State("wander",
                        new Wander(0.6),
                        new Shoot(20, coolDownOffset: 500, coolDown: 1000)
                        ),
                    new State("decay",
                        new Decay(100)
                        )
                    )
            )
            ;
    }
}