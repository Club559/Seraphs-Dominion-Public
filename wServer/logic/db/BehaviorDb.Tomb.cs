﻿using wServer.logic.behaviors;
using wServer.logic.loot;
using wServer.logic.transitions;

namespace wServer.logic
{
    partial class BehaviorDb
    {
        private _ Tomb = () => Behav()
            .Init("Tomb Defender",
                new State(
                    new State("idle",
                        new Taunt("THIS WILL NOW BE YOUR TOMB!"),
                        new ConditionalEffect(ConditionEffectIndex.Armored),
                        new Orbit(.6, 5, target: "Tomb Boss Anchor", radiusVariance: 0.5),
                        new HpLessTransition(.99, "weakning")
                        ),
                    new State("weakning",
                        new Orbit(.6, 5, target: "Tomb Boss Anchor", radiusVariance: 0.5),
                        new Taunt("Impudence! I am an immortal, I needn't take you seriously."),
                        new Shoot(50, 24, projectileIndex: 3, coolDown: 6000, coolDownOffset: 2000),
                        new HpLessTransition(.97, "active")
                        ),
                    new State("active",
                        new Orbit(.7, 5, target: "Tomb Boss Anchor", radiusVariance: 0.5),
                        new Shoot(50, 8, projectileIndex: 2, coolDown: 1000, coolDownOffset: 500),
                        new Shoot(50, 4, projectileIndex: 1, coolDown: 3000, coolDownOffset: 500),
                        new Shoot(50, 6, projectileIndex: 0, coolDown: 3100, coolDownOffset: 500),
                        new HpLessTransition(.9, "boomerang")
                        ),
                    new State("boomerang",
                        new ConditionalEffect(ConditionEffectIndex.Armored),
                        new Orbit(.6, 5, target: "Tomb Boss Anchor", radiusVariance: 0.5),
                        new Shoot(50, 8, projectileIndex: 2, coolDown: 1000, coolDownOffset: 500),
                        new Shoot(50, 8, 10, 1, coolDown: 4750, coolDownOffset: 500),
                        new Shoot(50, 1, 10, 0, coolDown: 4750, coolDownOffset: 500),
                        new HpLessTransition(.66, "double shot")
                        ),
                    new State("double shot",
                        new ConditionalEffect(ConditionEffectIndex.Armored),
                        new Orbit(.7, 5, target: "Tomb Boss Anchor", radiusVariance: 0.5),
                        new Shoot(50, 8, projectileIndex: 2, coolDown: 1000, coolDownOffset: 500),
                        new Shoot(50, 8, 10, 1, coolDown: 4750, coolDownOffset: 500),
                        new Shoot(50, 2, 10, 0, coolDown: 4750, coolDownOffset: 500),
                        new HpLessTransition(.5, "artifacts")
                        ),
                    new State("triple shot",
                        new ConditionalEffect(ConditionEffectIndex.Armored),
                        new Orbit(.6, 5, target: "Tomb Boss Anchor", radiusVariance: 0.5),
                        new Shoot(50, 8, projectileIndex: 2, coolDown: 1000, coolDownOffset: 500),
                        new Shoot(50, 8, 10, 1, coolDown: 4750, coolDownOffset: 500),
                        new Shoot(50, 3, 10, 0, coolDown: 4750, coolDownOffset: 500),
                        new HpLessTransition(.1, "rage")
                        ),
                    new State("artifacts",
                        new ConditionalEffect(ConditionEffectIndex.Armored),
                        new Taunt("My artifacts shall prove my wall of defense is impenetrable!"),
                        new Orbit(.6, 5, target: "Tomb Boss Anchor", radiusVariance: 0.5),
                        new Shoot(50, 8, projectileIndex: 2, coolDown: 1000, coolDownOffset: 500),
                        new Shoot(50, 8, 10, 1, coolDown: 4750, coolDownOffset: 500),
                        new Shoot(50, 2, 10, 0, coolDown: 4750, coolDownOffset: 500),
                        new Spawn("Pyramid Artifact 1", 3, 0, 3500000),
                        new Reproduce("Pyramid Artifact 1", 10, 3, 1500),
                        new Spawn("Pyramid Artifact 2", 3, 0, 3500000),
                        new Reproduce("Pyramid Artifact 2", 10, 3, 1500),
                        new Spawn("Pyramid Artifact 3", 3, 0, 3500000),
                        new Reproduce("Pyramid Artifact 3", 10, 3, 1500),
                        new HpLessTransition(.33, "triple shot")
                        ),
                    new State("rage",
                        new Taunt("The end of your path is here!"),
                        new Follow(0.6, range: 1, duration: 5000, coolDown: 0),
                        new Flash(0xfFF0000, 1, 9000001),
                        new Shoot(50, 8, 10, 1, coolDown: 4750, coolDownOffset: 500),
                        new Shoot(50, 4, 10, 4, coolDown: 300),
                        new Shoot(50, 3, 10, 0, coolDown: 4750, coolDownOffset: 500)
                        )
                    ),
                new ItemLoot("Ring of the Pyramid", 0.002),
                new ItemLoot("Tome of Holy Protection", 0.002),
                new ItemLoot("Wine Cellar Incantation", 0.002),
                new ItemLoot("Potion of Life", 0.5)
            )



