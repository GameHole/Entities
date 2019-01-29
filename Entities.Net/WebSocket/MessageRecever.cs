using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Entities.Net
{
    class MessageRecever
    {
        enum State { Head, Body }
        private TaskCompletionSource<byte[]> tcs;
        private byte[] data;
        private byte[] recvBuffer = new byte[35];
        private bool isMasked;
        private byte[] mask = new byte[4];
        private int recvIndex;
        private int copyedIndex;
        private State state = State.Head;
        private Socket socket;
        public event Action onOver;
        private bool isRun = false;
        public MessageRecever(Socket socket)
        {
            this.socket = socket;
        }
        public Task<byte[]> Take()
        {
            tcs = new TaskCompletionSource<byte[]>();
            if (!isRun)
            {
                Run();
            }
            return tcs.Task;
        }
        public async void Run()
        {
            isRun = true;
            await Task.Factory.StartNew(() =>
            {
                while (isRun)
                {
                    int n = 0;
                    try
                    {
                        n = socket.Receive(recvBuffer, recvIndex, recvBuffer.Length - recvIndex, SocketFlags.None);
                    }
                    catch(SocketException se)
                    {
                        if (se.ErrorCode == 10054)
                        {
                            n = 0;
                        }
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                        return;
                    }
                    
                    //Console.WriteLine(n);
                    if (n == 0)
                    {
                        Over();
                        return;
                    }
                    Read(n + recvIndex);
                }
            }, TaskCreationOptions.LongRunning);
        }
        void Over()
        {
            onOver?.Invoke();
            isRun = false;
        }
        void unMask()
        {
            if (!isMasked) return;
            MaskHelper.UnMask(data, mask);
        }
        private void Read(int lastIndex)
        {
            int runIndex = 0;
            while (true)
            {
                switch (state)
                {
                    case State.Head:
                        int size = lastIndex - runIndex;
                        if (size< 8)
                        {
                            Array.Copy(recvBuffer, runIndex, recvBuffer, 0, lastIndex - runIndex);
                            recvIndex = size;
                            return;
                        }
                        //websocket 主动关闭连接
                        if ((recvBuffer[runIndex] & 0x0f) == 8)
                        {
                            Over();
                            return;
                        }
                        runIndex++;
                        int payloadLenght = recvBuffer[runIndex] & 0x7f;
                        isMasked =(recvBuffer[runIndex] & 0x80) == 0x80;
                        runIndex++;
                        if(payloadLenght>=127)
                        {
                            throw new ArgumentException("Pay Load Is Too Big!!!");
                        }
                        else if (payloadLenght == 126)
                        {
                            payloadLenght = BitConverter.ToUInt16(recvBuffer, runIndex);
                            runIndex += 2;
                        }
                        if (isMasked)
                        {
                            Array.Copy(recvBuffer, runIndex, mask, 0, 4);
                            runIndex += 4;
                        }
                        data = new byte[payloadLenght];
                        state = State.Body;
                        break;
                    case State.Body:
                        int willCopyLenght = lastIndex - runIndex;
                        //Console.WriteLine($"willCopyLenght::{willCopyLenght}");
                        //Console.WriteLine($"copyedIndex::{copyedIndex}");
                        if (copyedIndex > 0)
                        {
                            int n = data.Length - copyedIndex;
                            if (willCopyLenght >= n)
                            {
                                Array.Copy(recvBuffer, runIndex, data, copyedIndex, n);
                                copyedIndex = 0;
                                runIndex += n;
                                unMask();
                                tcs.SetResult(data);
                                state = State.Head;
                                continue;
                            }
                            else
                            {
                                Array.Copy(recvBuffer, runIndex, data, copyedIndex, willCopyLenght);
                                copyedIndex += willCopyLenght;
                                return;
                            }
                        }
                        if (willCopyLenght >= data.Length)
                        {
                            Array.Copy(recvBuffer, runIndex, data, copyedIndex, data.Length);
                            unMask();
                            tcs.SetResult(data);
                            runIndex += data.Length;
                            state = State.Head;
                            if (runIndex>=lastIndex)
                            {
                                recvIndex = 0;
                                return;
                            }
                        }
                        else
                        {
                            Array.Copy(recvBuffer, runIndex, data, copyedIndex, willCopyLenght);
                            copyedIndex += willCopyLenght;
                            recvIndex = 0;
                            return;
                        }
                        break;
                }
            }
        }
    }
}
