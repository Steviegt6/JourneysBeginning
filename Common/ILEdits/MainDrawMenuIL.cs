﻿using JourneysBeginning.Common.Bases;
using JourneysBeginning.Common.Utilities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.UI.Chat;

namespace JourneysBeginning.Common.ILEdits
{
    internal sealed class MainDrawMenuIL : ILEdit
    {
        public override string DictKey => "Terraria.Main.DrawMenu";

        public override void Load() => IL.Terraria.Main.DrawMenu += ChangeDrawMethodIL;

        public override void Unload() => IL.Terraria.Main.DrawMenu -= ChangeDrawMethodIL;

        private void ChangeDrawMethodIL(ILContext il)
        {
            il.CreateCursor(out ILCursor c);

            ModifyCopyrightTextDrawMethod(c);
            ModifyVersionTextAddChangelogText(c);

            ILHelper.LogILCompletion("DrawMenu");
        }

        private void ModifyCopyrightTextDrawMethod(ILCursor c)
        {
            if (!c.TryGotoNext(x => x.MatchLdstr("Copyright © 2017 Re-Logic")))
            {
                ILHelper.LogILError("ldstr", "Copyright © 2017 Re-Logic");
                return;
            }

            if (!c.TryGotoNext(x => x.MatchLdloc(179)))
            {
                ILHelper.LogILError("ldloc.s", "179");
                return;
            }

            for (int i = 0; i < 2; i++)
                if (!c.TryGotoNext(x => x.MatchLdsfld(typeof(Main).GetField("spriteBatch", BindingFlags.Static | BindingFlags.Public))))
                {
                    ILHelper.LogILError("ldsfld", "Terraria.Main::spriteBatch");
                    return;
                }

            c.Index++;

            c.RemoveRange(31);

            c.Emit(OpCodes.Ldloc_2); // color
            c.Emit(OpCodes.Ldloc, 179); // text to draw
            c.EmitDelegate<Action<Color, string>>((color, text) =>
            {
                // Match the drawColor to what text would normally be drawn like
                // Thanks, Terraria
                Color drawColor = color;
                drawColor.R = (byte)((255 + drawColor.R) / 2);
                drawColor.G = (byte)((255 + drawColor.R) / 2);
                drawColor.B = (byte)((255 + drawColor.R) / 2);
                drawColor.A = (byte)(drawColor.A * 0.3f);

                Utils.DrawBorderString(Main.spriteBatch, text, new Vector2(Main.screenWidth - Main.fontMouseText.MeasureString(text).X - 10f, Main.screenHeight - 2f - Main.fontMouseText.MeasureString(text).Y), drawColor);
            });
        }

        private void ModifyVersionTextAddChangelogText(ILCursor c)
        {
            if (!c.TryGotoNext(x => x.MatchStloc(193)))
            {
                ILHelper.LogILError("stloc.s", "193");
                return;
            }

            if (!c.TryGotoNext(x => x.MatchLdsfld(typeof(Main).GetField("spriteBatch", BindingFlags.Static | BindingFlags.Public))))
            {
                ILHelper.LogILError("ldsfld", "Terraria.Main::spriteBatch");
                return;
            }

            c.Index++;

            c.RemoveRange(28); // Remove the entire method

            c.Emit(OpCodes.Ldloc, 186); // num107 (for index)
            c.Emit(OpCodes.Ldloc, 187); // text color
            c.Emit(OpCodes.Ldloc, 188); // x offset
            c.Emit(OpCodes.Ldloc, 193); // text to draw
            c.EmitDelegate<Action<int, Color, int, string>>((index, drawColor, xOffset, text) =>
            {
                // Only draw when the white text would draw
                if (index == 4)
                {
                    DrawVersionText(drawColor, xOffset, text);
                    DrawChangelog();
                }
            });
        }

        private void DrawVersionText(Color drawColor, int xOffset, string text)
        {
            // Split the text into a list of strings divided by newlines, reverse to order them properly, then convert them to an array
            string[] lines = text.Split('\n').Reverse().ToArray();

            // Draw text for each line, offsetting it on the y-axis as well
            for (int i = 0; i < lines.Length; i++)
                Utils.DrawBorderString(Main.spriteBatch, lines[i], new Vector2(xOffset + 10f, Main.screenHeight - 2f - (28f * (i + 1))), drawColor);

            // Finally, draw our text at the very top
            Utils.DrawBorderString(Main.spriteBatch, $"Journey's Beginning {JourneysBeginning.Instance.Version}", new Vector2(xOffset + 10f, Main.screenHeight - 28f - 2f - Main.fontMouseText.MeasureString(text).Y), Color.Goldenrod);
        }

