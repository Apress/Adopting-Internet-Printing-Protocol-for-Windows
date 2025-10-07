using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace BasicPsa
{
    public static class CommonDialogs
    {
        
        public static async void ShowMessage(string title, string content, [Optional] object[][] buttons)
        {
            MessageDialog dialog = new MessageDialog(content, title);

            // Sets the default cancel and default indexes to zero. (incase no buttons are passed)
            dialog.CancelCommandIndex = 0;
            dialog.DefaultCommandIndex = 0;

            // If the optional buttons array is not empty or null.
            if (buttons != null)
            {
                // If there's multiple buttons
                if (buttons.Length > 1)
                {
                    // Loops through the given buttons array
                    for (Int32 i = 0; i < buttons.Length; i++)
                    {
                        /* Assigns text and handler variables from the current index subarray.
                         * The first object at the currentindex should be a string and 
                         * the second object should be a "UICommandInvokedHandler" 
                         */
                        string text = (string)buttons[i][0];

                        UICommandInvokedHandler handler = (UICommandInvokedHandler)buttons[i][1];

                        /* Checks whether both variables types actually are relevant and correct.
                         * If not, it will return and terminate this function and not display anything.
                         */
                        if (handler.GetType().Equals(typeof(UICommandInvokedHandler)) &&
                            text.GetType().Equals(typeof(string)))
                        {
                            /* Creates a new "UICommand" instance which is required for
                             * adding multiple buttons.
                             */
                            UICommand button = new UICommand(text, handler);

                            // Simply adds the newly created button to the dialog
                            dialog.Commands.Add(button);
                        }
                        else return;
                    }
                }
                else
                {
                    // Already described
                    string text = (string)buttons[0][0];

                    UICommandInvokedHandler handler = (UICommandInvokedHandler)buttons[0][1];

                    // Already described
                    if (handler.GetType().Equals(typeof(UICommandInvokedHandler)) &&
                        text.GetType().Equals(typeof(string)))
                    {
                        // Already described
                        UICommand button = new UICommand(text, handler);

                        // Already described
                        dialog.Commands.Add(button);
                    }
                    else return;
                }

                /* Sets the default command index to the length of the button array.
                 * The first, colored button will become the default button or index.
                 */
                dialog.DefaultCommandIndex = (UInt32)buttons.Length;
            }

            await dialog.ShowAsync();
        }
    }
}
