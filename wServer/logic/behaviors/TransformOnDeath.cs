using wServer.realm;

namespace wServer.logic.behaviors
{
    internal class TransformOnDeath : Behavior
    {
        private readonly int max;
        private readonly int min;
        private readonly float probability;
        private readonly ushort target;

        public TransformOnDeath(string target, int min = 1, int max = 1, double probability = 1)
        {
            this.target = BehaviorDb.InitGameData.IdToObjectType[target];
            this.min = min;
            this.max = max;
            this.probability = (float) probability;
        }

        protected internal override void Resolve(State parent)
        {
            parent.Death += (sender, e) =>
            {
                if (e.Host.CurrentState.Is(parent) &&
                    Random.NextDouble() < probability)
                {
                    int count = Random.Next(min, max + 1);
                    for (int i = 0; i < count; i++)
                    {
                        Entity entity = Entity.Resolve(e.Host.Manager, target);

                        entity.Move(e.Host.X, e.Host.Y);
                        e.Host.Owner.EnterWorld(entity);
                    }
                }
            };
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
        }
    }
}