using System;
using System.Linq;
using ChaosFramework.Collections.Immutable;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Menu.OpenGl.Controls;
using ChaosUtil.Reflection;
using LD60.Tactician;

namespace LD60.World.Tactician
{
    using PlacableMeta = TypeWithAttribute<PlacableAttribute>;

    public abstract class PlacableTool
        : Tool
    {
        public static readonly ImmutableArray<PlacableMeta> placables = PlacableMeta.GetTypes(typeof(Placable));

        static bool NotAbstract(Type t)
            => !t.IsAbstract;

        public override void Select()
        {
            base.Select();
            CreatePlacableButtons();
        }

        protected void CreatePlacableButtons()
        {
            scene.overlay.subTools.CreateControl<Image>(0, 0.05f).textureSource = string.Empty;
            foreach (Type it in AssemblyManager.SubTypesOf<Placable>().Where(NotAbstract))
            {
                Type placable = it;
                ToolButton btn = scene.overlay.subTools.CreateControl<ToolButton>(0, 0.05f);
                btn.texture = scene.overlay.toolRenderer.GetOrCreateImage(placable, this);
                ModifyPlacableButton(btn, placable);

                scene.overlay.panel.Layout(); // TODO: Don't
                Label lblAmount = btn.scene.CreateControl<Label>(btn.position + new Vector2f(btn.size.x, -btn.size.y), btn.size);
                lblAmount.textAlign = ChaosFramework.Graphics.Align.BottomRight;
                lblAmount.getText = _ => scene.inventory.GetArsenal(placable).ToString();
            }
            scene.overlay.panel.Layout();
        }

        protected abstract void ModifyPlacableButton(ToolButton btn, Type placable);
    }
}
