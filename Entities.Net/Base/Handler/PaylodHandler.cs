namespace Entities.Net
{
    public abstract class PaylodHandler<T> : ADealer where T : struct, IToBytes
    {
        public override void Run(Entity client, IPay patload)
        {
            Handle(client, (patload as PayloadPakcet<T>).value);
        }
        protected abstract void Handle(Entity client, T Msg);
    }
}
