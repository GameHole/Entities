namespace Entities.Net
{
    public abstract class ADealer
    {
        protected World world { get; private set; }
        internal void Setworld(World world)
        {
            this.world = world;
        }
        public abstract void Run(Entity client, IPay patload);
    }
}