        private void DrawChangelog()
        {
            // This code is only ran *after* updating, and will not run after reloading
            if (JourneysBeginning.Instance.showChangelogTextVersionDifference)
            {
                string collapseText = JourneysBeginning.Instance.showChangelogTextOptional ?
                    "Collapse JB Changelog" :
                    "Open JB Changelog";

                // todo: read from file
                string changelogText = $"JB v{JourneysBeginning.Instance.Version} Changelog:" +
                    "\nLorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris condimentum scelerisque rutrum. Morbi sagittis odio libero, ut volutpat augue vulputate aliquam." +
                    "\n* Curabitur dapibus imperdiet erat sed vestibulum." +
                    "\n* Sed elementum massa ac ipsum dignissim efficitur." +
                    "\n* In consequat ante pretium dolor convallis aliquet." +
                    "\n* Nam commodo consectetur mollis." +
                    "\n* Duis rhoncus bibendum condimentum." +
                    "\n* Nulla turpis velit, scelerisque eget sem eu, cursus consectetur odio." +
                    "\nClass aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Aliquam pellentesque purus ac posuere pellentesque.";

                // 10f is the default offset, we then measaure and offset the screen by how wide the text is so it properly shows
                // 38f is the defualt offset (10f) + how tall a fontMouseText string is (28f)
                Vector2 collapseTextPos = new Vector2(10f + Main.fontMouseText.MeasureString(collapseText).X, 38f);

                // This code is the same as above, but we offset the text on the y-axis as well to move it further down
                Vector2 changelogTextPos = new Vector2(10f + Main.fontMouseText.MeasureString(changelogText).X, 38f + Main.fontMouseText.MeasureString(changelogText).Y);

                // Parse and convert our strings into TextSnippet arrays for use with ChatManager.DrawColorCodedString
                TextSnippet[] collapseTextSnippet = ChatManager.ParseMessage(collapseText, Color.Goldenrod).ToArray();
                TextSnippet[] changelogTextSnippets = ChatManager.ParseMessage(changelogText, Color.White).ToArray();

                // Draw collapse text
                ChatManager.DrawColorCodedStringWithShadow(
                    Main.spriteBatch,
                    Main.fontMouseText,
                    collapseTextSnippet,
                    collapseTextPos,
                    0f,
                    Main.fontMouseText.MeasureString(collapseText),
                    Vector2.One,
                    out _,
                    maxWidth: 500f);

                // Draw changelog text
                // Only drawn if the changelog isn't collapsed
                if (JourneysBeginning.Instance.showChangelogTextOptional)
                    ChatManager.DrawColorCodedStringWithShadow(
                        Main.spriteBatch,
                        Main.fontMouseText,
                        changelogTextSnippets,
                        changelogTextPos,
                        0f,
                        Main.fontMouseText.MeasureString(changelogText),
                        Vector2.One,
                        out _,
                        maxWidth: 500f);

                // Turn the mouse into a rectangle so we can check for intersection
                Rectangle mouseRectangle = new Rectangle(Main.mouseX, Main.mouseY, 1, 1);

                int collapseTextStartingXCoord = (int)collapseTextPos.X - (int)Main.fontMouseText.MeasureString(collapseText).X;
                int collapseTextStartingYCoord = (int)collapseTextPos.Y - (int)Main.fontMouseText.MeasureString(collapseText).Y;
                Rectangle collapseTextRectangle = new Rectangle(
                    collapseTextStartingXCoord,
                    collapseTextStartingYCoord,
                    (int)Main.fontMouseText.MeasureString(collapseText).X, // Rectangle width should match text width
                    (int)Main.fontMouseText.MeasureString(collapseText).Y); // Rectangle height should match text height

                // Code that collapses and exapands the changelog
                // Main.mouseLeft && Main.mouseLeftRelease indicates that a user clicked, it's only true for one frame, the same frame the user lets go of the left mouse button
                if (Main.mouseLeft && Main.mouseLeftRelease && mouseRectangle.Intersects(collapseTextRectangle))
                    JourneysBeginning.Instance.showChangelogTextOptional = !JourneysBeginning.Instance.showChangelogTextOptional;
            }
        }
    }
}