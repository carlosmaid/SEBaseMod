using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Game.Entities;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

namespace RestoreCharInv
{

    public static class Support
    {
        #region Inventory

        public static bool InventoryAdd(VRage.Game.Entity.MyEntity entity, MyFixedPoint amount, MyDefinitionId definitionId)
        {
            var itemAdded = false;
            var count = entity.InventoryCount;

            // Try to find the right inventory to put the item into.
            // Ie., Refinery has 2 inventories. One for ore, one for ingots.
            for (int i = 0; i < count; i++)
            {
                var inventory = entity.GetInventory(i);
                if (inventory.CanItemsBeAdded(amount, definitionId))
                {
                    itemAdded = true;
                    Support.InventoryAdd(inventory, amount, definitionId);
                    break;
                }
            }

            return itemAdded;
        }

        public static bool InventoryAdd(IMyInventory inventory, MyFixedPoint amount, MyDefinitionId definitionId)
        {
            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);

            var gasContainer = content as MyObjectBuilder_GasContainerObject;
            if (gasContainer != null)
                gasContainer.GasLevel = 1f;

            MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = amount, Content = content };

            if (inventory.CanItemsBeAdded(inventoryItem.Amount, definitionId))
            {
                inventory.AddItems(inventoryItem.Amount, (MyObjectBuilder_PhysicalObject)inventoryItem.Content, -1);
                return true;
            }

            // Inventory full. Could not add the item.
            return false;
        }

        #endregion
    }
}
