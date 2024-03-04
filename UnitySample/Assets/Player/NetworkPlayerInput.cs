using Unity.Netcode;

public struct NetworkPlayerInput : PRN.IInput, INetworkSerializable
{

    public int tick;
    public int forward;
    public int right;
    public bool jump;

    // You need to implement those 2 methods
    public void SetTick(int tick) => this.tick = tick;
    public int GetTick() => tick;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref forward);
        serializer.SerializeValue(ref right);
        serializer.SerializeValue(ref jump);
    }

}
