/* *************************************** 
*           ModifyRegistry.cs
* ---------------------------------------
*         a very simple class 
*    to read, write, delete and count
*       registry values with C#
* ---------------------------------------
*      if you improve this code 
*   please email me your improvement!
* ---------------------------------------
*         by Francesco Natali
*        - fn.varie@libero.it -
* ***************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;

namespace TrialModePlugin
{
    class RegistryComponent
    {
        private RegistryKey BASE_REGISTRY_KEY = Registry.LocalMachine;
        private string SUB_KEY = "SOFTWARE\\";
        private bool ShowError = false;

        private RegistryComponent()
        {
        }

        public RegistryComponent(string SubKey, bool showError)
        {
            SUB_KEY = SubKey;
            ShowError = showError;
        }

        public string Read(string KeyName)
        {
            // Opening the registry key
            RegistryKey rk = BASE_REGISTRY_KEY;
            // Open a SUB_KEY as read-only
            RegistryKey sk1 = rk.OpenSubKey(SUB_KEY);
            // If the RegistrySUB_KEY doesn't exist -> (null)
            if (sk1 == null)
            {
                return null;
            }
            else
            {
                try
                {
                    // If the RegistryKey exists I get its value
                    // or null is returned.
                    string value = (string)sk1.GetValue(KeyName.ToUpper());
                    return value;
                }
                catch (Exception e)
                {
                    // AAAAAAAAAAARGH, an error!
                    ShowErrorMessage(e, "Reading registry " + KeyName.ToUpper());
                    return null;
                }
            }
        }

        public bool Write(string KeyName, object Value)
        {
            try
            {
                // Setting
                RegistryKey rk = BASE_REGISTRY_KEY;
                // I have to use CreateSubKey 
                // (create or open it if already exits), 
                // 'cause OpenSubKey open a SUB_KEY as read-only
                RegistryKey sk1 = rk.CreateSubKey(SUB_KEY);
                // Save the value
                sk1.SetValue(KeyName.ToUpper(), Value);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Writing registry " + KeyName.ToUpper());
                return false;
            }
        }

        public bool DeleteKey(string KeyName)
        {
            try
            {
                // Setting
                RegistryKey rk = BASE_REGISTRY_KEY;
                RegistryKey sk1 = rk.CreateSubKey(SUB_KEY);
                // If the RegistrySUB_KEY doesn't exists -> (true)
                if (sk1 == null)
                    return true;
                else
                    sk1.DeleteValue(KeyName);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Deleting SUB_KEY " + SUB_KEY);
                return false;
            }
        }

        public bool DeleteSubKeyTree()
        {
            try
            {
                // Setting
                RegistryKey rk = BASE_REGISTRY_KEY;
                RegistryKey sk1 = rk.OpenSubKey(SUB_KEY);
                // If the RegistryKey exists, I delete it
                if (sk1 != null)
                    rk.DeleteSubKeyTree(SUB_KEY);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Deleting SUB_KEY " + SUB_KEY);
                return false;
            }
        }

        public int SubKeyCount()
        {
            try
            {
                // Setting
                RegistryKey rk = BASE_REGISTRY_KEY;
                RegistryKey sk1 = rk.OpenSubKey(SUB_KEY);
                // If the RegistryKey exists...
                if (sk1 != null)
                    return sk1.SubKeyCount;
                else
                    return 0;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Retriving SUB_KEYs of " + SUB_KEY);
                return 0;
            }
        }

        public int ValueCount()
        {
            try
            {
                // Setting
                RegistryKey rk = BASE_REGISTRY_KEY;
                RegistryKey sk1 = rk.OpenSubKey(SUB_KEY);
                // If the RegistryKey exists...
                if (sk1 != null)
                    return sk1.ValueCount;
                else
                    return 0;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Retriving keys of " + SUB_KEY);
                return 0;
            }
        }

        private void ShowErrorMessage(Exception e, string Title)
        {
            if (ShowError == true)
                MessageBox.Show(e.Message,
                        Title
                        , MessageBoxButtons.OK
                        , MessageBoxIcon.Error);
        }
    }


}
