using System;
using System.Collections.Generic;
using Urho;
using Urho.Gui;

namespace ARKitXamarinDemo
{
    public class ArkitDebugMenu
    {
        static List<Button> buttons;

        public static void Initialize(Urho.Application app, List<DebugAction> handlers)
        {
            app.UI.Root.SetDefaultStyle(CoreAssets.UIs.DefaultStyle);

            buttons = new List<Button>(handlers.Count);
            for (int j = 0; j < handlers.Count + 1; j++)
            {
                var w = app.Graphics.Width / (handlers.Count + 1);
                var h = app.Graphics.Height / 25;

                var button = new Button();
                app.UI.Root.AddChild(button);
                button.SetStyle("Button");
                button.SetSize(w, h);
                button.Position = new IntVector2(w * j, 0);
                button.Visible = false;

                var label = new Text();
                button.AddChild(label);
                label.SetStyle("Text");
				label.SetFontSize((int)(label.FontSize / 1f));
                label.HorizontalAlignment = HorizontalAlignment.Center;
                label.VerticalAlignment = VerticalAlignment.Center;

                int index = j;
                if (j == handlers.Count)
                {
                    label.Value = "debug menu";
                    button.Visible = true;
                    button.Pressed += args => buttons.ForEach(b => b.Visible = !b.Visible);
                }
                else
                {
                    label.Value = handlers[j].Name;
                    button.Pressed += args => handlers[index].Action();
                    buttons.Add(button);
                }
            }
        }
    }

    public class DebugAction
    {
        public DebugAction(string name, Action action)
        {
            Name = name;
            Action = action;
        }

        public string Name { get; }
        public Action Action { get; }
    }
}
