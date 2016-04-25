using System;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace RestoreCharInv
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class BaseModSessionComponent : MySessionComponentBase
    {
        // set your testmod's id to this in order to have more debug messages
        private const long DebugModId = 0;

        private bool _isInitialized;
        private bool _isServer;

        private readonly Action<byte[]> _messageHandler = HandleMessage;

        public override void UpdateBeforeSimulation()
        {
            // init clients
            if (!_isInitialized && MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null)
            {

                if (!MyAPIGateway.Session.OnlineMode.Equals(MyOnlineModeEnum.OFFLINE) && MyAPIGateway.Multiplayer.IsServer && !MyAPIGateway.Utilities.IsDedicated)
                    // init hosts
                    InitServer();

                InitClient();
            }

            // init servers
            if (!_isInitialized && MyAPIGateway.Utilities != null && MyAPIGateway.Multiplayer != null
                && MyAPIGateway.Session != null && MyAPIGateway.Utilities.IsDedicated && MyAPIGateway.Multiplayer.IsServer)
            {
                InitServer();
            }

            TimerRegistry.Update();
            base.UpdateBeforeSimulation();
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(ConnectionHelper.ConnectionId, _messageHandler);
            TimerRegistry.Close();

            // close things here

            base.UnloadData();
        }

        public override void SaveData()
        {
            // save things here
            base.SaveData();
        }

        private void InitServer()
        {
            _isInitialized = true;
            Logger.LogInfo("Init Server.");
            SetDebugMode();
            MyAPIGateway.Multiplayer.RegisterMessageHandler(ConnectionHelper.ConnectionId, _messageHandler);
        }

        private void InitClient()
        {
            _isInitialized = true;
            Logger.LogInfo("Init Client.");
            _isServer = true;
            // might be true for hosts
            if (!_isServer)
            {
                SetDebugMode();
                MyAPIGateway.Multiplayer.RegisterMessageHandler(ConnectionHelper.ConnectionId, _messageHandler);
            }
        }

        private static void HandleMessage(byte[] message)
        {
            ConnectionHelper.ProcessData(message);
        }

        private void SetDebugMode()
        {
            var mods = MyAPIGateway.Session.GetCheckpoint("null").Mods;

            if (mods == null)
                return;

            if (mods.Any(mod => mod.PublishedFileId == DebugModId))
                Logger.DebugEnabled = true;
        }
    }
}