            //Minions
            .Init("Pyramid Artifact 1",
                new State(
                    new Prioritize(
                        new Orbit(1, 2, target: "Tomb Defender", radiusVariance: 0.5),
                        new Follow(0.85, range: 1, duration: 5000, coolDown: 0)
                        ),
                    new Shoot(3, coolDown: 2500)
                    ))
            .Init("Pyramid Artifact 2",
                new State(
                    new Prioritize(
                        new Orbit(1, 2, target: "Tomb Attacker", radiusVariance: 0.5),
                        new Follow(0.85, range: 1, duration: 5000, coolDown: 0)
                        ),
                    new Shoot(3, coolDown: 2500)
                    ))
            .Init("Pyramid Artifact 3",
                new State(
                    new Prioritize(
                        new Orbit(1, 2, target: "Tomb Support", radiusVariance: 0.5),
                        new Follow(0.85, range: 1, duration: 5000, coolDown: 0)
                        ),
                    new Shoot(3, coolDown: 2500)
                    ))
            .Init("Tomb Defender Statue",
                new State(
                    new State(
                        new ConditionalEffect(ConditionEffectIndex.Invulnerable),
                        new EntityNotExistsTransition("Inactive Sarcophagus", 1000, "checkActive"),
                        new EntityNotExistsTransition("Active Sarcophagus", 1000, "checkInactive")
                        ),
                    new State("checkActive",
                        new EntityNotExistsTransition("Active Sarcophagus", 1000, "ItsGoTime")
                        ),
                    new State("checkInactive",
                        new EntityNotExistsTransition("Inactive Sarcophagus", 1000, "ItsGoTime")
                        ),
                    new State("ItsGoTime",
                        new Transform("Tomb Defender")
                        )
                    ))
            .Init("Tomb Support Statue",
                new State(
                    new State(
                        new ConditionalEffect(ConditionEffectIndex.Invulnerable),
                        new EntityNotExistsTransition("Inactive Sarcophagus", 1000, "checkActive"),
                        new EntityNotExistsTransition("Active Sarcophagus", 1000, "checkInactive")
                        ),
                    new State("checkActive",
                        new EntityNotExistsTransition("Active Sarcophagus", 1000, "ItsGoTime")
                        ),
                    new State("checkInactive",
                        new EntityNotExistsTransition("Inactive Sarcophagus", 1000, "ItsGoTime")
                        ),
                    new State("ItsGoTime",
                        new Transform("Tomb Support")
                        )
                    ))
            .Init("Tomb Attacker Statue",
                new State(
                    new State(
                        new ConditionalEffect(ConditionEffectIndex.Invulnerable),
                        new EntityNotExistsTransition("Inactive Sarcophagus", 1000, "checkActive"),
                        new EntityNotExistsTransition("Active Sarcophagus", 1000, "checkInactive")
                        ),
                    new State("checkActive",
                        new EntityNotExistsTransition("Active Sarcophagus", 1000, "ItsGoTime")
                        ),
                    new State("checkInactive",
                        new EntityNotExistsTransition("Inactive Sarcophagus", 1000, "ItsGoTime")
                        ),
                    new State("ItsGoTime",
                        new Transform("Tomb Attacker")
                        )
                    ))
            .Init("Inactive Sarcophagus",
                new State(
                    new State(
                        new ConditionalEffect(ConditionEffectIndex.Invulnerable),
                        new EntityNotExistsTransition("Beam Priestess", 14, "checkPriest"),
                        new EntityNotExistsTransition("Beam Priest", 1000, "checkPriestess")
                        ),
                    new State("checkPriest",
                        new EntityNotExistsTransition("Beam Priest", 1000, "activate")
                        ),
                    new State("checkPriestess",
                        new EntityNotExistsTransition("Beam Priestess", 1000, "activate")
                        ),
                    new State("activate",
                        new Transform("Active Sarcophagus")
                        )
                    ))
            .Init("Active Sarcophagus",
                new State(
                    new State(
                        new HpLessTransition(60, "stun")
                        ),
                    new State("stun",
                        new Shoot(50, 8, 10, 0, coolDown: 9999999, coolDownOffset: 500),
                        new Shoot(50, 8, 10, 0, coolDown: 9999999, coolDownOffset: 1000),
                        new Shoot(50, 8, 10, 0, coolDown: 9999999, coolDownOffset: 1500),
                        new TimedTransition(1500, "idle")
                        ),
                    new State("idle",
                        new ChangeSize(100, 100)
                        )
                    ),
                new ItemLoot("Magic Potion", 0.002),
                new ItemLoot("Health Potion", 0.15),
                new ItemLoot("Tincture of Mana", 0.15),
                new ItemLoot("Tincture of Dexterity", 0.15),
                new ItemLoot("Tincture of Life", 0.15)
            );
    }
}