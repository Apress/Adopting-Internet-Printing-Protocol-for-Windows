using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IppCheck
{
    public static class RegSettings
    {
        private const string INIT_REG_PATH = @"Software\IppCheck";

        /// <summary>
        /// TestForApplicationSubKey
        /// </summary>
        /// <returns></returns>
        public static bool TestForApplicationSubKey()
        {
            string regKey = INIT_REG_PATH;
            using (Microsoft.Win32.RegistryKey rKey = Registry.CurrentUser.OpenSubKey(regKey))
            {
                if (rKey == null)
                {
                    RegistryKey rk = Registry.CurrentUser.CreateSubKey(regKey);
                    {
                        if (regKey != null)
                            return true;
                    }
                }
                else
                {
                    //key exists 
                    return true;
                }
             
            }
            return false;
        }

        /// <summary>
        /// GetRegistryStringValue
        /// </summary>
        /// <param name="sValue"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetRegistryStringValue(string sValue)
        {
            string regKey = INIT_REG_PATH;
            string regValue = null;
            using (Microsoft.Win32.RegistryKey rKey = Registry.CurrentUser.OpenSubKey(regKey))
            {
                if (rKey != null)
                {
                    try
                    {
                        object o = rKey.GetValue(sValue, "", RegistryValueOptions.None);
                        if(o != null) 
                        { 
                            if(o.ToString() == "")
                            {
                                throw new Exception($"The registry Name/Value pair: {sValue} does not exist..");
                            }
                            regValue = o.ToString();
                        }
                        else
                        {
                            throw new Exception(" null value returned.");
                        }
                    }
                    catch(Exception ex)
                    {
                        string errorString = string.Format("Error retrieving reg key {0} value, reason: {1}", regKey, ex.Message);
                        throw new Exception(errorString);
                    }
                }
            }
            return regValue;
        }

        /// <summary>
        /// SetRegistryStringValue
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sValue"></param>
        /// <exception cref="Exception"></exception>
        public static void SetRegistryStringValue(string key, string sValue)
        {
            string regKey = INIT_REG_PATH;
            using (Microsoft.Win32.RegistryKey rKey = Registry.CurrentUser.OpenSubKey(regKey, true))
            {
                if (rKey != null)
                {
                    try
                    {
                        rKey.SetValue(key, sValue, RegistryValueKind.String);
                    }
                    catch (Exception ex)
                    {
                        string errorString = string.Format("Error setting reg key {0} to value {1}, reason: {2}", regKey, sValue, ex.Message);
                        throw new Exception(errorString);
                    }
                }
            }
        }

        /// <summary>
        /// GetLocalMachineRegistryStringValue
        /// </summary>
        /// <param name="sPath"></param>
        /// <param name="sKeyName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetLocalMachineRegistryStringValue(string sPath, string sKeyName)
        {
            string regValue = null;
            using (Microsoft.Win32.RegistryKey rKey = Registry.LocalMachine.OpenSubKey(sPath, false))
            {
                if (rKey != null)
                {
                    try
                    {
                        object o = rKey.GetValue(sKeyName, "", RegistryValueOptions.None);
                        if (o != null)
                        {
                            regValue = o.ToString();
                        }
                        else
                        {
                            throw new Exception(" null value returned.");
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorString = string.Format("Error retrieving reg key {0} value, reason: {1}", sKeyName, ex.Message);
                        throw new Exception(errorString);
                    }
                }
            }
            return regValue;
        }


    }
}
