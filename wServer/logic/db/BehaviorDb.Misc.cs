using wServer.logic.behaviors;
using wServer.logic.transitions;

namespace wServer.logic
{
    partial class BehaviorDb
    {
        private _ Misc = () => Behav()
            .Init("White Fountain",
                new State(
                    //new NexusHealHp(5, 100, 1000)
                    new NexusHealHpAOE(5, 100, 1000)
                    )
            )
            .Init("Sheep",
                new State(
                    new PlayerWithinTransition(15, "player_nearby"),
                    new State("player_nearby",
                        new Prioritize(
                            new StayCloseToSpawn(0.1, 2),
                            new Wander(0.1)
                            ),
                        new Taunt(0.001, 1000, "baa", "baa baa")
                        )
                    )
            )
            .Init("Forgotten Archmage of Flame",
                //Luci, I'll be using this guy's behaviors as a backup when I forget/don't know how your code works
                new State(
                    new Prioritize(
                        new Wander(0.1)
                        ),
                    new Shoot(10, 6, 60, fixedAngle: 0, coolDown: 1200, coolDownOffset: 0),
                    new Shoot(10, 6, 60, fixedAngle: 10, coolDown: 1200, coolDownOffset: 200),
                    new Shoot(10, 6, 60, fixedAngle: 20, coolDown: 1200, coolDownOffset: 400),
                    new Shoot(10, 6, 60, fixedAngle: 30, coolDown: 1200, coolDownOffset: 600),
                    new Shoot(10, 6, 60, fixedAngle: 40, coolDown: 1200, coolDownOffset: 800),
                    new Shoot(10, 6, 60, fixedAngle: 50, coolDown: 1200, coolDownOffset: 1000)
                    )
            )
            .InitMany("Black Cat", "Snowman", name =>
                new State(
                    new PetChasing(1.5, 7, 1)
                    )
            );
    }
}