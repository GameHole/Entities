namespace Entities.Net
{
    public class DataSyncer<T> : ADealer where T : struct, IToBytes, IField
    {
        public override void Run(Entity client, IPay patload)
        {
            client.Set((patload as PayloadPakcet<T>).value);
        }
    }
}
