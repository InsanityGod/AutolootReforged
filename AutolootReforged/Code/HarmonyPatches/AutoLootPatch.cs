using AutoLootReforged.Code.Custom;
using HarmonyLib;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace AutoLootReforged.Code.HarmonyPatches
{
    [HarmonyPatch]
    public static class AutoLootPatch
    {
        [HarmonyPatch(typeof(GuiDialogCreatureContents), nameof(GuiDialogCreatureContents.OnGuiOpened))]
        [HarmonyPostfix]
        public static void OnGuiOpened(GuiDialogCreatureContents __instance)
        {
            Traverse self = Traverse.Create(__instance);

			InventoryBase inventory = self.Field<InventoryGeneric>("inv").Value;
            if(inventory.ClassName != "harvestableContents") return;

			Entity owningEntity = self.Field<Entity>("owningEntity").Value;

            IAttribute harvest = owningEntity.WatchedAttributes["harvestableInv"];
            if(harvest != null && !inventory.Empty)
            {
                AutoLootReforgedModSystem.ClientAPI.World.RegisterCallback(_ => Loot(__instance, owningEntity, inventory), 10);
            }
            else if(!owningEntity.WatchedAttributes.OnModified.OfType<HarvetableListener>().Any())
            {
                owningEntity.WatchedAttributes.OnModified.Add(new HarvetableListener() { path = "harvestableInv", listener = () => AutoLootReforgedModSystem.ClientAPI.World.RegisterCallback(_ => Loot(__instance, owningEntity, inventory), 10)});
            }
        }

        public static void Loot(GuiDialogCreatureContents dialog, Entity owningEntity, InventoryBase inventory)
        {
            var player = AutoLootReforgedModSystem.ClientAPI.World.Player;

            var strBuilder = AutoLootReforgedModSystem.Config.Log ? new StringBuilder("AutoLoot:") : null;

            if (!inventory.Empty)
            {
                for (var i = 0; i < inventory.Count; i++)
                {
                    var slot = inventory[i];
                    if (slot.Empty) continue;
                    strBuilder?.AppendLine($"Looting slot {i}, containg {slot.Itemstack.StackSize} x {slot.Itemstack.GetName()}");

                    ItemStackMoveOperation operation = new(AutoLootReforgedModSystem.ClientAPI.World, EnumMouseButton.Left, EnumModifierKey.SHIFT, EnumMergePriority.AutoMerge, inventory[i].StackSize)
                    {
                        ActingPlayer = player
                    };

                    var packet = inventory.ActivateSlot(i, inventory[i], ref operation);
                    AutoLootReforgedModSystem.ClientAPI.Network.SendEntityPacket(owningEntity.EntityId, packet);

                    if(operation.MovedQuantity > 0) strBuilder?.AppendLine($"Moved {operation.MovedQuantity} items");
                    if (operation.NotMovedQuantity > 0)
                    {
                        strBuilder?.AppendLine($"Failed to move {operation.NotMovedQuantity} items");
                    }
                }

                if(AutoLootReforgedModSystem.Config.Sound) AutoLootReforgedModSystem.ClientAPI.World.PlaySoundFor("autolootreforged:sounds/loot", player, false);

                if (inventory.Empty) AutoLootReforgedModSystem.ClientAPI.Event.EnqueueMainThreadTask(() => dialog.TryClose(), "closedlg");
            }
            else
            {
                strBuilder?.AppendLine("Inventory was empty");
                if(AutoLootReforgedModSystem.Config.Sound) AutoLootReforgedModSystem.ClientAPI.World.PlaySoundFor("autolootreforged:sounds/loot", player, false);
                AutoLootReforgedModSystem.ClientAPI.Event.EnqueueMainThreadTask(() => dialog.TryClose(), "closedlg");
            }

            if (strBuilder != null) AutoLootReforgedModSystem.ClientAPI.Logger.Log(EnumLogType.Notification, strBuilder.ToString());

            var listener = owningEntity.WatchedAttributes.OnModified.OfType<HarvetableListener>().FirstOrDefault();
            if(listener != null) owningEntity.WatchedAttributes.OnModified.Remove(listener);
        }

        [HarmonyPatch(typeof(EntityBehaviorHarvestable), nameof(EntityBehaviorHarvestable.OnInteract))]
        [HarmonyPostfix]
        public static void PostFixDialog(EntityBehaviorHarvestable __instance)
        {
            var dlg = Traverse.Create(__instance).Field<GuiDialogCreatureContents>("dlg").Value;
            if(dlg == null) return;
            
            AutoLootReforgedModSystem.ClientAPI.World.RegisterCallback(_ =>
            {
                //This is to fix an edge case where you open the inventory again right after the entity has been disposed but the client hasn't realized it yet
                if(AutoLootReforgedModSystem.ClientAPI.World.GetNearestEntity(__instance.entity.Pos.XYZ, 1, 1, (e) => e.EntityId == __instance.entity.EntityId) == null)
                {
                    AutoLootReforgedModSystem.ClientAPI.Event.EnqueueMainThreadTask(() => dlg.TryClose(), "closedlg");
                }
            }, AutoLootReforgedModSystem.Config.CheckDubbleWindowOpenIntervalInMs);
            
        }
    }
}
