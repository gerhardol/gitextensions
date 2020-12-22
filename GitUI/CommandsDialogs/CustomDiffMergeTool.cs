using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI
{
    public class CustomDiffMergeTool
    {
        public CustomDiffMergeTool(ToolStripMenuItem menuItem, System.EventHandler click)
        {
            _menuItem = menuItem;
            _click = click;
        }

        private ToolStripMenuItem _menuItem { get; set; }
        private System.EventHandler _click;

        public static void LoadCustomDiffMergeTools(GitModule module, IList<CustomDiffMergeTool> menus, IContainer components, bool isDiff, bool useSetting)
        {
            InitMenus(menus);

            if (useSetting && !AppSettings.ShowAvailableDiffTools)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                IEnumerable<string> tools = await module.GetCustomDiffMergeTools(isDiff: isDiff);
                if (tools.Count<string>() <= 1)
                {
                    // No need to show the menu
                    return;
                }

                foreach (var menu in menus)
                {
                    menu._menuItem.DropDown = new ContextMenuStrip(components);
                    foreach (var tool in tools)
                    {
                        var item = new ToolStripMenuItem(tool) { Tag = tool };

                        item.Click += menu._click;
                        menu._menuItem.DropDown.Items.Add(item);
                        if (menu._menuItem.DropDownItems.Count == 1)
                        {
                            item.Font = new Font(item.Font, FontStyle.Bold);
                            if (menu._menuItem.ShortcutKeys != Keys.None)
                            {
                                item.ShortcutKeys = menu._menuItem.ShortcutKeys;
                            }
                            else
                            {
                                item.ShortcutKeyDisplayString = menu._menuItem.ShortcutKeyDisplayString;
                            }
                        }
                    }

                    if (useSetting)
                    {
                        menu._menuItem.DropDown.Items.Add(new ToolStripSeparator());
                        var disableItem = new ToolStripMenuItem
                        {
                            Text = ResourceManager.Strings.DisableMenuItem
                        };

                        disableItem.Click += (o, s) =>
                        {
                            // Disables the dropdown in the current form only
                            // RevDiff will not be affected from other forms, requires a restart
                            // To overcome this limitation other forms would require a reference to RevDiff
                            AppSettings.ShowAvailableDiffTools = false;
                            InitMenus(menus);
                        };
                        menu._menuItem.DropDown.Items.Add(disableItem);
                    }
                }
            }).FileAndForget();
            return;

            static void InitMenus(IList<CustomDiffMergeTool> menus)
            {
                foreach (var menu in menus)
                {
                    menu._menuItem.DropDown = null;
                }
            }
        }
    }
}
