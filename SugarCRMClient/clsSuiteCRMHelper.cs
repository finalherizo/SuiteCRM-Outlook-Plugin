﻿/**
 * Outlook integration for SuiteCRM.
 * @package Outlook integration for SuiteCRM
 * @copyright SalesAgility Ltd http://www.salesagility.com
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU AFFERO GENERAL PUBLIC LICENSE as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU AFFERO GENERAL PUBLIC LICENSE
 * along with this program; if not, see http://www.gnu.org/licenses
 * or write to the Free Software Foundation,Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA 02110-1301  USA
 *
 * @author SalesAgility <info@salesagility.com>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SuiteCRMClient
{
    using RESTObjects;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Windows.Forms;
    
    public static class clsSuiteCRMHelper
    {
        public static string InstallationPath { get; set; }
     
        public static clsUsersession SuiteCRMUserSession;

        public static eModuleList GetModules()
        {
            string strUserID = clsSuiteCRMHelper.GetUserId();
            if (strUserID == "")
            {
                SuiteCRMUserSession.Login();
            }
            object data = new
            {
                @session = SuiteCRMUserSession.id
            };
            return clsGlobals.GetResponse<eModuleList>("get_available_modules", data);            
        }
                

        public static string GetUserId()
        {
            try
            {
                string userId = "";
                object data = new
                {
                    @session = SuiteCRMUserSession.id
                };
                userId = clsGlobals.GetResponse<string>("get_user_id", data);
                return userId;
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                return "";
            }
        }

        public static string SetEntry(eNameValue[] Data, string ModuleName = "Emails")
        {
            try
            {
                string strUserID = clsSuiteCRMHelper.GetUserId();
                if (strUserID == "")
                {
                    SuiteCRMUserSession.Login();
                }
                object data = new
                {
                    @session = SuiteCRMUserSession.id,
                    @module_name = ModuleName,
                    @name_value_list = Data
                };
                eSetEntryResult _result = clsGlobals.GetResponse<eSetEntryResult>("set_entry", data);  
                                
                return _result.id.ToString();
            }
            catch (System.Exception exception)
            {
                exception.Data.Clear();
                return string.Empty;
            }
        }

        public static string getRelationship(string MainModule, string ID, string ModuleToFind)
        {
            try
            {
                string strUserID = clsSuiteCRMHelper.GetUserId();
                if (strUserID == "")
                {
                    SuiteCRMUserSession.Login();
                }
                object data = new
                {
                    @session = SuiteCRMUserSession.id,
                    @module_name = MainModule,
                    @module_id = ID,
                    @link_field_name = ModuleToFind,
                    @related_module_query = "",
                    @related_fields = new string[] { "id" }/*,
                    @query = ""
                    //@limit = 1*/
                };
                eGetRelationshipResult _result = clsGlobals.GetResponse<eGetRelationshipResult>("get_relationships", data);
                if (_result.entry_list.Length > 0)
                    return _result.entry_list[0].id;
                return "";
            }
            catch (System.Exception exception)
            {
                exception.Data.Clear();
                return "";
            }            
        }

        public static eEntryValue[] getRelationships(string MainModule, string ID, string ModuleToFind, string[] fields)
        {
            try
            {
                string strUserID = clsSuiteCRMHelper.GetUserId();
                if (strUserID == "")
                {
                    SuiteCRMUserSession.Login();
                }
                object data = new
                {
                    @session = SuiteCRMUserSession.id,
                    @module_name = MainModule,
                    @module_id = ID,
                    @link_field_name = ModuleToFind,
                    @related_module_query = "",
                    @related_fields = fields/*,
                    @query = ""
                    //@limit = 1*/
                };
                eGetRelationshipResult _result = clsGlobals.GetResponse<eGetRelationshipResult>("get_relationships", data);
                if (_result.entry_list.Length > 0)
                    return _result.entry_list;
                return null;
            }
            catch (System.Exception exception)
            {
                exception.Data.Clear();
                return null;
            }
        }

        public static bool SetRelationship(eSetRelationshipValue info)
        {
            try
            {
                string strUserID = clsSuiteCRMHelper.GetUserId();
                if (strUserID == "")
                {
                    SuiteCRMUserSession.Login();
                }
                object data = new
                {
                    @session = SuiteCRMUserSession.id,
                    @module_name = info.module1,
                    @module_id = info.module1_id,
                    @link_field_name = info.module2,
                    @related_ids = new string[] { info.module2_id }
                };
                var _value = clsGlobals.GetResponse<RESTObjects.eNewSetRelationshipListResult>("set_relationship", data);
                if (_value.Created==0)
                {
                    return false;
                }
            }
            catch (System.Exception exception)
            {
                exception.Data.Clear();
                return false;
            }
            return true;
        }

        public static bool UploadAttahcment(clsEmailAttachments objAttachment, string email_id)
        {
            string strUserID = clsSuiteCRMHelper.GetUserId();
            if (strUserID == "")
            {
                SuiteCRMUserSession.Login();
            }
            //Initialize AddIn attachment
            List<RESTObjects.eNameValue> initNoteData = new List<RESTObjects.eNameValue>();
            initNoteData.Add(new RESTObjects.eNameValue() { name = "name", value = objAttachment.DisplayName });

            object initNoteDataWebFormat = new
            {
                @session = SuiteCRMUserSession.id,
                @module_name = "Notes",
                @name_value_list = initNoteData
            };
            var res = clsGlobals.GetResponse<RESTObjects.eNewSetEntryResult>("set_entry", initNoteDataWebFormat);

            //upload the attachment  
            RESTObjects.eNewNoteAttachment attachment = new RESTObjects.eNewNoteAttachment();
            attachment.ID = res.id;
            attachment.FileName = objAttachment.DisplayName;
            attachment.FileCotent = objAttachment.FileContentInBase64String;

            object attachmentDataWebFormat = new
            {
                @session = SuiteCRMUserSession.id,
                @note = attachment
            };

            var attachmentResult = clsGlobals.GetResponse<RESTObjects.eNewSetEntryResult>("set_note_attachment", attachmentDataWebFormat);

            //Relate the email and the attachment
            object contacRelationshipData = new
            {
                @session = SuiteCRMUserSession.id,
                @module_name = "Emails",
                @module_id = email_id,
                @link_field_name = "notes",
                @related_ids = new string[] { attachmentResult.id }
            };
            var rel = clsGlobals.GetResponse<RESTObjects.eNewSetRelationshipListResult>("set_relationship", contacRelationshipData);

            if (rel.Created == 0)
            {
                return false;
            }
            return true;
        }
        public static eNameValue SetNameValuePair(string name, string value)
        {
            return new eNameValue { name = name, value = value };
        }       

        public static string GetAttendeeList(string id)
        {
            string strUserID = clsSuiteCRMHelper.GetUserId();
            if (strUserID == "")
            {
                SuiteCRMUserSession.Login();
            }
            string _result = "";
            try
            {
                object data = new
                {
                    @session = SuiteCRMUserSession.id,
                    @module_name = "Meetings",
                    @module_id = id,
                    @link_field_name = "contacts",
                    @related_fields = new string[] { "email1" }
                    /*,
                    @related_module_link_name_to_fields_array = new object[] {new object[]{
                        new {@name = "employees", @value=new string[]{"email1"}}
                    } }*/
                };
                _result = clsGlobals.GetResponse<string>("get_relationships", data);                
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            return _result;
        }
        
        public static eGetEntryListResult GetEntryList(string module, string query, int limit, string order_by, int offset, bool GetDeleted, string[] fields)
        {
            string strUserID = clsSuiteCRMHelper.GetUserId();
            if (strUserID == "")
            {
                SuiteCRMUserSession.Login();
            } 
            eGetEntryListResult _result = new eGetEntryListResult();
            try
            {
                object data = new
                {
                    @session = SuiteCRMUserSession.id,
                    @module_name = module,
                    @query = query,
                    @order_by = order_by,
                    @offset = offset,
                    @select_fields = fields,
                    @max_results = limit,
                    @deleted = Convert.ToInt32(GetDeleted)
                };
                _result = clsGlobals.GetResponse<RESTObjects.eGetEntryListResult>("get_entry_list", data);                
                if (_result.error != null)
                {
                    throw new Exception(_result.error.description);                    
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            try
            {
                Hashtable hashtable = new Hashtable();
                int index = 0;
                foreach (eEntryValue _value in _result.entry_list)
                {
                    if (!hashtable.Contains(_value.id))
                    {
                        hashtable.Add(_value.id, _value);
                    }
                    _result.entry_list[index] = null;
                    index++;
                }
                int num2 = 0;
                _result.entry_list = null;
                _result.entry_list = new eEntryValue[hashtable.Count];
                _result.result_count = hashtable.Count;
                foreach (DictionaryEntry entry in hashtable)
                {
                    _result.entry_list[num2] = (eEntryValue)entry.Value;
                    num2++;
                }
            }
            catch (System.Exception)
            {
                _result.result_count = 0;
            }
            return _result;
        }
        public static string GetValueByKey(eEntryValue entry, string key)
        {
            string str = string.Empty;
            foreach (eNameValue _value in entry.name_value_list1)
            {
                if (_value.name == key)
                {
                    str = _value.value;
                }
            }
            return str;
        }

        public static List<string> GetFields(string module)
        {
            string strUserID = clsSuiteCRMHelper.GetUserId();
            if (strUserID == "")
            {
                SuiteCRMUserSession.Login();
            }
            List<string> list = new List<string>();
            if (module == null)
                return list;

            object data = new
            {
                @session = SuiteCRMUserSession.id,
                @module_name = module
            };
            foreach (eField field in clsGlobals.GetResponse<eModuleFields>("get_module_fields", data).module_fields1)
            {
                list.Add(field.name);
            }
            return list;
        }

        public static string[] GetSugarFields(string module)
        {
            string[] strArray = new string[14];
            if (module == null)
            {
                return strArray;
            }
            if (module == "Contacts")
            {
                return new string[] { 
                    "id", "first_name", "last_name", "email1", "phone_work", "phone_home", "title", "department", "primary_address_city", "primary_address_country", "primary_address_postalcode", "primary_address_state", "primary_address_street", "description", "user_sync", "date_modified", 
                    "account_name", "phone_mobile", "phone_fax", "salutation"
                 };
            }
            if (module == "Tasks")
            {
                return new string[] { "id", "name", "description", "date_due", "status", "date_modified", "date_start", "priority" };
            }
            if (module == "Meetings")
            {
                return new string[] { "id", "name", "description", "date_start", "date_end", "location", "date_modified", "duration_minutes", "duration_hours", "invitees" };
            }
            if (module == "Calls")
            {
                return new string[] { "id", "name", "description", "date_start", "date_end", "date_modified", "duration_minutes", "duration_hours" };
            }
            return strArray;
        }

        public static eSetEntryResult SetAccountsEntry(eNameValue[] Data)
        {
            string strUserID = clsSuiteCRMHelper.GetUserId();
            if (strUserID == "")
            {
                SuiteCRMUserSession.Login();
            }
            object data = new
            {
                @session = SuiteCRMUserSession.id,
                @module_name = "Accounts",
                @name_value_list = Data
            };
            eSetEntryResult _result = clsGlobals.GetResponse<eSetEntryResult>("set_entry", data);
            return _result;
      
        }
        public static eSetEntryResult SetOpportunitiesEntry(eNameValue[] Data)
        {
            string strUserID = clsSuiteCRMHelper.GetUserId();
            if (strUserID == "")
            {
                SuiteCRMUserSession.Login();
            }
            object data = new
            {
                @session = SuiteCRMUserSession.id,
                @module_name = "Opportunities",
                @name_value_list = Data
            };
            eSetEntryResult _result = clsGlobals.GetResponse<eSetEntryResult>("set_entry", data);
            return _result;

        }

        public static Hashtable FindAccounts(string val)
        {
            Hashtable hashtable = new Hashtable();
            string query = "accounts.name LIKE '" + val + "%'";
            eGetEntryListResult _result = GetEntryList("Accounts", query, 200, "date_entered DESC", 0, false, new string[] { "name", "id" });
            if (_result.result_count > 0)
            {
                foreach (eEntryValue _value in _result.entry_list)
                {
                    string valueByKey = string.Empty;
                    string key = string.Empty;
                    valueByKey = GetValueByKey(_value, "name");
                    key = GetValueByKey(_value, "id");
                    hashtable.Add(key, valueByKey);
                }
            }
            return hashtable;
        }
       
        public static void WriteException(Exception ex, string sMethodName)
        {
            try
            {
                string strLog;
                strLog = "------------------" + System.DateTime.Now.ToString() + "-----------------\n";
                strLog += "Method:" + sMethodName + "\n";
                strLog += "Message:" + ex.Message + "\n";
                strLog += "Source:" + ex.Source + "\n";
                strLog += "StackTrace:" + ex.StackTrace + "\n";
                strLog += "Data:" + ex.Data.ToString() + "\n";
                strLog += "HResult:" + ex.HResult.ToString() + "\n";
                strLog += "-------------------------------------------------------------------------" + "\n";
                clsSuiteCRMHelper.WriteLog(strLog);
                ex.Data.Clear();
            }
            catch { }

        }

        public static void WriteLog(string strLog)
        {
            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;
            
            string logFilePath = clsSuiteCRMHelper.InstallationPath + "\\Logs\\";
            logFilePath = logFilePath + "Log-" + System.DateTime.Today.ToString("MM-dd-yyyy") + "." + "txt";            
            logFileInfo = new FileInfo(logFilePath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            log = new StreamWriter(fileStream);
            log.WriteLine(strLog);
            log.Close();
        }        
    }
}
