using wServer.logic.behaviors;
using wServer.logic.loot;
using wServer.logic.transitions;

namespace wServer.logic
{
    partial class BehaviorDb
    {
        private _ Test = () => Behav()
            .Init("Test Miniboss 1",
                new State(
                    new State("default",
                        new PlayerWithinTransition(10, "init")
                        ),
                    new State("init",
                        new PlayerScaleHealth(10000),
                        new PlayerScaleDefense(2),
                        new Taunt("I have {HP} health"),
                        new Taunt("I have {DEF} defense"),
                        new TimedTransition(0, "following")
                        ),
                    new State(
                        new State("following",
                            new ConditionalEffect(ConditionEffectIndex.Invulnerable),
                            new Follow(0.85, range: 1),
                            new TimedTransition(2500, "followattack")
                            ),
                        new State("followattack",
                            new ConditionalEffect(ConditionEffectIndex.Armored),
                            new Shoot(10, 7, coolDown: 250),
                            new TimedTransition(500, "following")
                            ),
                        new TimedTransition(10000, "chasing")
                        ),
                    new State("chasing",
                        new ConditionalEffect(ConditionEffectIndex.Armored),
                        new Follow(2, range: 1),
                        new Shoot(10, 7, coolDown: 250),
                        new TimedTransition(4000, "idle")
                        ),
                    new State("idle",
                        new TimedTransition(3000, "following"),
                        new Flash(0x00FF00, 0.2, 15)
                        )
                    ),
                new TierLoot(1, ItemType.Weapon, 0.2),
                new ItemLoot("Health Potion", 0.03)
            )
            .Init("Demon of Darkness",
                new State(
                    new State("default",
                        new PlayerWithinTransition(8, "mainInit")
                        ),
                    new State("mainInit",
                        new Order(50, "Demon of Despair", "init"),
                        new Taunt(true, "Welcome, mortal. Choose your opponent wisely."),
                        new SwitchMusic("boss/Dark and Despair"),
                        new TimedTransition(0, "init")
                        ),
                    new State("init",
                        new PlayerScaleHealth(15000),
                        new PlayerScaleDefense(5),
                        new Shoot(10, projectileIndex: 0, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                        new Prioritize(
                            new StayCloseToSpawn(0.8, 5),
                            new Follow(1, range: 7),
                            new Wander(0.4)
                            ),
                        new HpLessTransition(0.8, "spin")
                        ),
                    new State("wait",
                        new ConditionalEffect(ConditionEffectIndex.Invulnerable),
                        new ConditionalEffect(ConditionEffectIndex.Invincible),
                        new EntityNotExistsTransition("Demon of Despair", 50, "spin2")
                        ),
                    new State(
                        new State("spin",
                            new Taunt(true, "So you have chosen me. Death awaits you!"),
                            new Order(50, "Demon of Despair", "wait"),
                            new Shoot(20, count: 6, projectileIndex: 1, shootAngle: 60, angleOffset: 0, coolDown: 900),
                            new Shoot(20, count: 6, projectileIndex: 1, shootAngle: 60, angleOffset: 10, coolDown: 900, coolDownOffset: 150),
                            new Shoot(20, count: 6, projectileIndex: 1, shootAngle: 60, angleOffset: 20, coolDown: 900, coolDownOffset: 300),
                            new Shoot(20, count: 6, projectileIndex: 1, shootAngle: 60, angleOffset: 30, coolDown: 900, coolDownOffset: 450),
                            new Shoot(20, count: 6, projectileIndex: 1, shootAngle: 60, angleOffset: 40, coolDown: 900, coolDownOffset: 600),
                            new Shoot(20, count: 6, projectileIndex: 1, shootAngle: 60, angleOffset: 50, coolDown: 900, coolDownOffset: 750),
                            new Shoot(20, projectileIndex: 0, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                            new TimedTransition(30000, "spawn"),
                            new HpLessTransition(0.1, "chase")
                            ),
                        new State("spawn",
                            new Reproduce("Minion of Darkness", 20, 6, 1000, 1),
                            new Shoot(20, projectileIndex: 0, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                            new Prioritize(
                                new Follow(0.7, range: 2),
                                new Wander(0.4)
                                ),
                            new HpLessTransition(0.1, "chase")
                            ),
                        new State("chase",
                            new Shoot(10, projectileIndex: 2, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                            new Prioritize(
                                new Follow(1.2, range: 2),
                                new Wander(0.4)
                                )
                            )
                        ),
                    new State(
                        new State("spin2",
                            new Taunt(true, "I will finish you off!"),
                            new Shoot(20, count: 6, projectileIndex: 4, shootAngle: 60, angleOffset: 0, coolDown: 900),
                            new Shoot(20, count: 6, projectileIndex: 4, shootAngle: 60, angleOffset: 10, coolDown: 900, coolDownOffset: 150),
                            new Shoot(20, count: 6, projectileIndex: 4, shootAngle: 60, angleOffset: 20, coolDown: 900, coolDownOffset: 300),
                            new Shoot(20, count: 6, projectileIndex: 4, shootAngle: 60, angleOffset: 30, coolDown: 900, coolDownOffset: 450),
                            new Shoot(20, count: 6, projectileIndex: 4, shootAngle: 60, angleOffset: 40, coolDown: 900, coolDownOffset: 600),
                            new Shoot(20, count: 6, projectileIndex: 4, shootAngle: 60, angleOffset: 50, coolDown: 900, coolDownOffset: 750),
                            new Shoot(20, projectileIndex: 0, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                            new TimedTransition(20000, "spawn2"),
                            new HpLessTransition(0.1, "chase2")
                            ),
                        new State("spawn2",
                            new Reproduce("Minion of Darkness", 20, 3, 1000, 1),
                            new Reproduce("Minion of Despair", 20, 3, 1500, 1),
                            new Shoot(20, projectileIndex: 3, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                            new Shoot(20, projectileIndex: 6, count: 5, predictive: 1, coolDown: 1200, coolDownOffset: 600),
                            new Prioritize(
                                new Follow(0.7, range: 2),
                                new Wander(0.4)
                                ),
                            new HpLessTransition(0.1, "chase2")
                            ),
                        new State("chase2",
                            new Shoot(10, projectileIndex: 5, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                            new Shoot(20, projectileIndex: 6, count: 5, predictive: 1, coolDown: 1200, coolDownOffset: 600),
                            new Prioritize(
                                new Follow(1.2, range: 2),
                                new Wander(0.4)
                                )
                            )
                        )
                    ),
                new Threshold(0.05,
                    new TierLoot(9, ItemType.Weapon, 0.2),
                    new TierLoot(10, ItemType.Weapon, 0.1),
                    new TierLoot(11, ItemType.Weapon, 0.05),
                    new ItemLoot("Potion of Defense", 0.50),
                    new ItemLoot("Potion of Vitality", 0.50),
                    new ItemLoot("Demon Essence", 0.01)
                    )
            )
            .Init("Minion of Darkness",
                new State(
                    new PlayerScaleHealth(800),
                    new PlayerScaleDefense(2),
                    new Shoot(10, predictive: 0.5, coolDown: 500),
                    new Prioritize(
                        new Follow(1.4, range: 1),
                        new Wander(1)
                        )
                    )
            )
            .Init("Demon of Despair",
                new State(
                    new State("default",
                        new PlayerWithinTransition(8, "mainInit")
                        ),
                    new State("mainInit",
                        new Order(50, "Demon of Darkness", "init"),
                        new Taunt(true, "Welcome, mortal. Choose your opponent wisely."),
                        new SwitchMusic("boss/Dark and Despair"),
                        new TimedTransition(0, "init")
                        ),
                    new State("init",
                        new PlayerScaleHealth(15000),
                        new PlayerScaleDefense(5),
                        new Shoot(10, projectileIndex: 0, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                        new Prioritize(
                            new StayCloseToSpawn(0.8, 5),
                            new Follow(1, range: 7),
                            new Wander(0.4)
                            ),
                        new HpLessTransition(0.8, "spin")
                        ),
                    new State("wait",
                        new ConditionalEffect(ConditionEffectIndex.Invulnerable),
                        new ConditionalEffect(ConditionEffectIndex.Invincible),
                        new EntityNotExistsTransition("Demon of Darkness", 50, "spin2")
                        ),
                    new State(
                        new State("spin",
                            new Taunt(true, "An unwise choice you have made."),
                            new Order(50, "Demon of Darkness", "wait"),
                            new Shoot(20, count: 6, projectileIndex: 1, shootAngle: 60, angleOffset: 0, coolDown: 900),
                            new Shoot(20, count: 6, projectileIndex: 1, shootAngle: 60, angleOffset: 10, coolDown: 900, coolDownOffset: 150),
                            new Shoot(20, count: 6, projectileIndex: 1, shootAngle: 60, angleOffset: 20, coolDown: 900, coolDownOffset: 300),
                            new Shoot(20, count: 6, projectileIndex: 1, shootAngle: 60, angleOffset: 30, coolDown: 900, coolDownOffset: 450),
                            new Shoot(20, count: 6, projectileIndex: 1, shootAngle: 60, angleOffset: 40, coolDown: 900, coolDownOffset: 600),
                            new Shoot(20, count: 6, projectileIndex: 1, shootAngle: 60, angleOffset: 50, coolDown: 900, coolDownOffset: 750),
                            new Shoot(20, projectileIndex: 0, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                            new TimedTransition(20000, "spawn"),
                            new HpLessTransition(0.1, "chase")
                            ),
                        new State("spawn",
                            new Reproduce("Minion of Despair", 20, 6, 1000, 1),
                            new Shoot(20, projectileIndex: 0, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                            new Grenade(5, 200, 8, coolDown: 2000),
                            new Prioritize(
                                new Follow(0.7, range: 2),
                                new Wander(0.4)
                                ),
                            new HpLessTransition(0.1, "chase")
                            ),
                        new State("chase",
                            new Shoot(10, projectileIndex: 2, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                            new Grenade(5, 200, 8, coolDown: 2000),
                            new Prioritize(
                                new Follow(1.2, range: 2),
                                new Wander(0.4)
                                )
                            )
                        ),
                    new State(
                        new State("spin2",
                            new Taunt(true, "You shall die!"),
                            new Shoot(20, count: 6, projectileIndex: 4, shootAngle: 60, angleOffset: 0, coolDown: 900),
                            new Shoot(20, count: 6, projectileIndex: 4, shootAngle: 60, angleOffset: 10, coolDown: 900, coolDownOffset: 150),
                            new Shoot(20, count: 6, projectileIndex: 4, shootAngle: 60, angleOffset: 20, coolDown: 900, coolDownOffset: 300),
                            new Shoot(20, count: 6, projectileIndex: 4, shootAngle: 60, angleOffset: 30, coolDown: 900, coolDownOffset: 450),
                            new Shoot(20, count: 6, projectileIndex: 4, shootAngle: 60, angleOffset: 40, coolDown: 900, coolDownOffset: 600),
                            new Shoot(20, count: 6, projectileIndex: 4, shootAngle: 60, angleOffset: 50, coolDown: 900, coolDownOffset: 750),
                            new Shoot(20, projectileIndex: 0, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                            new TimedTransition(30000, "spawn2"),
                            new HpLessTransition(0.1, "chase2")
                            ),
                        new State("spawn2",
                            new Reproduce("Minion of Darkness", 20, 3, 1500, 1),
                            new Reproduce("Minion of Despair", 20, 3, 1000, 1),
                            new Shoot(20, projectileIndex: 3, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                            new Shoot(20, projectileIndex: 6, count: 5, predictive: 1, coolDown: 1200, coolDownOffset: 600),
                            new Grenade(5, 200, 8, coolDown: 2000),
                            new Prioritize(
                                new Follow(0.7, range: 2),
                                new Wander(0.4)
                                ),
                            new HpLessTransition(0.1, "chase2")
                            ),
                        new State("chase2",
                            new Shoot(10, projectileIndex: 5, count: 9, shootAngle: 10, predictive: 1, coolDown: 1200),
                            new Shoot(20, projectileIndex: 6, count: 5, predictive: 1, coolDown: 1200, coolDownOffset: 600),
                            new Grenade(5, 200, 8, coolDown: 2000),
                            new Prioritize(
                                new Follow(1.2, range: 2),
                                new Wander(0.4)
                                )
                            )
                        )
                    ),
                new Threshold(0.05,
                    new TierLoot(9, ItemType.Weapon, 0.2),
                    new TierLoot(10, ItemType.Weapon, 0.1),
                    new TierLoot(11, ItemType.Weapon, 0.05),
                    new ItemLoot("Potion of Defense", 0.50),
                    new ItemLoot("Potion of Vitality", 0.50),
                    new ItemLoot("Demon Essence", 0.01)
                    )
            )
            .Init("Minion of Despair",
                new State(
                    new PlayerScaleHealth(800),
                    new PlayerScaleDefense(2),
                    new Shoot(10, predictive: 0.5, coolDown: 500),
                    new Prioritize(
                        new Follow(1.5, range: 1),
                        new Wander(1)
                        )
                    )
            )
            .Init("Mysterious Head",
                new State(
                    new State("default",
                        new PlayerWithinTransition(10, "init")
                        ),
                    new State("init",
                        new PlayerScaleHealth(15000),
                        new PlayerScaleDefense(1),
                        new Order(40, "Mysterious Hand", "init"),
                        new TimedTransition(1000, "attack1")
                        ),
                    new State("attack1",
                        new Order(40, "Mysterious Hand", "attack1"),
                        new ConditionalEffect(ConditionEffectIndex.Invulnerable),
                        new Shoot(10, 14, 15, coolDown: 2000),
                        new ConditionalTransition(new EntityCountLessThan("Mysterious Hand", 40, 2), "attack2")
                        ),
                    new State("attack2",
                        new Order(40, "Mysterious Hand", "attack2"),
                        new ConditionalEffect(ConditionEffectIndex.Armored),
                        new Shoot(10, 14, 15, coolDown: 1500),
                        new ConditionalTransition(new EntityCountLessThan("Mysterious Hand", 40, 1), "rage")
                        ),
                    new State("rage",
                        new Follow(0.7, acquireRange: 20, range: 4),
                        new Shoot(10, 17, coolDown: 1000)
                        )
                    )
            )
            .Init("Mysterious Hand",
                new State(
                    new State("default",
                        new SetAltTexture(1),
                        new ConditionalEffect(ConditionEffectIndex.Invincible)
                        ),
                    new State("init",
                        new PlayerScaleHealth(8000),
                        new PlayerScaleDefense(1),
                        new SetAltTexture(0)
                        ),
                    new State("attack1",
                        new Shoot(20, 15, coolDown: 1000),
                        new Prioritize(
                            new StayCloseToSpawn(0.8, range: 10),
                            new Follow(0.8, acquireRange: 20, range: 3, duration: 2000, coolDown: 4000),
                            new Wander(0.8)
                            )
                        ),
                    new State("attack2",
                        new Heal(40, "Mysterious", coolDown: 2000),
                        new Shoot(20, 15, coolDown: 750),
                        new Prioritize(
                            new Follow(1.2, acquireRange: 20, range: 1, duration: 5000, coolDown: 4000),
                            new Wander(0.8)
                            )
                        )
                    )
            )
            ;
    }
}