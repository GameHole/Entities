namespace Server
{
    using Entities;
    using Entities.Net;
    class JoinHandler : PaylodHandler<Join>
    {
        protected override void Handle(Entity client, Join Msg)
        {
            //if(client.Has<>)
            var ok = new JoinOk();
            world.GetShared<INetShared>().Send(client, ref ok);
        }
    }
}
