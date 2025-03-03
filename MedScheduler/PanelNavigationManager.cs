using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MedScheduler
{
    class PanelNavigationManager
    {
        private Form parentForm;
        private Dictionary<string, Panel> screens = new Dictionary<string, Panel>();
        private string currentScreenName;

        public PanelNavigationManager(Form form)
        {
            parentForm = form;
        }

        // Register a panel as a screen
        public void RegisterScreen(string screenName, Panel panel)
        {
            if (!screens.ContainsKey(screenName))
            {
                screens.Add(screenName, panel);
                panel.Dock = DockStyle.Fill; // Make panel fill its container
                panel.Visible = false; // Hide all panels initially
            }
        }

        // Navigate to a specific screen
        public void NavigateTo(string screenName)
        {
            if (!screens.ContainsKey(screenName))
            {
                throw new ArgumentException($"Screen '{screenName}' is not registered.");
            }

            // Hide all screens
            foreach (var screen in screens.Values)
            {
                screen.Visible = false;
            }

            // Show the requested screen
            screens[screenName].Visible = true;
            currentScreenName = screenName;
        }

        // Go back to the previous screen
        public string GetCurrentScreenName()
        {
            return currentScreenName;
        }
    }
}
