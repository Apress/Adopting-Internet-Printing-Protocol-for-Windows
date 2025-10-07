using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public static class Status
    {
        public enum IPP_STATUS      // IPP status code values
        {
            IPP_STATUS_CUPS_INVALID = -1,                       // Invalid status name for @link ippErrorValue@
            IPP_STATUS_OK = 0x0000,                             // successful-ok
            IPP_STATUS_OK_IGNORED_OR_SUBSTITUTED,               // successful-ok-ignored-or-substituted-attributes
            IPP_STATUS_OK_CONFLICTING,                          // successful-ok-conflicting-attributes
            IPP_STATUS_OK_IGNORED_SUBSCRIPTIONS,                // successful-ok-ignored-subscriptions
            IPP_STATUS_OK_IGNORED_NOTIFICATIONS,                // successful-ok-ignored-notifications @private@
            IPP_STATUS_OK_TOO_MANY_EVENTS,                      // successful-ok-too-many-events
            IPP_STATUS_OK_BUT_CANCEL_SUBSCRIPTION,              // successful-ok-but-cancel-subscription @private@
            IPP_STATUS_OK_EVENTS_COMPLETE,                      // successful-ok-events-complete
            IPP_STATUS_REDIRECTION_OTHER_SITE = 0x0200,         // redirection-other-site @private@
            IPP_STATUS_CUPS_SEE_OTHER = 0x0280,                 // cups-see-other @private@
            IPP_STATUS_ERROR_BAD_REQUEST = 0x0400,              // client-error-bad-request
            IPP_STATUS_ERROR_FORBIDDEN,                         // client-error-forbidden
            IPP_STATUS_ERROR_NOT_AUTHENTICATED,                 // client-error-not-authenticated
            IPP_STATUS_ERROR_NOT_AUTHORIZED,                    // client-error-not-authorized
            IPP_STATUS_ERROR_NOT_POSSIBLE,                      // client-error-not-possible
            IPP_STATUS_ERROR_TIMEOUT,                           // client-error-timeout
            IPP_STATUS_ERROR_NOT_FOUND,                         // client-error-not-found
            IPP_STATUS_ERROR_GONE,                              // client-error-gone
            IPP_STATUS_ERROR_REQUEST_ENTITY,                    // client-error-request-entity-too-large
            IPP_STATUS_ERROR_REQUEST_VALUE,                     // client-error-request-value-too-long
            IPP_STATUS_ERROR_DOCUMENT_FORMAT_NOT_SUPPORTED,     // client-error-document-format-not-supported
            IPP_STATUS_ERROR_ATTRIBUTES_OR_VALUES,              // client-error-attributes-or-values-not-supported
            IPP_STATUS_ERROR_URI_SCHEME,                        // client-error-uri-scheme-not-supported
            IPP_STATUS_ERROR_CHARSET,                           // client-error-charset-not-supported
            IPP_STATUS_ERROR_CONFLICTING,                       // client-error-conflicting-attributes
            IPP_STATUS_ERROR_COMPRESSION_NOT_SUPPORTED,         // client-error-compression-not-supported
            IPP_STATUS_ERROR_COMPRESSION_ERROR,                 // client-error-compression-error
            IPP_STATUS_ERROR_DOCUMENT_FORMAT_ERROR,             // client-error-document-format-error
            IPP_STATUS_ERROR_DOCUMENT_ACCESS,                   // client-error-document-access-error
            IPP_STATUS_ERROR_ATTRIBUTES_NOT_SETTABLE,           // client-error-attributes-not-settable
            IPP_STATUS_ERROR_IGNORED_ALL_SUBSCRIPTIONS,         // client-error-ignored-all-subscriptions
            IPP_STATUS_ERROR_TOO_MANY_SUBSCRIPTIONS,            // client-error-too-many-subscriptions
            IPP_STATUS_ERROR_IGNORED_ALL_NOTIFICATIONS,         // client-error-ignored-all-notifications @private@
            IPP_STATUS_ERROR_PRINT_SUPPORT_FILE_NOT_FOUND,      // client-error-print-support-file-not-found @private@
            IPP_STATUS_ERROR_DOCUMENT_PASSWORD,                 // client-error-document-password-error
            IPP_STATUS_ERROR_DOCUMENT_PERMISSION,               // client-error-document-permission-error
            IPP_STATUS_ERROR_DOCUMENT_SECURITY,                 // client-error-document-security-error
            IPP_STATUS_ERROR_DOCUMENT_UNPRINTABLE,              // client-error-document-unprintable-error
            IPP_STATUS_ERROR_ACCOUNT_INFO_NEEDED,               // client-error-account-info-needed
            IPP_STATUS_ERROR_ACCOUNT_CLOSED,                    // client-error-account-closed
            IPP_STATUS_ERROR_ACCOUNT_LIMIT_REACHED,             // client-error-account-limit-reached
            IPP_STATUS_ERROR_ACCOUNT_AUTHORIZATION_FAILED,      // client-error-account-authorization-failed
            IPP_STATUS_ERROR_NOT_FETCHABLE,                     // client-error-not-fetchable
            IPP_STATUS_ERROR_INTERNAL = 0x0500,                 // server-error-internal-error
            IPP_STATUS_ERROR_OPERATION_NOT_SUPPORTED,           // server-error-operation-not-supported
            IPP_STATUS_ERROR_SERVICE_UNAVAILABLE,               // server-error-service-unavailable
            IPP_STATUS_ERROR_VERSION_NOT_SUPPORTED,             // server-error-version-not-supported
            IPP_STATUS_ERROR_DEVICE,                            // server-error-device-error
            IPP_STATUS_ERROR_TEMPORARY,                         // server-error-temporary-error
            IPP_STATUS_ERROR_NOT_ACCEPTING_JOBS,                // server-error-not-accepting-jobs
            IPP_STATUS_ERROR_BUSY,                              // server-error-busy
            IPP_STATUS_ERROR_JOB_CANCELED,                      // server-error-job-canceled
            IPP_STATUS_ERROR_MULTIPLE_JOBS_NOT_SUPPORTED,       // server-error-multiple-document-jobs-not-supported
            IPP_STATUS_ERROR_PRINTER_IS_DEACTIVATED,            // server-error-printer-is-deactivated
            IPP_STATUS_ERROR_TOO_MANY_JOBS,                     // server-error-too-many-jobs
            IPP_STATUS_ERROR_TOO_MANY_DOCUMENTS,                // server-error-too-many-documents
            // These are internal and never sent over the wire...
            IPP_STATUS_ERROR_CUPS_AUTHENTICATION_CANCELED = 0x1000,// cups-authentication-canceled - Authentication canceled by user
            IPP_STATUS_ERROR_CUPS_PKI,                          // cups-pki-error - Error negotiating a secure connection
            IPP_STATUS_ERROR_CUPS_UPGRADE_REQUIRED              // cups-upgrade-required - TLS upgrade required
        };


        /// <summary>
        /// GetIppStatusFromException
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetIppStatusFromException(Exception ex)
        {
            if(ex.Message.Length > 0) 
            {
                string msg = ex.Message;
                int pos = msg.IndexOf("status code was: ");
                if(pos != -1)
                {
                    string sStatus = msg.Substring(pos + 17); 
                    if(sStatus.Length > 0)
                    {
                        try
                        {
                            int iStatus = Int32.Parse(sStatus);
                            return GetIppStatusMessage(iStatus);
                        }
                        catch(Exception)
                        {
                            return "Unable to translate exception";
                        }
                    }
                }
            }
            return "Unable to translate exception";
        }

        /// <summary>
        /// GetIppStatusMessage
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string GetIppStatusMessage(int status)
        {
            switch (status)
            {
                case (int)IPP_STATUS.IPP_STATUS_CUPS_INVALID:
                    return "Invalid status name for link";
                case (int)IPP_STATUS.IPP_STATUS_OK:
                    return "successful-ok";
                case (int)IPP_STATUS.IPP_STATUS_OK_IGNORED_OR_SUBSTITUTED:
                    return "successful-ok-ignored-or-substituted-attributes";
                case (int)IPP_STATUS.IPP_STATUS_OK_CONFLICTING:
                    return "successful-ok-conflicting-attributes";
                case (int)IPP_STATUS.IPP_STATUS_OK_IGNORED_SUBSCRIPTIONS:
                    return "successful-ok-ignored-subscriptions";
                case (int)IPP_STATUS.IPP_STATUS_OK_IGNORED_NOTIFICATIONS:
                    return "successful-ok-ignored-notifications @private@";
                case (int)IPP_STATUS.IPP_STATUS_OK_TOO_MANY_EVENTS:
                    return "successful-ok-too-many-events";
                case (int)IPP_STATUS.IPP_STATUS_OK_BUT_CANCEL_SUBSCRIPTION:
                    return "successful-ok-but-cancel-subscription @private@";
                case (int)IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE:
                    return "successful-ok-events-complete";
                case (int)IPP_STATUS.IPP_STATUS_REDIRECTION_OTHER_SITE:
                    return "redirection-other-site @private@";
                case (int)IPP_STATUS.IPP_STATUS_CUPS_SEE_OTHER:
                    return "cups-see-other @private@";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_BAD_REQUEST:
                    return "client-error-bad-request";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_FORBIDDEN:
                    return "client-error-forbidden";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_NOT_AUTHENTICATED:
                    return "client-error-not-authenticated";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_NOT_AUTHORIZED:
                    return "client-error-not-authorized";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_NOT_POSSIBLE:
                    return "client-error-not-possible";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_TIMEOUT:
                    return "client-error-timeout";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_NOT_FOUND:
                    return "client-error-not-found";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_GONE:
                    return "client-error-gone";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_REQUEST_ENTITY:
                    return "client-error-request-entity-too-large";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_REQUEST_VALUE:
                    return "client-error-request-value-too-long";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_DOCUMENT_FORMAT_NOT_SUPPORTED:
                    return "client-error-document-format-not-supported";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_ATTRIBUTES_OR_VALUES:
                    return "client-error-attributes-or-values-not-supported";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_URI_SCHEME:
                    return "client-error-uri-scheme-not-supported";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_CHARSET:
                    return "client-error-charset-not-supported";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_CONFLICTING:
                    return "client-error-conflicting-attributes";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_COMPRESSION_NOT_SUPPORTED:
                    return "client-error-compression-not-supported";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_COMPRESSION_ERROR:
                    return "client-error-compression-error";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_DOCUMENT_FORMAT_ERROR:
                    return "client-error-document-format-error";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_DOCUMENT_ACCESS:
                    return "client-error-document-access-error";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_ATTRIBUTES_NOT_SETTABLE:
                    return "client-error-attributes-not-settable";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_IGNORED_ALL_SUBSCRIPTIONS:
                    return "client-error-ignored-all-subscriptions";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_TOO_MANY_SUBSCRIPTIONS:
                    return "client-error-too-many-subscriptions";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_IGNORED_ALL_NOTIFICATIONS:
                    return "client-error-ignored-all-notifications @private@";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_PRINT_SUPPORT_FILE_NOT_FOUND:
                    return "client-error-print-support-file-not-found @private@";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_DOCUMENT_PASSWORD:
                    return "client-error-document-password-error";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_DOCUMENT_PERMISSION:
                    return "client-error-document-permission-error";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_DOCUMENT_SECURITY:
                    return "client-error-document-security-error";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_DOCUMENT_UNPRINTABLE:
                    return "client-error-document-unprintable-error";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_ACCOUNT_INFO_NEEDED:
                    return "client-error-account-info-needed";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_ACCOUNT_CLOSED:
                    return "client-error-account-closed";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_ACCOUNT_LIMIT_REACHED:
                    return "client-error-account-limit-reached";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_ACCOUNT_AUTHORIZATION_FAILED:
                    return "client-error-account-authorization-failed";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_NOT_FETCHABLE:
                    return "client-error-not-fetchable";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_INTERNAL:
                    return "server-error-internal-error";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_OPERATION_NOT_SUPPORTED:
                    return "server-error-operation-not-supported";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_SERVICE_UNAVAILABLE:
                    return "server-error-service-unavailable";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_VERSION_NOT_SUPPORTED:
                    return "server-error-version-not-supported";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_DEVICE:
                    return "server-error-device-error";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_TEMPORARY:
                    return "server-error-temporary-error";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_NOT_ACCEPTING_JOBS:
                    return "server-error-not-accepting-jobs";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_BUSY:
                    return "server-error-busy";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_JOB_CANCELED:
                    return "server-error-job-canceled";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_MULTIPLE_JOBS_NOT_SUPPORTED:
                    return "server-error-multiple-document-jobs-not-supported";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_PRINTER_IS_DEACTIVATED:
                    return "server-error-printer-is-deactivated";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_TOO_MANY_JOBS:
                    return "server-error-too-many-jobs";
                case (int)IPP_STATUS.IPP_STATUS_ERROR_TOO_MANY_DOCUMENTS:
                    return "server-error-too-many-documents";
                default:
                    return "unknown error";
            }

        }
        
    }
}
