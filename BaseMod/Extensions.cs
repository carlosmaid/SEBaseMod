using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

namespace BaseMod
{
    public static class Extensions
    {
        #region player

        public static bool TryGetPlayer(this IMyPlayerCollection collection, string name, out IMyPlayer player)
        {
            player = null;
            if (string.IsNullOrEmpty(name))
                return false;
            var players = new List<IMyPlayer>();
            collection.GetPlayers(players, p => p != null);

            player = players.FirstOrDefault(p => p.DisplayName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (player == null)
                return false;

            return true;
        }

        public static bool TryGetPlayer(this IMyPlayerCollection collection, ulong steamId, out IMyPlayer player)
        {
            player = null;
            if (steamId == 0)
                return false;
            var players = new List<IMyPlayer>();
            collection.GetPlayers(players, p => p != null);

            player = players.FirstOrDefault(p => p.SteamUserId == steamId);
            if (player == null)
                return false;

            return true;
        }

        public static IMyPlayer Player(this IMyIdentity identity)
        {
            var listPlayers = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(listPlayers, p => p.PlayerID == identity.PlayerId);
            return listPlayers.FirstOrDefault();
        }

        public static IMyIdentity Player(this IMyPlayer player)
        {
            var listIdentites = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(listIdentites, p => p.IdentityId == player.IdentityId);
            return listIdentites.FirstOrDefault();
        }

        public static IMyPlayer FindPlayerBySteamId(this IMyPlayerCollection collection, ulong steamId)
        {
            var listplayers = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(listplayers, p => p.SteamUserId == steamId);
            return listplayers.FirstOrDefault();
        }

        /// <summary>
        /// Used to find the Character Entity (which is the physical representation in game) from the Player (the network connected human).
        /// This is a kludge as a proper API doesn't exist, even though the game code could easily expose this and save all this processing we are forced to do.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static IMyCharacter GetCharacter(this IMyPlayer player)
        {
            var character = player.Controller.ControlledEntity as IMyCharacter;
            if (character != null)
                return character;

            var cubeBlock = player.Controller.ControlledEntity as IMyCubeBlock;
            if (cubeBlock == null)
                return null;

            var shipController = cubeBlock as Sandbox.Game.Entities.MyShipController;
            if (shipController != null)
                return shipController.Pilot;

            // TODO: test conditions for MyCryoChamber block.

            // Cannot determine Character controlling MyLargeTurretBase as class is not whitelisted.
            // TODO: find if the player is controlling a turret.
            //var turretTontroller = cubeBlock as Sandbox.Game.Weapons.MyLargeTurretBase;
            //if (turretTontroller != null)
            //    return turretTontroller.Pilot;

            //var charComponent = cubeBlock.Components.Get<MyCharacterComponent>();

            //if (charComponent != null)
            //{
            //    var entity = charComponent.Entity;
            //    MyAPIGateway.Utilities.ShowMessage("Entity", "Good");
            //}
            //var turret = cubeBlock as Sandbox.Game.Weapons.MyLargeTurretBase; // not whitelisted.
            //var turret = cubeBlock as IMyControllableEntity;

            return null;
        }

        public static bool IsHost(this IMyPlayer player)
        {
            return MyAPIGateway.Multiplayer.IsServerPlayer(player.Client);
        }

        public static IMyInventory GetPlayerInventory(this IMyPlayer player)
        {
            var character = player.GetCharacter();
            if (character == null)
                return null;
            return character.GetPlayerInventory();
        }

        public static IMyInventory GetPlayerInventory(this IMyCharacter character)
        {
            if (character == null)
                return null;

            return ((MyEntity)character).GetInventory();
        }

        #endregion

        #region Definition

        public static MyPhysicalItemDefinition GetDefinition(this MyDefinitionManager definitionManager, string typeId, string subtypeName)
        {
            MyPhysicalItemDefinition definition = null;
            MyObjectBuilderType result;
            if (MyObjectBuilderType.TryParse(typeId, out result))
            {
                var id = new MyDefinitionId(result, subtypeName);
                MyDefinitionManager.Static.TryGetPhysicalItemDefinition(id, out definition);
            }

            return definition;
        }

        #endregion

        #region entity

        /// <summary>
        /// Creates the objectbuilder in game, and syncs it to the server and all clients.
        /// </summary>
        /// <param name="entity"></param>
        public static void CreateAndSyncEntity(this MyObjectBuilder_EntityBase entity)
        {
            CreateAndSyncEntities(new List<MyObjectBuilder_EntityBase> { entity });
        }

        /// <summary>
        /// Creates the objectbuilders in game, and syncs it to the server and all clients.
        /// </summary>
        /// <param name="entities"></param>
        public static void CreateAndSyncEntities(this List<MyObjectBuilder_EntityBase> entities)
        {
            MyAPIGateway.Entities.RemapObjectBuilderCollection(entities);
            entities.ForEach(item => MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(item));
            MyAPIGateway.Multiplayer.SendEntitiesCreated(entities);
        }

        #endregion

        #region misc/util

        /// <summary>
        /// Adds an element with the provided key and value to the System.Collections.Generic.IDictionary&gt;TKey,TValue&lt;.
        /// If the provide key already exists, then the existing key is updated with the newly supplied value.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="System.ArgumentNullException">key is null</exception>
        /// <exception cref="System.NotSupportedException">The System.Collections.Generic.IDictionary&gt;TKey,TValue&lt; is read-only.</exception>
        public static void Update<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        public static void ShowMessage(this IMyUtilities utilities, string sender, string messageText, params object[] args)
        {
            utilities.ShowMessage(sender, string.Format(messageText, args));
        }
        
        #endregion
    }
}
